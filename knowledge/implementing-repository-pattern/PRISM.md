# Repository Pattern - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 DI 등록 패턴. SKILL.md의 GenericHost DI 대응.

> SKILL.md의 Repository/Service 인터페이스, 구현체, 레이어 구조는 프레임워크 무관이므로 그대로 사용합니다.
> 이 문서는 DI 등록 문법 차이만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm (GenericHost) | Prism 9 |
|------|-------------------------------------|---------|
| 등록 위치 | `ConfigureServices` 람다 | `RegisterTypes` 오버라이드 |
| 등록 API | `services.AddSingleton<I, T>()` | `containerRegistry.RegisterSingleton<I, T>()` |
| Transient | `services.AddTransient<T>()` | `containerRegistry.Register<T>()` |

## DI 등록 (IContainerRegistry)

```csharp
// GenericHost 버전:
// var host = Host.CreateDefaultBuilder(args)
//     .ConfigureServices(services =>
//     {
//         services.AddSingleton<IUserRepository, UserRepository>();
//         services.AddSingleton<IUserService, UserService>();
//         services.AddSingleton<App>();
//     })
//     .Build();

// Prism 9 버전:
// Prism 9 version:
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    // Repository 등록
    // Register Repository
    containerRegistry.RegisterSingleton<IUserRepository, UserRepository>();

    // Service 등록
    // Register Service
    containerRegistry.RegisterSingleton<IUserService, UserService>();
}
```

## Module 내부 등록

```csharp
// 모듈별로 자체 Repository/Service 등록
// Each module registers its own Repository/Service
public class DataModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IUserRepository, UserRepository>();
        containerRegistry.RegisterSingleton<IUserService, UserService>();
    }

    public void OnInitialized(IContainerProvider containerProvider) { }
}
```

## Key Differences from SKILL.md

- **RegisterTypes**: `ConfigureServices` 람다 대신 `PrismApplication.RegisterTypes()` 오버라이드
- **IContainerRegistry API**: `AddSingleton` → `RegisterSingleton`, `AddTransient` → `Register`
- **Repository/Service 동일**: 인터페이스, 구현체, 레이어 구조는 변경 없음
