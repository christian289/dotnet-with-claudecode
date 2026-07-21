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
    private readonly Func<string?> _pickFolder;
    private readonly Action<string> _openFolder;
    private readonly Func<string, string?> _openInVisualStudio;
    private readonly Action<TaskEvent> _notify;
    private readonly Action _close;

    public XamlExportViewModel(
        int objectCount,
        Func<IReadOnlyList<SceneObjectState>> snapshot,
        Func<(double Intensity, double Angle)> light,
        Action<string> copyToClipboard,
        Func<string?> pickFolder,
        Action<string> openFolder,
        Func<string, string?> openInVisualStudio,
        Action<TaskEvent> notify,
        Action close)
    {
        Summary = $"오브젝트 {objectCount}개 + 조명 · 현재 장면 기준";
        _snapshot = snapshot;
        _light = light;
        _copyToClipboard = copyToClipboard;
        _pickFolder = pickFolder;
        _openFolder = openFolder;
        _openInVisualStudio = openInVisualStudio;
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

    // ---------------- runnable project export ----------------

    [NotifyPropertyChangedFor(nameof(IsNet8))]
    [NotifyPropertyChangedFor(nameof(IsNet9))]
    [NotifyPropertyChangedFor(nameof(IsNet10))]
    [ObservableProperty] private string _tfm = "net10.0-windows";

    [ObservableProperty] private string? _exportedPath;

    [ObservableProperty] private string? _exportError;

    public bool IsNet8 => Tfm == "net8.0-windows";

    public bool IsNet9 => Tfm == "net9.0-windows";

    public bool IsNet10 => Tfm == "net10.0-windows";

    [RelayCommand]
    private void PickTfm(string tfm) => Tfm = tfm;

    [RelayCommand]
    private void ExportProject()
    {
        if (_pickFolder() is not { Length: > 0 } folder)
        {
            return;
        }

        (double intensity, double angle) = _light();
        string xaml = Variant == "declarative"
            ? WpfSceneCodeGenerator.GenerateDeclarativeXaml(_snapshot(), intensity, angle)
            : WpfSceneCodeGenerator.GenerateXaml();
        string cs = Variant == "declarative"
            ? WpfSceneCodeGenerator.GenerateDeclarativeCs()
            : WpfSceneCodeGenerator.GenerateCs(_snapshot(), intensity, angle);

        try
        {
            ExportedPath = WpfProjectExporter.Export(folder, Tfm, xaml, cs);
            ExportError = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ExportedPath = null;
            ExportError = $"내보내기에 실패했어요: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenExportedFolder()
    {
        if (ExportedPath is { Length: > 0 } path)
        {
            _openFolder(path);
        }
    }

    [RelayCommand]
    private void OpenInVisualStudio()
    {
        if (ExportedPath is not { Length: > 0 } directory)
        {
            return;
        }

        string solutionPath = Path.Combine(directory, "PolyLabScene.slnx");
        ExportError = _openInVisualStudio(solutionPath); // null on success
    }
}
