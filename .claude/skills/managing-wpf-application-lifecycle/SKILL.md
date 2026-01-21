---
name: managing-wpf-application-lifecycle
description: Manages WPF Application lifecycle including Startup, Exit, SessionEnding events and single instance detection. Use when handling app initialization, shutdown, or preventing multiple instances.
---

# WPF Application Lifecycle Patterns

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
        // 명령줄 인수 처리
        ProcessCommandLineArgs(e.Args);

        // Initialize services
        // 서비스 초기화
        InitializeServices();

        // Create and show main window manually (if StartupUri not set)
        // 메인 윈도우 수동 생성 및 표시 (StartupUri 미설정 시)
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
    // 설정 저장
    SaveUserSettings();

    // Cleanup resources
    // 리소스 정리
    DisposeServices();

    // Set exit code
    // 종료 코드 설정
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
    // 저장되지 않은 변경사항 확인
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
    // e.ReasonSessionEnding: Logoff 또는 Shutdown

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
    // 긴급 저장 수행
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
        // UI 스레드 예외 처리
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
        // 애플리케이션 충돌 방지 (예외 처리)
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
    // 비-UI 스레드 예외 처리
    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

    // Handle Task exceptions
    // Task 예외 처리
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

## 6. References

- [Application Management Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/application-management-overview)
- [Application Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.application)
- [WPF Application Startup - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/building-a-wpf-application-wpf)
