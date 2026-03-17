# CollectionView MVVM - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 CollectionView 패턴. SKILL.md의 CommunityToolkit.Mvvm 대응.

> SKILL.md의 Service Layer 아키텍처, CollectionViewSource 캡슐화 패턴은 그대로 적용됩니다.
> 이 문서는 ViewModel의 BindableBase 문법과 DI 등록 차이만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| ViewModel Base | `ObservableObject` | `BindableBase` |
| 속성 정의 | `[ObservableProperty]` | `SetProperty()` 수동 |
| DI 등록 | `services.AddSingleton<I, T>()` | `containerRegistry.RegisterSingleton<I, T>()` |

## 1. ViewModel (BindableBase 문법)

```csharp
// CommunityToolkit.Mvvm 버전:
// public sealed partial class AppViewModel(IMemberCollectionService memberService)
//     : ObservableObject
// {
//     public IEnumerable? Members { get; } = memberService.CreateView();
// }

// Prism 9 버전:
// Prism 9 version:
namespace MyApp.ViewModels;

public class AppViewModel : BindableBase
{
    public IEnumerable? Members { get; }

    public AppViewModel(IMemberCollectionService memberService)
    {
        Members = memberService.CreateView();
    }
}

// 필터 적용 ViewModel
// Filtered ViewModel
public class WalkerViewModel : BindableBase
{
    public IEnumerable? Members { get; }

    public WalkerViewModel(IMemberCollectionService memberService)
    {
        Members = memberService.CreateView(
            item => (item as Member)?.Type == DeviceTypes.Walker);
    }
}
```

## 2. DI 등록 (IContainerRegistry)

```csharp
// CommunityToolkit.Mvvm 버전 (GenericHost):
// services.AddSingleton<IMemberCollectionService, MemberCollectionService>();

// Prism 9 버전:
// Prism 9 version:
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterSingleton<IMemberCollectionService, MemberCollectionService>();
    containerRegistry.Register<AppViewModel>();
    containerRegistry.Register<WalkerViewModel>();
}
```

## 3. Service Layer (변경 없음)

Service Layer의 `MemberCollectionService`는 WPF `CollectionViewSource`를 직접 사용하므로 MVVM 프레임워크와 무관합니다. SKILL.md의 구현을 그대로 사용합니다.

## Key Differences from SKILL.md

- **BindableBase**: `ObservableObject` 대신 `BindableBase` 상속
- **SetProperty**: `[ObservableProperty]` 대신 수동 속성 작성
- **IContainerRegistry**: `IServiceCollection` 대신 Prism DI API 사용
- **Service Layer 동일**: `CollectionViewSource` 캡슐화 패턴은 변경 없음
