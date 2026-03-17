# WPF Project Structure - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 모듈 아키텍처. SKILL.md의 Clean Architecture 대응.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| 아키텍처 | Clean Architecture 레이어 | Module 기반 기능 분리 |
| 진입점 | `.WpfApp` (Composition Root) | Shell (PrismApplication) |
| 기능 분리 | 레이어별 프로젝트 | `Modules.*` 프로젝트 |
| 모듈 초기화 | 없음 (수동) | `IModule` 인터페이스 |
| 공유 코드 | `.Core` / `.Application` | `.Core` (공유 인터페이스, 모델) |

## Prism Module 기반 구조

```
MyApp/
├── MyApp.slnx
├── Directory.Build.props
├── src/
│   ├── MyApp/                           # 🔴 Shell Application
│   │   ├── App.xaml                     # PrismApplication
│   │   ├── App.xaml.cs
│   │   ├── MainWindow.xaml              # Region Host
│   │   ├── MainWindow.xaml.cs
│   │   ├── GlobalUsings.cs
│   │   └── MyApp.csproj
│   │
│   ├── MyApp.Core/                      # 🔵 Shared (인터페이스, 모델, 상수)
│   │   ├── Mvvm/
│   │   │   └── RegionNames.cs
│   │   ├── Models/
│   │   ├── Services/                    # 서비스 인터페이스
│   │   ├── Events/                      # PubSubEvent 정의
│   │   ├── GlobalUsings.cs
│   │   └── MyApp.Core.csproj
│   │
│   ├── MyApp.Modules.Home/             # 🟢 Feature Module
│   │   ├── HomeModule.cs               # IModule 구현
│   │   ├── ViewModels/
│   │   │   └── HomeViewModel.cs
│   │   ├── Views/
│   │   │   └── HomeView.xaml
│   │   ├── GlobalUsings.cs
│   │   └── MyApp.Modules.Home.csproj
│   │
│   └── MyApp.Modules.Settings/         # 🟢 Feature Module
│       ├── SettingsModule.cs
│       ├── ViewModels/
│       ├── Views/
│       └── MyApp.Modules.Settings.csproj
│
└── tests/
    └── MyApp.Modules.Home.Tests/
```

## NuGet 패키지 분배

| 프로젝트 | NuGet 패키지 |
|----------|-------------|
| Shell (`MyApp`) | `Prism.DryIoc` (9.0.537) |
| Core (`MyApp.Core`) | `Prism.Core` (9.0.537) |
| Module (`MyApp.Modules.*`) | `Prism.DryIoc` (9.0.537) |

## csproj 예시

### Shell (MyApp.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Prism.DryIoc" Version="9.0.537" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
    <ProjectReference Include="..\MyApp.Modules.Home\MyApp.Modules.Home.csproj" />
    <ProjectReference Include="..\MyApp.Modules.Settings\MyApp.Modules.Settings.csproj" />
  </ItemGroup>
</Project>
```

### Core (MyApp.Core.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Prism.Core" Version="9.0.537" />
  </ItemGroup>
</Project>
```

### Module (MyApp.Modules.Home.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Prism.DryIoc" Version="9.0.537" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>
</Project>
```

## IModule 구현 패턴

```csharp
namespace MyApp.Modules.Home;

public class HomeModule : IModule
{
    private readonly IRegionManager _regionManager;

    public HomeModule(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // View-ViewModel 네비게이션 등록
        // Register View-ViewModel navigation
        containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();

        // 모듈 내부 서비스 등록
        // Register module-internal services
        containerRegistry.RegisterSingleton<IHomeService, HomeService>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 초기 View를 Region에 등록
        // Register initial View to Region
        _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(HomeView));
    }
}
```

## 프로젝트 참조 규칙

```
Shell ──→ Core, Modules.*
Module ──→ Core (only)
Core ──→ (no references)
```

- **Module 간 참조 금지**: Module끼리 직접 참조하지 않음. `IEventAggregator`로 통신
- **Core는 독립적**: 순수 인터페이스, 모델, 이벤트 정의만 포함

## Key Differences from SKILL.md

- **Module 기반 분리**: Clean Architecture 레이어 대신 기능(Feature) 단위 모듈 분리
- **IModule 인터페이스**: 각 모듈이 `RegisterTypes` + `OnInitialized`로 자체 초기화
- **RegionManager**: 모듈이 자신의 View를 Region에 등록
- **IEventAggregator**: 모듈 간 통신에 `PubSubEvent` 사용 (직접 참조 대신)
- **Clean Architecture 혼용 가능**: 대규모 모듈 내부에서 Domain/Application/Infrastructure 레이어 적용 가능
