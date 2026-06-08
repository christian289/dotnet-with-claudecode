# WPF Application Lifecycle Patterns

> Manages WPF Application lifecycle including Startup, Exit, SessionEnding events, single instance detection, and programmatic restart. Use when handling app initialization, shutdown, preventing multiple instances, or restarting the app from code (AppInstance.Restart for packaged/MSIX apps — UWP RequestRestartAsync deadlocks in FullTrust WPF — vs Process.Start for unpackaged).

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/mvvm-framework.md` 설정에 따라 코드 스타일이 결정됩니다.
> Prism 9 사용 시 → [PRISM.md](PRISM.md) 참조

Managing application startup, shutdown, and runtime behavior.

**Advanced Patterns:** See [ADVANCED.md](ADVANCED.md) for single instance, settings, and activation handling.

## 1. Application Lifecycle Overview

```
Application Start
    │
    ├─ App Constructor
    ├─ App.OnStartup() / Startup event
    ├─ MainWindow Constructor
    ├─ MainWindow.Loaded event
    │
    ▼
Running State
    │
    ├─ Activated / Deactivated events
    ├─ SessionEnding event (logoff/shutdown)
    │
    ▼
Shutdown Initiated
    │
    ├─ Window.Closing event (can cancel)
    ├─ Window.Closed event
    ├─ App.OnExit() / Exit event
    │
    ▼
Application End
```

---

## 2. Startup Handling

### 2.1 App.xaml Startup

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml"
             Startup="Application_Startup"
             Exit="Application_Exit"
             SessionEnding="Application_SessionEnding">
    <Application.Resources>
        <!-- Resources -->
    </Application.Resources>
</Application>
```

### 2.2 Override OnStartup

```csharp
namespace MyApp;

using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Process command line arguments
        ProcessCommandLineArgs(e.Args);

        // Initialize services
        InitializeServices();

        // Create and show main window manually (if StartupUri not set)
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    private void ProcessCommandLineArgs(string[] args)
    {
        foreach (var arg in args)
        {
            if (arg.Equals("--debug", StringComparison.OrdinalIgnoreCase))
            {
                // Enable debug mode
            }
            else if (arg.StartsWith("--file=", StringComparison.OrdinalIgnoreCase))
            {
                var filePath = arg[7..];
                // Process file
            }
        }
    }

    private void InitializeServices()
    {
        // DI container setup, logging, etc.
    }
}
```

---

## 3. Shutdown Handling

> **Async cleanup on shutdown?** If you need to `await` anything during shutdown (flushing buffers, `IHost.StopAsync`, closing network connections), do not use `.GetAwaiter().GetResult()` inside `OnExit` — it deadlocks the Dispatcher. See the [`shutting-down-wpf-gracefully`](../shutting-down-wpf-gracefully/SKILL.md) skill for the `OnMainWindowClose` and `OnExplicitShutdown` strategies, and [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/SKILL.md) for the underlying mechanism.

### 3.1 ShutdownMode

```xml
<!-- Default: When last window closes -->
<Application ShutdownMode="OnLastWindowClose">

<!-- When main window closes -->
<Application ShutdownMode="OnMainWindowClose">

<!-- Only when Shutdown() is called explicitly -->
<Application ShutdownMode="OnExplicitShutdown">
```

### 3.2 Override OnExit

```csharp
protected override void OnExit(ExitEventArgs e)
{
    // Save settings
    SaveUserSettings();

    // Cleanup resources
    DisposeServices();

    // Set exit code
    e.ApplicationExitCode = 0;

    base.OnExit(e);
}
```

### 3.3 Window Closing (Can Cancel)

```csharp
// MainWindow.xaml.cs
private void Window_Closing(object sender, CancelEventArgs e)
{
    // Check for unsaved changes
    if (HasUnsavedChanges)
    {
        var result = MessageBox.Show(
            "You have unsaved changes. Do you want to save before closing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        switch (result)
        {
            case MessageBoxResult.Yes:
                SaveChanges();
                break;
            case MessageBoxResult.Cancel:
                e.Cancel = true;  // Prevent closing
                return;
        }
    }
}
```

---

## 4. Session Ending (Logoff/Shutdown)

```csharp
private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
{
    // e.ReasonSessionEnding: Logoff or Shutdown

    if (HasCriticalOperation)
    {
        var result = MessageBox.Show(
            "A critical operation is in progress. Are you sure you want to close?",
            "Session Ending",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
        {
            e.Cancel = true;  // Try to prevent session end
            return;
        }
    }

    // Perform emergency save
    EmergencySave();
}
```

---

## 5. Unhandled Exception Handling

### 5.1 UI Thread Exceptions

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Handle UI thread exceptions
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Log the exception
        LogException(e.Exception);

        // Show error dialog
        MessageBox.Show(
            $"An unexpected error occurred:\n{e.Exception.Message}",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Prevent application crash (handle the exception)
        e.Handled = true;
    }
}
```

### 5.2 Background Thread Exceptions

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Handle non-UI thread exceptions
    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

    // Handle Task exceptions
    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
}

private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    var exception = e.ExceptionObject as Exception;
    LogException(exception);

    if (e.IsTerminating)
    {
        EmergencySave();
    }
}

private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
{
    LogException(e.Exception);
    e.SetObserved();  // Prevent crash
}
```

---

## 6. Programmatic Restart (packaged vs unpackaged)

To restart the app from code, the correct API depends on whether the app runs **packaged** (MSIX) or **unpackaged**. (For the MSIX/WASDK packaging side — Content globbing, multi-exe MSIX, native DLL PATH — see `publishing-wpf-apps`.)

### Packaged (MSIX, incl. FullTrust): use `AppInstance.Restart`

Do **not** use UWP `Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync` in a FullTrust WPF package app: called on the UI thread it **deadlocks without even returning** (the synchronous portion blocks, so a `Task.WhenAny(call.AsTask(), Task.Delay(...))` timeout never fires); moved to a background thread it terminates the process but **does not relaunch** (foreground context is lost). `RequestRestartAsync` assumes a UWP CoreApplication view (CoreDispatcher/CoreWindow) that a `Windows.FullTrustApplication` does not have.

Use Windows App SDK `Microsoft.Windows.AppLifecycle.AppInstance.Restart` instead — it is **synchronous**, so calling it on the UI thread (which is foreground) neither deadlocks nor loses foreground, and it relaunches correctly.

```csharp
using Windows.ApplicationModel.Core;      // AppRestartFailureReason
using Microsoft.Windows.AppLifecycle;     // AppInstance

// Synchronous. On success the process terminates and the next line never runs,
// so REACHING the switch means the restart FAILED — handle it (silent-failure guard).
AppRestartFailureReason reason = AppInstance.Restart(string.Empty);
switch (reason)
{
    case AppRestartFailureReason.NotInForeground:
        // The app must be visible / in the foreground when calling Restart.
        break;
    case AppRestartFailureReason.RestartPending:
    case AppRestartFailureReason.InvalidUser:
    case AppRestartFailureReason.Other:
        // Surface the failure to the user; do not discard the return value.
        break;
}
```

`AppInstance.Restart(string arguments)` is **static** and returns `Windows.ApplicationModel.Core.AppRestartFailureReason` (`RestartPending` / `NotInForeground` / `InvalidUser` / `Other`). Always inspect it — discarding it makes a failed restart look like "the button did nothing".

### Unpackaged (development / xcopy): `Process.Start` + `Shutdown`

```csharp
var exePath = Environment.ProcessPath;   // null-guard: can be null in rare hosts
if (exePath is not null)
{
    Process.Start(exePath);
    Application.Current.Shutdown();
}
```

Do **not** `Process.Start` a packaged app's exe directly — it launches outside the package and **loses package identity**. Packaged apps must restart via `AppInstance.Restart`.

### Command wiring

A restart command is usually `async void`; wrap the whole method in try-catch so an `AppInstance.Restart` / `Process.Start` exception reaches the user. Switching to `async Task` does not by itself add robustness — and `DelegateCommand(Action)` does not accept `Func<Task>`, so you would need `AsyncDelegateCommand` and still need the try-catch (or command-level exception handling) separately.

---

## 7. Related Skills

| Skill | Relationship |
|---|---|
| [`shutting-down-wpf-gracefully`](../shutting-down-wpf-gracefully/SKILL.md) | Async cleanup during `Window.Closing` / `Closed` and graceful `Application.Shutdown()` timing |
| [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/SKILL.md) | Dispatcher priority and cross-thread marshaling for lifecycle code |
| [`implementing-wpf-splash-screen`](../implementing-wpf-splash-screen/SKILL.md) | STA-thread splash with cross-thread foreground handoff to `MainWindow` — wires into `OnStartup` and `MainWindow.ContentRendered` |

---

## 8. References

- [Application Management Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/application-management-overview)
- [Application Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.application)
- [WPF Application Startup - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/building-a-wpf-application-wpf)
