# Dependency Injection - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 DI 패턴. SKILL.md의 GenericHost 대응.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm (GenericHost) | Prism 9 |
|------|-------------------------------------|---------|
| 진입점 | `Application` + `Host.CreateDefaultBuilder()` | `PrismApplication` |
| DI 컨테이너 | `IServiceCollection` | `IContainerRegistry` |
| 서비스 해석 | `IServiceProvider` | `IContainerProvider` |
| 구성 위치 | `App()` 생성자 | `RegisterTypes()` 오버라이드 |
| Shell 생성 | `OnStartup()` + 수동 | `CreateShell()` 오버라이드 |
| 모듈 시스템 | 없음 | `ConfigureModuleCatalog()` |
| NuGet | `Microsoft.Extensions.Hosting` | `Prism.DryIoc` |

## Prerequisites

```xml
<!-- Shell (WPF Application) -->
<PackageReference Include="Prism.DryIoc" Version="9.0.537" />

<!-- Core / Module 프로젝트 -->
<!-- Core / Module projects -->
<PackageReference Include="Prism.Core" Version="9.0.537" />
```

## 1. App.xaml (PrismApplication)

```xml
<prism:PrismApplication x:Class="MyApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:prism="http://prismlibrary.com/">
    <Application.Resources>
        <!-- Styles, Themes -->
    </Application.Resources>
</prism:PrismApplication>
```

## 2. App.xaml.cs (PrismApplication)

```csharp
// GenericHost 버전:
// public partial class App : Application
// {
//     private readonly IHost _host;
//     public App() { _host = Host.CreateDefaultBuilder()... }
// }

// Prism 9 버전:
// Prism 9 version:
namespace MyApp;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 서비스 등록
        // Register services
        containerRegistry.RegisterSingleton<IUserRepository, UserRepository>();
        containerRegistry.RegisterSingleton<IUserService, UserService>();

        // ViewModel 등록 (Transient)
        // Register ViewModels (Transient)
        containerRegistry.Register<MainViewModel>();
        containerRegistry.Register<SettingsViewModel>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        // 모듈 등록
        // Register modules
        moduleCatalog.AddModule<HomeModule>();
    }
}
```

## 3. 등록 API 매핑

| GenericHost (`IServiceCollection`) | Prism 9 (`IContainerRegistry`) |
|------------------------------------|-------------------------------|
| `services.AddSingleton<I, T>()` | `containerRegistry.RegisterSingleton<I, T>()` |
| `services.AddTransient<I, T>()` | `containerRegistry.Register<I, T>()` |
| `services.AddSingleton<T>()` | `containerRegistry.RegisterSingleton<T>()` |
| `services.AddTransient<T>()` | `containerRegistry.Register<T>()` |
| N/A | `containerRegistry.RegisterForNavigation<V, VM>()` |
| N/A | `containerRegistry.RegisterDialog<V, VM>()` |
| N/A | `containerRegistry.RegisterInstance<I>(instance)` |

> ⚠️ Prism은 `Scoped` 수명을 지원하지 않습니다. WPF에서는 일반적으로 불필요합니다.

## 4. Constructor Injection

```csharp
// GenericHost와 동일한 패턴
// Same pattern as GenericHost
public class MainViewModel : BindableBase
{
    private readonly IUserService _userService;

    public MainViewModel(IUserService userService)
    {
        _userService = userService;
    }
}
```

## 5. MainWindow.xaml.cs

```csharp
// GenericHost 버전: 생성자 주입
// GenericHost version: constructor injection
// Prism 9 버전: ViewModelLocator가 자동으로 ViewModel 연결
// Prism 9 version: ViewModelLocator auto-wires ViewModel
namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // DataContext는 ViewModelLocator가 자동 설정
        // DataContext is auto-set by ViewModelLocator
    }
}
```

```xml
<!-- MainWindow.xaml -->
<Window x:Class="MyApp.MainWindow"
    xmlns:prism="http://prismlibrary.com/"
    prism:ViewModelLocator.AutoWireViewModel="True"
    Title="MyApp">
    <!-- Content -->
</Window>
```

## 6. 서비스 수명 비교

| 수명 | GenericHost | Prism 9 | 용도 |
|------|------------|---------|------|
| Singleton | `AddSingleton` | `RegisterSingleton` | Repository, 글로벌 서비스 |
| Transient | `AddTransient` | `Register` | ViewModel, 일회성 서비스 |
| Scoped | `AddScoped` | ❌ 미지원 | Web 전용 (WPF 불필요) |

## 7. GlobalUsings.cs (Prism)

```csharp
global using System.Windows;
global using Prism.Ioc;
global using Prism.Modularity;
global using Prism.DryIoc;
```

## Key Differences from SKILL.md

- **PrismApplication**: `Application` 대신 `PrismApplication` 상속 (`Host.CreateDefaultBuilder` 불필요)
- **RegisterTypes**: `ConfigureServices` 대신 `RegisterTypes` 오버라이드
- **CreateShell**: `OnStartup` + 수동 `Show()` 대신 `CreateShell()` 반환
- **ViewModelLocator**: XAML에서 `prism:ViewModelLocator.AutoWireViewModel="True"` 설정으로 자동 ViewModel 연결
- **Scoped 미지원**: Prism DI 컨테이너는 Scoped 수명을 지원하지 않음
- **Navigation/Dialog 등록**: `RegisterForNavigation`, `RegisterDialog` 전용 API 제공
