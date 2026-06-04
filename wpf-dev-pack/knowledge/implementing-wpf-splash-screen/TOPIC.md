# Implementing a WPF Splash Screen on a Dedicated STA Thread

> Hosts a WPF splash screen on a dedicated STA thread with its own Dispatcher and performs a cross-thread foreground handoff to MainWindow. Use whenever the splash must remain animated and responsive while App.OnStartup, OnInitialized, license dialogs, DI bootstrap, or other long synchronous work blocks the main UI thread, and the WPF built-in static-image SplashScreen build action is not enough. Covers the strict Activate-before-Close ordering required for Win32 SetForegroundWindow to grant foreground to MainWindow, why Window.Owner cannot cross thread boundaries, why the splash AXAML must not depend on App.xaml ResourceDictionary entries because of cross-thread Freezable rules, idempotent Close(), and lock-free Interlocked.CompareExchange sentinel coordination between Show and Close. Apply alongside threading-wpf-dispatcher and shutting-down-wpf-gracefully — the latter covers the dispatcher shutdown race that fires when OnExit disposes the splash mid-fade.

> **Related foundation skills**:
> [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/SKILL.md) explains why each Dispatcher belongs to exactly one STA thread.
> [`shutting-down-wpf-gracefully`](../shutting-down-wpf-gracefully/SKILL.md) covers the dispatcher shutdown race that surfaces when `App.OnExit` disposes the splash mid-fade.

---

## 1. Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read this section.

1. **`Window.Owner` cannot cross threads.** The splash lives on its own STA thread with its own `Dispatcher`. `splash.Owner = Application.Current.MainWindow` throws `InvalidOperationException` because `MainWindow` belongs to a different `Dispatcher`. Do not set `Owner` at all — coordinate foreground via Activate instead.
2. **Activate BEFORE Close, never after.** On the splash thread, inside the fade-out `Storyboard.Completed` handler, marshal to the main `Dispatcher` and call `Application.Current.MainWindow?.Activate()` first, **then** call `splash.Close()` on the splash thread. If you reverse the order, Windows denies `SetForegroundWindow` (the splash was the foreground HWND, its process loses foreground-grant the instant it disappears) and the result is a taskbar flash with the previously foreground app — typically the user's browser — still painted on top of `MainWindow`.
3. **Splash AXAML must not reference `App.xaml` resources.** `App.xaml`'s `Application.Resources` (and merged dictionaries like WPF-UI `ThemesDictionary`) live on the main UI Dispatcher. Loading them from the splash thread tries to share unfrozen `Freezable` instances across threads and throws `InvalidOperationException` ("must have the same ThreadAffinity as the rest of the object tree"). Use inline hex `#AARRGGBB` colors and basic primitives (`Border`, `Grid`, `StackPanel`, `Image`, `TextBlock`, `TranslateTransform`). Pack URI image resolution (`pack://application:,,,/...`) is thread-safe and remains allowed.
4. **`Close()` must be idempotent.** `App.OnInitialized` (or the bootstrap finalizer) and `MainWindow.ContentRendered` both call `Close()` as a safety net. The second call must return silently — never throw, never re-run the fade.
5. **Use a lock-free 3-state sentinel for the Show↔Close race.** If `Close()` is called before `Show()` has finished mounting the window, the close must still register and run as soon as the splash is mounted. Reserve three reference values in one `object?` field — `null` (initial), a captured close callback, and a static `ReadySentinel` marker — and switch between them with `Interlocked.CompareExchange<object?>`. `ManualResetEventSlim` or `TaskCompletionSource<bool>` also works but adds an extra waitable handle that cold-startup paths do not need.

---

## 2. When to Use This Skill

Use this skill when **all** of the following are true:

| Condition | Why |
|---|---|
| The splash must keep animating while the main thread is blocked | A splash on the main UI thread freezes during synchronous bootstrap work |
| Startup contains synchronous work that cannot be made async (third-party initializers, license dialogs, DI container build, plugin scan) | Async/await on the main thread cannot rescue work that does not expose a Task |
| The splash needs vector content, a fade-out animation, or progress text | The built-in `SplashScreen` build action only renders a static image |
| The splash must hand foreground over to `MainWindow` smoothly | Otherwise users see the previous foreground app reappear |

### When NOT to use this skill

| Scenario | Use Instead |
|---|---|
| A single static PNG/JPG is acceptable, no fade, no animation, no progress text | WPF's built-in `SplashScreen` (set image `Build Action` to `SplashScreen`) — loads via Win32 before WPF starts, costs near zero |
| All startup work is already async and the main UI thread is responsive | Show a normal `Window` on the main Dispatcher |
| The splash must be modal over the main window | This skill is for the opposite case (splash precedes the main window). Use a regular modal dialog after the main window is shown. |

---

## 3. Architecture

### 3.1 Components

| Component | Thread | Lifetime |
|---|---|---|
| `ISplashScreenService` | Any | Singleton in DI container |
| `SplashScreenService : ISplashScreenService, IDisposable` | Owns its own STA worker thread | Singleton |
| Splash `Window` (e.g., `SplashWindow`) | Splash STA thread | Created and destroyed by the service |
| Splash `Dispatcher` | Splash STA thread | Runs `Dispatcher.Run()` until shutdown |
| `App` / bootstrap code | Main UI thread | Calls `Show()` early, schedules `Close()` |

### 3.2 Control flow

```
App.OnStartup
    │
    └─► ISplashScreenService.Show()
            │
            ├─► spawn STA Thread (the "splash thread")
            ├─► (on splash thread) construct SplashWindow
            ├─► (on splash thread) splash.Show()
            ├─► (on splash thread) Dispatcher.Run()  // pumps until shutdown
            └─► (returns immediately on main thread)

[main thread continues bootstrap: DI build, license dialog, MainWindow.Show()]

App.OnInitialized  OR  MainWindow.ContentRendered
    │
    └─► ISplashScreenService.Close()
            │
            ├─► (on splash thread) start fade-out Storyboard
            │
            └─► Storyboard.Completed (splash thread)
                    │
                    ├─► Application.Current.Dispatcher.Invoke(
                    │       () => Application.Current.MainWindow?.Activate())
                    │   // (1) Activate MainWindow on the main thread FIRST
                    │
                    ├─► splash.Close()                  // (2) THEN close splash
                    └─► splashDispatcher.InvokeShutdown()
```

---

## 4. Reference Implementation

### 4.1 Service contract

```csharp
namespace MyApp.Services;

public interface ISplashScreenService
{
    /// <summary>
    /// Shows the splash on a dedicated STA thread. Returns once the window is mounted.
    /// </summary>
    void Show();

    /// <summary>
    /// Schedules the fade-out and close. Idempotent — safe to call twice.
    /// Coordinates the cross-thread foreground handoff to MainWindow.
    /// </summary>
    void Close();
}
```

### 4.2 Splash service

```csharp
namespace MyApp.Services;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

public sealed class SplashScreenService : ISplashScreenService, IDisposable
{
    // Sentinel value that means "Show() has finished and the window is mounted".
    private static readonly object ReadySentinel = new();

    // Tri-state slot:
    //   null            → Show() has not finished yet AND no close has been requested
    //   Action          → a close callback was registered before the window was ready
    //   ReadySentinel   → the window is mounted; Close() can fire immediately
    private object? _state;

    private Thread? _splashThread;
    private Dispatcher? _splashDispatcher;
    private SplashWindow? _splashWindow;
    private int _closed;     // 0 = open, 1 = close already scheduled (idempotent guard)
    private int _disposed;   // 0 = live, 1 = disposed

    public void Show()
    {
        if (_splashThread is not null)
        {
            return; // Show() is itself idempotent
        }

        var ready = new ManualResetEventSlim(false);

        _splashThread = new Thread(() =>
        {
            _splashDispatcher = Dispatcher.CurrentDispatcher;
            _splashWindow = new SplashWindow();
            _splashWindow.Show();

            // Publish the "ready" state. If a Close() arrived first, it left an
            // Action in _state; run it now and skip the sentinel write.
            var previous = Interlocked.CompareExchange(ref _state, ReadySentinel, null);
            if (previous is Action pending)
            {
                // A Close() raced ahead of us. Honor it now on the splash thread.
                pending();
            }

            ready.Set();
            Dispatcher.Run(); // pumps until InvokeShutdown
        })
        {
            IsBackground = true,
            Name = "SplashScreenThread",
        };

        _splashThread.SetApartmentState(ApartmentState.STA);
        _splashThread.Start();

        // Block briefly so callers see a fully mounted splash before bootstrap work.
        ready.Wait();
    }

    public void Close()
    {
        if (Interlocked.Exchange(ref _closed, 1) == 1)
        {
            return; // Idempotent — second caller wins nothing
        }

        Action closeCallback = ScheduleFadeAndClose;

        // If the window is already mounted, run the callback now.
        // Otherwise, park the callback in the slot; Show() will execute it on mount.
        var previous = Interlocked.CompareExchange(ref _state, closeCallback, null);
        if (ReferenceEquals(previous, ReadySentinel))
        {
            closeCallback();
        }
    }

    private void ScheduleFadeAndClose()
    {
        var dispatcher = _splashDispatcher;
        if (dispatcher is null || dispatcher.HasShutdownStarted)
        {
            return;
        }

        dispatcher.BeginInvoke(new Action(() =>
        {
            var window = _splashWindow;
            if (window is null)
            {
                return;
            }

            var fade = new DoubleAnimation(
                fromValue: 1.0,
                toValue: 0.0,
                duration: new Duration(TimeSpan.FromMilliseconds(220)));

            fade.Completed += OnFadeCompleted;
            window.BeginAnimation(UIElement.OpacityProperty, fade);
        }));
    }

    private void OnFadeCompleted(object? sender, EventArgs e)
    {
        // (1) Foreground handoff FIRST — on the MAIN Dispatcher.
        //     Win32 SetForegroundWindow only grants foreground while our process
        //     still owns the current foreground HWND (the splash). If we close
        //     the splash first, the foreground-grant lapses and Activate() falls
        //     back to a taskbar flash, leaving the previous foreground app on top.
        var app = Application.Current;
        app?.Dispatcher.Invoke(() => app.MainWindow?.Activate());

        // (2) Close the splash AFTER the main window has been activated.
        _splashWindow?.Close();
        _splashWindow = null;

        // (3) Tear down the splash Dispatcher so the splash thread can exit.
        _splashDispatcher?.InvokeShutdown();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        Close();
        _splashThread?.Join(TimeSpan.FromSeconds(2));
    }
}
```

### 4.3 Splash AXAML (no `App.xaml` resource dependency)

```xml
<Window x:Class="MyApp.Services.SplashWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        ShowInTaskbar="False"
        Topmost="True"
        WindowStartupLocation="CenterScreen"
        Width="520" Height="320">
    <!--
        Inline colors only. Do NOT MergedDictionaries any resource from App.xaml
        or any third-party theme dictionary — they live on a different Dispatcher
        and the Freezable system rejects cross-thread sharing of unfrozen instances.
    -->
    <Border CornerRadius="12"
            Background="#FF1E1E28"
            BorderBrush="#FF2A2A36"
            BorderThickness="1">
        <Grid>
            <StackPanel HorizontalAlignment="Center"
                        VerticalAlignment="Center">
                <Image Source="pack://application:,,,/Assets/AppLogo.png"
                       Width="96" Height="96" />
                <TextBlock Text="Loading…"
                           Foreground="#FFE6E6F0"
                           FontSize="18"
                           HorizontalAlignment="Center"
                           Margin="0,16,0,0">
                    <TextBlock.RenderTransform>
                        <TranslateTransform Y="0" />
                    </TextBlock.RenderTransform>
                </TextBlock>
            </StackPanel>
        </Grid>
    </Border>
</Window>
```

```csharp
namespace MyApp.Services;

using System.Windows;

public sealed partial class SplashWindow : Window
{
    public SplashWindow() => InitializeComponent();
}
```

---

## 5. Bootstrap Integration

### 5.1 `App.OnStartup` — show as early as possible

```csharp
namespace MyApp;

using System.Windows;
using MyApp.Services;

public sealed partial class App : Application
{
    private readonly ISplashScreenService _splash = new SplashScreenService();

    protected override void OnStartup(StartupEventArgs e)
    {
        _splash.Show();   // before DI build, before any heavy import
        base.OnStartup(e);
        // ... heavy synchronous bootstrap continues here ...
    }
}
```

### 5.2 Schedule a fallback close — both `OnInitialized` and `ContentRendered`

```csharp
protected override void OnInitialized(EventArgs e)
{
    base.OnInitialized(e);

    if (MainWindow is { } main)
    {
        // Fires only after MainWindow has rendered its first frame.
        // The splash is closed here as a safety net even if no explicit
        // call site invoked Close() during bootstrap.
        main.ContentRendered += OnMainWindowContentRendered;
    }
}

private void OnMainWindowContentRendered(object? sender, EventArgs e)
{
    if (sender is MainWindow main)
    {
        main.ContentRendered -= OnMainWindowContentRendered;
    }
    _splash.Close();   // idempotent — safe even if a license dialog already closed it
}
```

### 5.3 Branch closes — license / pre-flight dialogs

If a user-driven dialog appears **before** `MainWindow` is shown (license consent, login, region picker), close the splash before showing it so the dialog comes up as the foreground window:

```csharp
private bool TryAcceptLicense()
{
    _splash.Close();   // dialog must be foreground; splash would steal it back

    var dialog = new LicenseDialog();
    return dialog.ShowDialog() == true;
}
```

---

## 6. Pitfall Table

| Pitfall | Symptom | Fix |
|---|---|---|
| `splash.Owner = Application.Current.MainWindow` | `InvalidOperationException`: "The calling thread cannot access this object because a different thread owns it." | Do not set `Owner`. Coordinate foreground via `Window.Activate()` on the main Dispatcher. |
| `splash.Close()` runs before `MainWindow.Activate()` | Taskbar icon flashes orange; the previously foreground app (browser, IDE) stays painted on top of MainWindow. | Activate first on the main Dispatcher (`app.Dispatcher.Invoke(...)`), then close the splash. |
| Splash AXAML merges `App.xaml` resources / WPF-UI `ThemesDictionary` | `InvalidOperationException` from `Freezable` — cross-thread sharing of unfrozen Brush/Color. | Inline `#AARRGGBB` colors. Pack URI images are fine. If you must share a resource, freeze it on the originating thread first or define it inside the splash itself. |
| `Close()` is called from both `OnInitialized` and `ContentRendered` | Second call throws because the dispatcher already shut down, or animates the fade twice. | Guard the body with `Interlocked.Exchange(ref _closed, 1) == 1` to make `Close()` idempotent. |
| `OnExit → Dispose() → fade animation` runs while the main Dispatcher is already shutting down | `TaskCanceledException` or no-op fade; sometimes a deadlock waiting on the join. | Skip the fade when `Application.Current?.Dispatcher.HasShutdownStarted == true`. See [`shutting-down-wpf-gracefully`](../shutting-down-wpf-gracefully/SKILL.md) for the broader pattern. |
| `Show()` returns before the window is mounted, then `Close()` arrives | Close becomes a no-op; the splash remains until process exit. | Block `Show()` on `ManualResetEventSlim.Wait()` (≤ ~50 ms) **and** use the `Interlocked.CompareExchange` sentinel to fire a queued close when Show finishes mounting. |
| Calling `_splashDispatcher.Invoke(...)` after `InvokeShutdown` has already started | `TaskCanceledException`. | Guard every cross-thread call with `dispatcher.HasShutdownStarted` and use `BeginInvoke` (fire-and-forget) instead of `Invoke` from background paths. |
| `Thread` created without `SetApartmentState(ApartmentState.STA)` | `InvalidOperationException` when constructing `SplashWindow`. | Set apartment state to `STA` **before** `Thread.Start`. |
| Reusing the splash service after `Dispose()` | `ObjectDisposedException` (or worse — silent corruption). | Register the service as a true singleton; do not call `Show()` after `Dispose()`. |

---

## 7. Verifying the Foreground Handoff

A correct implementation produces this sequence of HWND messages on close:

1. `WM_ACTIVATEAPP(1, ...)` fires on the MainWindow HWND **before** the splash receives `WM_CLOSE`.
2. `Application.Current.MainWindow.IsActive` reports `true` on the main Dispatcher inside the `Completed` handler.
3. The splash's `WM_DESTROY` arrives after MainWindow is already foreground.

If you observe the splash's `WM_DESTROY` first (e.g., with Spy++), the order is reversed — fix it before shipping.

---

## 8. Related Skills

| Skill | Relationship |
|---|---|
| [`managing-wpf-application-lifecycle`](../managing-wpf-application-lifecycle/SKILL.md) | Where to wire `Show()` (`OnStartup`) and the fallback `Close()` (`OnInitialized` / `MainWindow.ContentRendered`) |
| [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/SKILL.md) | Foundation — STA + `Dispatcher.Run()`, cross-thread `BeginInvoke`, why each WPF object has thread affinity |
| [`shutting-down-wpf-gracefully`](../shutting-down-wpf-gracefully/SKILL.md) | Handles the `OnExit → Dispose() → fade` shutdown race; treat the splash service as one more `IDisposable` participating in graceful teardown |

---

## 9. Checklist

- [ ] Splash lives on a dedicated STA `Thread` with `Dispatcher.Run()`.
- [ ] No call site sets `splash.Owner` to a window on another thread.
- [ ] `OnFadeCompleted` calls `MainWindow.Activate()` **first**, then `splash.Close()`.
- [ ] Splash AXAML has zero references to `App.xaml` resources / WPF-UI theme dictionaries.
- [ ] `Close()` returns silently on the second call (idempotent).
- [ ] A 3-state `Interlocked.CompareExchange` sentinel (or `ManualResetEventSlim`) coordinates Show↔Close.
- [ ] `Dispatcher.HasShutdownStarted` is checked before every cross-thread `Invoke` / `BeginInvoke`.
- [ ] Both `OnInitialized` and `MainWindow.ContentRendered` carry a fallback `Close()`.
- [ ] Pre-`MainWindow` dialogs (license, login) call `Close()` before `ShowDialog()`.

---

## 10. References

- [Dispatcher Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.threading.dispatcher)
- [Dispatcher.Run Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.threading.dispatcher.run)
- [Dispatcher.HasShutdownStarted Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.threading.dispatcher.hasshutdownstarted)
- [Dispatcher.InvokeShutdown Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.threading.dispatcher.invokeshutdown)
- [Window.Activate Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.window.activate) ("The rules that determine whether the window is activated are the same as those used by the Win32 `SetForegroundWindow` function.")
- [Window.Owner Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.window.owner)
- [Application.Current Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.application.current)
- [Interlocked.CompareExchange&lt;T&gt; Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.compareexchange)
- [Timeline.Completed Event — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.media.animation.timeline.completed) (raised on the timeline's Clock — the Storyboard inherits this)
- [SetForegroundWindow function (winuser.h) — Microsoft Learn](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setforegroundwindow)
- [Threading model (WPF) — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/threading-model)
