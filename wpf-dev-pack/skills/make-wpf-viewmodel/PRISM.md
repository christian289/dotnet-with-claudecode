# WPF ViewModel Generator — Prism 9

> `mvvm-framework.md`에서 Prism 9이 선택된 경우 이 파일을 참조합니다.
> CommunityToolkit.Mvvm 버전은 [SKILL.md](SKILL.md)를 참조하세요.

## Differences from CommunityToolkit.Mvvm

| Item | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| Base class | `ObservableObject` | `BindableBase` |
| Property | `[ObservableProperty]` | `SetProperty()` |
| Command | `[RelayCommand]` | `DelegateCommand` |
| DI registration | `IServiceCollection` | `IContainerRegistry` |
| View mapping | DataTemplate Mappings.xaml | `RegisterForNavigation` |

---

## Generated ViewModel

```csharp
using Prism.Mvvm;
using Prism.Commands;

namespace {Namespace}.ViewModels;

public sealed class {Name}ViewModel : BindableBase
{
    private string _title = "{Name}";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private DelegateCommand? _loadedCommand;
    public DelegateCommand LoadedCommand =>
        _loadedCommand ??= new DelegateCommand(ExecuteLoaded);

    private void ExecuteLoaded()
    {
        // TODO: Initialize data
        // TODO: 데이터 초기화
    }
}
```

## DI Registration

In `App.xaml.cs` `RegisterTypes`:

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    // Navigation registration (View + ViewModel pair)
    containerRegistry.RegisterForNavigation<{Name}View, {Name}ViewModel>();
}
```

## Navigation (instead of DataTemplate Mapping)

Prism uses `IRegionManager.RequestNavigate` instead of DataTemplate mapping:

```csharp
// Navigate to the view
_regionManager.RequestNavigate("ContentRegion", nameof({Name}View));
```

XAML region setup:

```xml
<ContentControl prism:RegionManager.RegionName="ContentRegion" />
```

> **Note**: `--no-mapping` flag is ignored in Prism 9 mode since navigation replaces DataTemplate mapping.
