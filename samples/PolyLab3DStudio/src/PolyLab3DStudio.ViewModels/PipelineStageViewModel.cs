namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// One clickable stage card in the render-pipeline diagram
/// (design screen "WPF 렌더링 파이프라인").
/// </summary>
public sealed partial class PipelineStageViewModel(
    string num,
    string name,
    string sub,
    string icon,
    bool isUiLane,
    bool isLastInLane,
    IReadOnlyList<string> paras,
    Action<PipelineStageViewModel> select) : ObservableObject
{
    public string Num { get; } = num;

    public string Name { get; } = name;

    public string Sub { get; } = sub;

    /// <summary>Stroke-icon path data (geometry mini-language), verbatim from the design.</summary>
    public string Icon { get; } = icon;

    public bool IsUiLane { get; } = isUiLane;

    /// <summary>True for the last stage of a lane — hides the trailing flow arrow.</summary>
    public bool IsLastInLane { get; } = isLastInLane;

    /// <summary>Detail-panel paragraphs shown while this stage is selected.</summary>
    public IReadOnlyList<string> Paras { get; } = paras;

    public string LaneLabel => IsUiLane ? "UI 스레드" : "렌더 스레드";

    [ObservableProperty] private bool _isSelected;

    [RelayCommand]
    private void Select() => select(this);
}
