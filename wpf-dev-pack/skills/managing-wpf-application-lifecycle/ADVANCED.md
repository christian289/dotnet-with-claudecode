# Advanced Application Lifecycle Patterns

## Single Instance Application

### Using Mutex

```csharp
namespace MyApp;

using System;
using System.Threading;
using System.Windows;

public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = "MyApp_SingleInstance_Mutex";

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            // Another instance is already running
            // 다른 인스턴스가 이미 실행 중
            MessageBox.Show(
                "Application is already running.",
                "Single Instance",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Optionally activate existing instance
            // 기존 인스턴스 활성화 (선택)
            ActivateExistingInstance();

            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }

    private void ActivateExistingInstance()
    {
        // Use named pipe or other IPC to signal existing instance
        // 네임드 파이프 또는 다른 IPC로 기존 인스턴스에 신호
    }
}
```

### Passing Arguments to Existing Instance

```csharp
namespace MyApp;

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public partial class App : Application
{
    private const string PipeName = "MyApp_SingleInstance_Pipe";
    private static Mutex? _mutex;
    private CancellationTokenSource? _pipeServerCts;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, "MyApp_Mutex", out var createdNew);

        if (!createdNew)
        {
            // Send arguments to existing instance
            // 기존 인스턴스에 인수 전송
            SendArgumentsToExistingInstance(e.Args);
            Shutdown();
            return;
        }

        // Start pipe server to receive arguments
        // 인수 수신용 파이프 서버 시작
        StartPipeServer();

        base.OnStartup(e);
    }

    private void StartPipeServer()
    {
        _pipeServerCts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!_pipeServerCts.Token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(PipeName);
                    await server.WaitForConnectionAsync(_pipeServerCts.Token);

                    using var reader = new StreamReader(server);
                    var args = await reader.ReadToEndAsync();

                    // Process arguments on UI thread
                    // UI 스레드에서 인수 처리
                    Dispatcher.Invoke(() => ProcessReceivedArguments(args.Split('\n')));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        });
    }

    private void SendArgumentsToExistingInstance(string[] args)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(1000);

            using var writer = new StreamWriter(client);
            writer.Write(string.Join('\n', args));
        }
        catch
        {
            // Failed to connect
        }
    }

    private void ProcessReceivedArguments(string[] args)
    {
        // Activate main window
        // 메인 윈도우 활성화
        MainWindow?.Activate();

        // Process arguments
        // 인수 처리
        foreach (var arg in args)
        {
            if (!string.IsNullOrWhiteSpace(arg))
            {
                // Handle argument
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _pipeServerCts?.Cancel();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
```

---

## Application Properties (Settings)

### Using Application.Properties

```csharp
// Store temporary runtime data
// 임시 런타임 데이터 저장
Application.Current.Properties["CurrentUser"] = currentUser;
Application.Current.Properties["IsDebugMode"] = true;

// Retrieve
// 검색
var user = Application.Current.Properties["CurrentUser"] as User;
```

### Using Settings.settings

```csharp
// Access strongly-typed settings
// 강력한 형식의 설정 접근
var lastOpenedFile = Properties.Settings.Default.LastOpenedFile;
Properties.Settings.Default.LastOpenedFile = filePath;
Properties.Settings.Default.Save();
```

---

## Activation and Deactivation

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Activated += OnActivated;
        Deactivated += OnDeactivated;
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        // Application gained focus
        // 애플리케이션이 포커스를 얻음
        ResumeBackgroundTasks();
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        // Application lost focus
        // 애플리케이션이 포커스를 잃음
        PauseBackgroundTasks();
        SaveDraft();
    }
}
```
