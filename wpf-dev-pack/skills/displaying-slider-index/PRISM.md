# Slider Index Display - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 Slider 인덱스 패턴. SKILL.md의 CommunityToolkit.Mvvm 대응.

> XAML 바인딩은 프레임워크 무관이므로 SKILL.md와 동일합니다.
> 이 문서는 ViewModel의 BindableBase + SetProperty 패턴만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| 속성 정의 | `[ObservableProperty]` | `SetProperty()` 수동 |
| 연관 속성 알림 | `[NotifyPropertyChangedFor]` | `RaisePropertyChanged()` 수동 |

## ViewModel (Prism 9)

```csharp
// CommunityToolkit.Mvvm 버전:
// [NotifyPropertyChangedFor(nameof(SliceDisplayNumber))]
// [ObservableProperty] private int _currentSliceIndex;
//
// [NotifyPropertyChangedFor(nameof(MaxSliceIndex))]
// [ObservableProperty] private int _totalSliceCount;

// Prism 9 버전:
// Prism 9 version:
public class ViewerViewModel : BindableBase
{
    // 내부 인덱스 (0-based)
    // Internal index (0-based)
    private int _currentSliceIndex;
    public int CurrentSliceIndex
    {
        get => _currentSliceIndex;
        set
        {
            if (SetProperty(ref _currentSliceIndex, value))
            {
                RaisePropertyChanged(nameof(SliceDisplayNumber));
            }
        }
    }

    // 총 개수
    // Total count
    private int _totalSliceCount;
    public int TotalSliceCount
    {
        get => _totalSliceCount;
        set
        {
            if (SetProperty(ref _totalSliceCount, value))
            {
                RaisePropertyChanged(nameof(MaxSliceIndex));
            }
        }
    }

    /// <summary>
    /// Slider Maximum (0-based index maximum)
    /// </summary>
    public int MaxSliceIndex => Math.Max(0, TotalSliceCount - 1);

    /// <summary>
    /// 사용자 표시 번호 (1-based)
    /// User display number (1-based)
    /// </summary>
    public int SliceDisplayNumber => CurrentSliceIndex + 1;
}
```

## XAML (SKILL.md와 동일)

```xml
<TextBlock Text="{Binding SliceDisplayNumber}" />
<Slider Minimum="0"
        Maximum="{Binding MaxSliceIndex}"
        Value="{Binding CurrentSliceIndex}" />
<TextBlock Text="{Binding TotalSliceCount}" />
```

## Key Differences from SKILL.md

- **SetProperty + RaisePropertyChanged**: `[NotifyPropertyChangedFor]` 대신 `SetProperty()` 성공 시 수동으로 `RaisePropertyChanged()` 호출
- **partial class 불필요**: 소스 제너레이터를 사용하지 않으므로 `partial` 키워드 불필요
- **XAML 동일**: 바인딩 패턴은 변경 없음
