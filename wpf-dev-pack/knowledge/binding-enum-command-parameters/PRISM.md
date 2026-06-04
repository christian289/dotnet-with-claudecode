# Enum Command Parameter Binding - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 Enum CommandParameter 패턴. SKILL.md의 CommunityToolkit.Mvvm 대응.

> XAML의 `x:Static` 마크업 확장은 프레임워크 무관이므로 SKILL.md와 동일합니다.
> 이 문서는 ViewModel의 DelegateCommand 부분만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| Command 정의 | `[RelayCommand]` 소스 제너레이터 | `DelegateCommand<T>` 수동 작성 |
| 타입 제약 | Value type 직접 지원 | Nullable wrapper 필요 (`T?`) |

## ViewModel (Prism 9)

```csharp
// CommunityToolkit.Mvvm 버전:
// [RelayCommand]
// private void SelectTool(ViewerTool tool) { CurrentTool = tool; }

// Prism 9 버전:
// Prism 9 version:
public class ViewerViewModel : BindableBase
{
    private ViewerTool _currentTool = ViewerTool.Pan;
    public ViewerTool CurrentTool
    {
        get => _currentTool;
        set => SetProperty(ref _currentTool, value);
    }

    // DelegateCommand<T>는 class 제약이 있으므로 Nullable enum 사용
    // DelegateCommand<T> has class constraint, use Nullable enum
    private DelegateCommand<ViewerTool?>? _selectToolCommand;
    public DelegateCommand<ViewerTool?> SelectToolCommand =>
        _selectToolCommand ??= new DelegateCommand<ViewerTool?>(ExecuteSelectTool);

    private void ExecuteSelectTool(ViewerTool? tool)
    {
        if (tool.HasValue)
            CurrentTool = tool.Value;
    }
}
```

## XAML (SKILL.md와 동일)

```xml
<!-- x:Static은 프레임워크 무관 -->
<!-- x:Static is framework-independent -->
<ToggleButton Content="Pan"
              Command="{Binding SelectToolCommand}"
              CommandParameter="{x:Static viewmodels:ViewerTool.Pan}" />
```

## Key Differences from SKILL.md

- **DelegateCommand\<T?\>**: `[RelayCommand]` 대신 `DelegateCommand<ViewerTool?>` 수동 작성
- **Nullable 처리**: Prism의 `DelegateCommand<T>`는 참조 타입 제약이 있으므로 값 타입은 `T?` 사용
- **XAML 동일**: `x:Static`, `EnumToBoolConverter` 등 XAML 부분은 변경 없음
