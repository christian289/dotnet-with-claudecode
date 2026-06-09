# WPF Application Lifecycle - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 애플리케이션 수명주기. SKILL.md의 Application 수명주기 대응.

> SKILL.md의 Window.Closing, SessionEnding, 예외 처리 패턴은 프레임워크 무관이므로 그대로 사용합니다.
> 이 문서는 PrismApplication의 수명주기 차이점만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| 진입점 | `Application.OnStartup()` | `PrismApplication.CreateShell()` |
| 초기화 순서 | `OnStartup` → 수동 Show | `CreateShell` → `OnInitialized` |
| DI 설정 | `Host.CreateDefaultBuilder()` | `RegisterTypes()` |
| 모듈 초기화 | 없음 | `ConfigureModuleCatalog()` → `IModule.OnInitialized()` |
| 종료 처리 | `OnExit` + `_host.StopAsync()` | `OnExit` (호스트 불필요) |

## PrismApplication 수명주기

```
Application Start
    │
    ├─ PrismApplication Constructor
    ├─ RegisterTypes()                 ← DI 서비스 등록
    ├─ ConfigureModuleCatalog()        ← 모듈 등록
    ├─ CreateShell()                   ← MainWindow 생성 및 반환
    ├─ InitializeShell()               ← Shell 표시 (자동)
    ├─ OnInitialized()                 ← 초기화 완료 콜백
    │   └─ IModule.OnInitialized()     ← 각 모듈 초기화
    │
    ▼
Running State
    │
    ├─ Region 네비게이션
    ├─ Module 로딩/언로딩
    │
    ▼
Shutdown
    │
    ├─ Window.Closing (취소 가능)
    ├─ Window.Closed
    ├─ OnExit()
    │
    ▼
Application End
```

## 1. PrismApplication 구현

```csharp
namespace MyApp;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        // GenericHost 버전:
        // var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        // mainWindow.Show();

        // Prism 9 버전: Window 반환만 하면 자동 표시
        // Prism 9 version: Just return Window, it shows automatically
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // GenericHost 버전의 ConfigureServices에 해당
        // Equivalent to GenericHost's ConfigureServices
        containerRegistry.RegisterSingleton<IUserService, UserService>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<HomeModule>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Shell 표시 후 추가 초기화
        // Additional initialization after Shell is shown
        InitializeServices();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 정리 로직 (GenericHost의 StopAsync 불필요)
        // Cleanup logic (no StopAsync needed)
        SaveUserSettings();
        DisposeServices();

        base.OnExit(e);
    }
}
```

## 2. 미처리 예외 (Prism에서도 동일)

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    // UI 스레드 예외
    // UI thread exceptions
    DispatcherUnhandledException += OnDispatcherUnhandledException;

    // 백그라운드 스레드 예외
    // Background thread exceptions
    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
}
```

## 3. 명령줄 인수 처리

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    // Prism에서 명령줄 인수 접근
    // Access command line args in Prism
    var args = Environment.GetCommandLineArgs();
    ProcessCommandLineArgs(args);
}
```

## Key Differences from SKILL.md

- **CreateShell**: `OnStartup` + `Show()` 대신 `CreateShell()`에서 Window 반환 (자동 표시)
- **OnInitialized**: Shell 표시 후 호출되는 콜백 (추가 초기화에 사용)
- **Host 불필요**: `_host.StartAsync()` / `_host.StopAsync()` 호출 불필요
- **모듈 초기화**: `OnInitialized` 시점에 등록된 모듈의 `IModule.OnInitialized()` 자동 호출
- **ShutdownMode**: `PrismApplication`에서도 App.xaml의 `ShutdownMode` 설정 동일하게 사용
