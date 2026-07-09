using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// XAML export modal: two code tabs generated from the live scene, a copy button
/// with the 1.8s "복사됨 ✓" flip, and task events for the WPF mission.
/// </summary>
public sealed partial class XamlExportViewModel : ObservableObject
{
    private readonly Func<IReadOnlyList<SceneObjectState>> _snapshot;
    private readonly Func<(double Intensity, double Angle)> _light;
    private readonly Action<string> _copyToClipboard;
    private readonly Action<TaskEvent> _notify;
    private readonly Action _close;

    public XamlExportViewModel(
        int objectCount,
        Func<IReadOnlyList<SceneObjectState>> snapshot,
        Func<(double Intensity, double Angle)> light,
        Action<string> copyToClipboard,
        Action<TaskEvent> notify,
        Action close)
    {
        Summary = $"오브젝트 {objectCount}개 + 조명 · 현재 장면 기준";
        _snapshot = snapshot;
        _light = light;
        _copyToClipboard = copyToClipboard;
        _notify = notify;
        _close = close;
    }

    public string Summary { get; }

    [NotifyPropertyChangedFor(nameof(CodeText))]
    [NotifyPropertyChangedFor(nameof(IsXamlTab))]
    [NotifyPropertyChangedFor(nameof(IsCsTab))]
    [ObservableProperty] private string _tab = "xaml";

    [NotifyPropertyChangedFor(nameof(CodeText))]
    [NotifyPropertyChangedFor(nameof(IsCodeBehindVariant))]
    [NotifyPropertyChangedFor(nameof(IsDeclarativeVariant))]
    [ObservableProperty] private string _variant = "codeBehind";

    [NotifyPropertyChangedFor(nameof(CopyLabel))]
    [ObservableProperty] private bool _copied;

    public bool IsXamlTab => Tab == "xaml";

    public bool IsCsTab => Tab == "cs";

    public bool IsCodeBehindVariant => Variant == "codeBehind";

    public bool IsDeclarativeVariant => Variant == "declarative";

    public string CopyLabel => Copied ? "복사됨 ✓" : "코드 복사";

    public string CodeText
    {
        get
        {
            (double intensity, double angle) = _light();
            if (Variant == "declarative")
            {
                return Tab == "xaml"
                    ? WpfSceneCodeGenerator.GenerateDeclarativeXaml(_snapshot(), intensity, angle)
                    : WpfSceneCodeGenerator.GenerateDeclarativeCs();
            }

            return Tab == "xaml"
                ? WpfSceneCodeGenerator.GenerateXaml()
                : WpfSceneCodeGenerator.GenerateCs(_snapshot(), intensity, angle);
        }
    }

    [RelayCommand]
    private void PickCodeBehindVariant()
    {
        Variant = "codeBehind";
        Copied = false;
    }

    [RelayCommand]
    private void PickDeclarativeVariant()
    {
        Variant = "declarative";
        Copied = false;
    }

    [RelayCommand]
    private void PickXamlTab()
    {
        Tab = "xaml";
        Copied = false;
    }

    [RelayCommand]
    private void PickCsTab()
    {
        Tab = "cs";
        Copied = false;
        _notify(TaskEvent.ForAction("cstab"));
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        _copyToClipboard(CodeText);
        _notify(TaskEvent.ForAction("copy"));
        Copied = true;

        await Task.Delay(1800);
        Copied = false;
    }

    [RelayCommand]
    private void Close() => _close();
}
