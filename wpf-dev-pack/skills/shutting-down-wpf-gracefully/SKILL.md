---
description: Shuts down a WPF application gracefully when cleanup requires awaiting async work (flushing buffers, closing connections, IHost.StopAsync, disposing DI scopes). Use whenever App.OnExit or Window.Closing needs to run async code and the naive .GetAwaiter().GetResult() pattern would deadlock the Dispatcher. Explains why App.OnExit cannot be made async void, then presents two fully worked strategies — ShutdownMode=OnMainWindowClose with an async Window.OnClosing that cancels, awaits, and re-closes, and ShutdownMode=OnExplicitShutdown with an async MainWindow.Closed handler that ends with Application.Current.Shutdown(). Covers cancellation-token guards against hangs, IHost.StopAsync teardown for CommunityToolkit.Mvvm + GenericHost, and container disposal for Prism 9. Apply together with the preventing-dispatcher-deadlock skill, which this skill builds on for shutdown-specific scenarios.
user-invocable: false
model: sonnet
---

# Shutting Down WPF Gracefully with Async Cleanup

> **Prerequisite**: Read [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/SKILL.md) first. This skill applies that skill's rules to the shutdown lifecycle, where the override signatures make the problem harder to fix in place.

## 1. Why `App.OnExit` Is the Hard Case

```csharp
// The override signature is fixed by the base class
protected override void OnExit(ExitEventArgs e) { }
```

- The signature returns `void`, but it is **not an event handler** — it is a virtual method. Changing it to `async void` would hide the base method and violate the override contract.
- Marking it `async void` anyway is allowed by the compiler but still blocks the Dispatcher once `OnExit` returns, because the shutdown sequence keeps running synchronously after `OnExit` completes.
- Therefore, `OnExit` must finish synchronously. Any async cleanup must happen **before** the shutdown reaches `OnExit`.

The solution is to move async cleanup earlier in the lifecycle — into `Window.Closing` or `Window.Closed` — and control the shutdown using `ShutdownMode`.

---

## 2. ShutdownMode Primer

| Mode | Triggers shutdown when |
|------|------------------------|
| `OnLastWindowClose` (default) | The last open `Window` is closed |
| `OnMainWindowClose` | `Application.MainWindow` is closed |
| `OnExplicitShutdown` | `Application.Shutdown()` is called explicitly |

For async cleanup, use `OnMainWindowClose` (Strategy A) or `OnExplicitShutdown` (Strategy B).

---

## 3. Strategy A — `OnMainWindowClose` + Cancel-Await-Close in `OnClosing`

The main window cancels its own close, awaits cleanup, then closes itself. Because `ShutdownMode` is `OnMainWindowClose`, the application ends once the main window is finally closed.

### 3.1 App.xaml

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml"
             ShutdownMode="OnMainWindowClose">
    <Application.Resources>
        <!-- Resources -->
    </Application.Resources>
</Application>
```

### 3.2 MainWindow cleanup flow

```csharp
namespace MyApp;

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public partial class MainWindow : Window
{
    private readonly ICleanupService _cleanup;
    private bool _cleanupCompleted;
    private bool _isClosing;

    public MainWindow(ICleanupService cleanup)
    {
        InitializeComponent();
        _cleanup = cleanup;
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_cleanupCompleted)
        {
            // Second pass: cleanup already ran, let the close proceed
            return;
        }

        if (_isClosing)
        {
            // Reentrancy guard — user clicked X multiple times
            e.Cancel = true;
            return;
        }

        _isClosing = true;
        e.Cancel = true;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _cleanup.DisposeAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Cleanup failed during shutdown");
        }
        finally
        {
            _cleanupCompleted = true;
            Close(); // Re-enter OnClosing with _cleanupCompleted == true
        }
    }
}
```

### 3.3 Key points

- `e.Cancel = true` prevents the window from closing until cleanup finishes.
- A `_cleanupCompleted` flag lets the second `Close()` call pass through.
- The `CancellationTokenSource` timeout prevents a broken `DisposeAsync` from hanging shutdown forever.
- `OnClosing` is an event-dispatch entry point, so `async void` is the correct choice here (see `preventing-dispatcher-deadlock`).

---

## 4. Strategy B — `OnExplicitShutdown` + Async Cleanup in `MainWindow.Closed`

The main window closes immediately, then the `Closed` event does async cleanup and finally calls `Application.Current.Shutdown()`. The UI disappears first, cleanup runs under a hidden Dispatcher still pumping messages, and only then the process ends.

### 4.1 App.xaml

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml"
             ShutdownMode="OnExplicitShutdown">
</Application>
```

### 4.2 Wire the Closed handler

```csharp
namespace MyApp;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public partial class App : Application
{
    private readonly ICleanupService _cleanup;

    public App(ICleanupService cleanup)
    {
        _cleanup = cleanup;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow();
        mainWindow.Closed += OnMainWindowClosed;
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private async void OnMainWindowClosed(object? sender, EventArgs e)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _cleanup.DisposeAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Cleanup failed during shutdown");
        }
        finally
        {
            Current.Shutdown();
        }
    }
}
```

### 4.3 Key points

- The UI disappears immediately, so the user does not perceive a hang during cleanup.
- The Dispatcher keeps pumping until `Application.Shutdown()` is called, so async continuations run normally.
- `async void` is correct for `Closed` because `EventHandler` requires a `void` return.
- The timeout guard applies for the same reason as Strategy A.

---

## 5. Comparing the Two Strategies

| Criterion | Strategy A (OnMainWindowClose + OnClosing) | Strategy B (OnExplicitShutdown + Closed) |
|-----------|---------------------------------------------|-------------------------------------------|
| User-visible latency | Window stays open while cleanup runs | Window disappears immediately |
| Can show a "closing" UI | Yes — the window is still rendering | No — the window is gone |
| Can cancel close based on cleanup outcome | Yes — `e.Cancel` is already set | No — `Closed` is past the cancel point |
| Code lives in | `MainWindow` | `App` (or a dedicated coordinator) |
| Risk of forgetting to call `Shutdown()` | None | Must call `Application.Current.Shutdown()` |
| Complexity | Slightly higher (reentrancy guard) | Lower |

**Default recommendation**: Strategy A when cleanup takes long enough that users benefit from a progress indicator, Strategy B when cleanup is quick and you want an immediately responsive close.

---

## 6. DI Container Teardown

### 6.1 CommunityToolkit.Mvvm + Generic Host

```csharp
namespace MyApp;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<ICleanupService, CleanupService>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Closed += OnMainWindowClosedAsync;
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private async void OnMainWindowClosedAsync(object? sender, EventArgs e)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _host.StopAsync(cts.Token);
            _host.Dispose();
        }
        finally
        {
            Current.Shutdown();
        }
    }
}
```

`IHost.StopAsync` triggers `IHostedService.StopAsync` on every hosted service, drains the host, and runs `IAsyncDisposable.DisposeAsync` on registered services. Do all of this **before** `Shutdown()`.

### 6.2 Prism 9

Prism 9's container (`IContainerExtension`) is `IDisposable`, not `IAsyncDisposable`, so async cleanup must happen in your own services before the container is disposed.

```csharp
namespace MyApp;

using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
        => Container.Resolve<MainWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ICleanupService, CleanupService>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (MainWindow is { } mainWindow)
        {
            mainWindow.Closed += OnMainWindowClosedAsync;
        }
    }

    private async void OnMainWindowClosedAsync(object? sender, EventArgs e)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cleanup = Container.Resolve<ICleanupService>();
            await cleanup.DisposeAsync(cts.Token);
        }
        finally
        {
            Current.Shutdown();
        }
    }
}
```

Register container disposal in `OnExit` **after** async cleanup has already completed:

```csharp
protected override void OnExit(ExitEventArgs e)
{
    // At this point, async cleanup has already finished
    // because Shutdown() was called from OnMainWindowClosedAsync.
    base.OnExit(e);
    (Container as IDisposable)?.Dispose();
}
```

---

## 7. What to Do with `OnExit`

After applying Strategy A or B, `OnExit` should contain **only synchronous** post-cleanup work such as flushing logs, writing a final exit code, or disposing unmanaged resources.

```csharp
protected override void OnExit(ExitEventArgs e)
{
    // Purely synchronous work only
    Log.CloseAndFlush();
    e.ApplicationExitCode = 0;
    base.OnExit(e);
}
```

Do **not** put async cleanup here. If something genuinely must run during `OnExit`, redesign the service to expose a synchronous `Dispose` and move its async work into the earlier stage.

---

## 8. Session Ending (Logoff / Shutdown)

`SessionEnding` fires when Windows is logging off or shutting down. The OS gives your process a limited amount of time, so you cannot count on long-running cleanup here.

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    SessionEnding += OnSessionEnding;
}

private async void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
{
    try
    {
        // OS-imposed budget is typically a few seconds — time-box cleanup hard
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await _cleanup.FlushCriticalStateAsync(cts.Token);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "SessionEnding cleanup failed");
    }
}
```

For full-weight cleanup during session end, persist enough state synchronously that a restart can resume correctly, rather than trying to do everything before the OS forcibly terminates you.

---

## 9. Checklist

- [ ] `preventing-dispatcher-deadlock` skill reviewed and applied in the project
- [ ] `ShutdownMode` set to `OnMainWindowClose` or `OnExplicitShutdown`
- [ ] Async cleanup runs inside `Window.OnClosing` or `MainWindow.Closed` (never `OnExit`)
- [ ] `OnClosing` sets `e.Cancel = true` until cleanup completes, then re-calls `Close()`
- [ ] `Closed` handler ends with `Application.Current.Shutdown()` when using Strategy B
- [ ] `CancellationTokenSource` timeout protects against stuck cleanup
- [ ] `IHost.StopAsync` awaited before `IHost.Dispose` (CommunityToolkit.Mvvm)
- [ ] Prism container disposed in `OnExit` only after async cleanup has completed
- [ ] `OnExit` contains purely synchronous work, no `.GetAwaiter().GetResult()` patterns
- [ ] `SessionEnding` cleanup is time-boxed to a few seconds

---

## 10. Related Skills

| Skill | Relationship |
|-------|--------------|
| `preventing-dispatcher-deadlock` | Foundation — explains why sync-over-async deadlocks |
| `managing-wpf-application-lifecycle` | Broader lifecycle events: Startup, SessionEnding, exception handling |
| `configuring-dependency-injection` | `IHost` / Prism container registration details |

---

## 11. References

- [Application.ShutdownMode — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.application.shutdownmode)
- [Window.Closing Event — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.window.closing)
- [IHostedService.StopAsync — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostedservice.stopasync)
- [Prism 9 Application Lifecycle](https://prismlibrary.com/docs/wpf/application-lifecycle.html)
- [Original discussion — dotnetdev.kr forum](https://forum.dotnetdev.kr/t/c-task-getawaiter-and-getresult-with-application-onexit-window-closing/14174)
