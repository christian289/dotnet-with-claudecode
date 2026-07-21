using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// The studio screen: scene objects, tool state, undo/redo, lighting, the
/// tutorial overlay, and the XAML export modal. Mirrors the design's studio
/// state and mutation semantics (snapshot-based undo capped at 40 entries).
/// </summary>
public sealed partial class StudioViewModel : ObservableObject
{
    private const int MaxUndo = 40;

    private readonly ShellViewModel _shell;
    private readonly Dictionary<ShapeKind, int> _counters = [];
    private readonly List<string> _undoStack = [];
    private readonly List<string> _redoStack = [];
    private bool _quiet;

    public StudioViewModel(ShellViewModel shell)
    {
        _shell = shell;
        Objects.CollectionChanged += (_, _) => UpdateSceneMeta();
    }

    public ShellViewModel Shell => _shell;

    public ObservableCollection<SceneObjectViewModel> Objects { get; } = [];

    public IReadOnlyList<ShapeDefinition> SolidShapes => ShapeCatalog.Solids;

    public IReadOnlyList<ShapeDefinition> CloudShapes => ShapeCatalog.Clouds;

    public IReadOnlyList<ToolDefinition> Tools => ToolCatalog.All;

    public IReadOnlyList<string> Swatches => ShapeCatalog.Swatches;

    [NotifyPropertyChangedFor(nameof(SelectedId))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(NoSelection))]
    [ObservableProperty] private SceneObjectViewModel? _selectedObject;

    [NotifyPropertyChangedFor(nameof(StatusHint))]
    [ObservableProperty] private string _tool = ToolCatalog.Select;

    [NotifyPropertyChangedFor(nameof(LightIntensityLabel))]
    [ObservableProperty] private double _lightIntensity = 1.1;

    [NotifyPropertyChangedFor(nameof(LightAngleLabel))]
    [ObservableProperty] private double _lightAngle = 35;

    [NotifyPropertyChangedFor(nameof(StudioContext))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyHint))]
    [ObservableProperty] private TutorialViewModel? _tutorial;

    [ObservableProperty] private XamlExportViewModel? _export;

    [NotifyPropertyChangedFor(nameof(StudioContext))]
    [ObservableProperty] private bool _freeMode = true;

    [ObservableProperty] private bool _canUndo;

    [ObservableProperty] private bool _canRedo;

    /// <summary>Raised when a camera preset button is pressed ("front"/"top"/"side"/"iso").</summary>
    public event Action<string>? ViewPresetRequested;

    /// <summary>Raised when generated code should be placed on the clipboard (view-side concern).</summary>
    public event Action<string>? CopyToClipboardRequested;

    /// <summary>Folder picker supplied by the view layer for the project export.</summary>
    public Func<string?>? PickFolderRequested { get; set; }

    /// <summary>Set by the view: opens the exported .slnx in Visual Studio; returns an error message or null on success.</summary>
    public Func<string, string?>? OpenInVisualStudioRequested { get; set; }

    public event Action<string>? OpenFolderRequested;

    public string? SelectedId => SelectedObject?.Id;

    public bool HasSelection => SelectedObject is not null;

    public bool NoSelection => SelectedObject is null;

    public string StatusHint => ToolCatalog.HintFor(Tool);

    public string LightIntensityLabel => LightIntensity.ToString("0.00", CultureInfo.InvariantCulture);

    public string LightAngleLabel => string.Create(CultureInfo.InvariantCulture, $"{LightAngle:0}°");

    public bool SceneEmpty => Objects.Count == 0;

    public bool ShowEmptyHint => Objects.Count == 0 && Tutorial is null;

    public string ObjCount => Objects.Count.ToString(CultureInfo.InvariantCulture);

    public string ObjCountLabel => $"오브젝트 {Objects.Count}개 · 폴리랩 v0.1";

    public string StudioContext => FreeMode || Tutorial is null
        ? "자유 모드 · Free Play"
        : $"코스 {int.Parse(Tutorial.Course.Num, CultureInfo.InvariantCulture)} · {Tutorial.Lesson.Title}";

    // ---------------- scene mutations ----------------

    [RelayCommand]
    private void AddShape(ShapeDefinition def)
    {
        int n = _counters.GetValueOrDefault(def.Kind) + 1;
        int k = Objects.Count;
        double gx = Math.Round((k % 3) - 1.0, 1) * 1.7;
        double gz = Math.Round((k / 3 % 3) - 1.0, 1) * 1.7;
        string color = ShapeCatalog.AddPalette[k % ShapeCatalog.AddPalette.Count];

        PushUndo();
        var vm = new SceneObjectViewModel(new SceneObjectState(
            NewId(), def.Kind, $"{def.Label} {n}",
            Math.Round(gx, 1), def.DefaultY, Math.Round(gz, 1),
            0, 1, color, 0.55, 0.05,
            PointCount: def.IsCloud ? PointCloudFactory.DefaultCount(def.Kind) : 0))
        {
            Notify = CheckTask,
        };
        Objects.Add(vm);
        SelectedObject = vm;
        _counters[def.Kind] = n;

        CheckTask(TaskEvent.ForAdd(def.Kind));
        CheckTask(TaskEvent.ForCount(def.Kind));
    }

    [RelayCommand]
    private void Duplicate()
    {
        if (SelectedObject is not { } sel)
        {
            return;
        }

        ShapeDefinition def = ShapeCatalog.Get(sel.Kind);
        int n = _counters.GetValueOrDefault(sel.Kind) + 1;

        PushUndo();
        SceneObjectState copy = sel.ToState() with
        {
            Id = NewId(),
            Name = $"{def.Label} {n}",
            X = sel.X + 0.9,
            Z = sel.Z + 0.3,
        };
        var vm = new SceneObjectViewModel(copy) { Notify = CheckTask };
        Objects.Add(vm);
        SelectedObject = vm;
        _counters[sel.Kind] = n;
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedObject is not { } sel)
        {
            return;
        }

        PushUndo();
        Objects.Remove(sel);
        SelectedObject = null;
        CheckTask(TaskEvent.Delete);
    }

    [RelayCommand]
    private void RemoveObject(SceneObjectViewModel obj)
    {
        PushUndo();
        Objects.Remove(obj);
        if (SelectedObject == obj)
        {
            SelectedObject = null;
        }

        CheckTask(TaskEvent.Delete);
    }

    [RelayCommand]
    private void SelectObject(SceneObjectViewModel obj)
    {
        SelectedObject = obj;
        CheckTask(TaskEvent.Select);
    }

    [RelayCommand]
    private void PickColor(string hex)
    {
        if (SelectedObject is not { } sel)
        {
            return;
        }

        PushUndo();
        sel.ColorHex = hex;
        CheckTask(TaskEvent.Color);
    }

    [RelayCommand]
    private void SetTool(string tool)
    {
        Tool = tool;
        CheckTask(TaskEvent.ForTool(tool));
    }

    [RelayCommand]
    private void Undo()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        string snap = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);
        PushSnapshot(_redoStack, Serialize());
        RestoreSnapshot(snap);
    }

    [RelayCommand]
    private void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        string snap = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        PushSnapshot(_undoStack, Serialize());
        RestoreSnapshot(snap);
    }

    // ---------------- navigation / modal commands ----------------

    [RelayCommand]
    private void GoStart() => _shell.GoStart();

    [RelayCommand]
    private void GoSettings() => _shell.GoSettings();

    [RelayCommand]
    private void OpenXaml()
    {
        Export = new XamlExportViewModel(
            Objects.Count,
            () => [.. Objects.Select(o => o.ToState())],
            () => (LightIntensity, LightAngle),
            text => CopyToClipboardRequested?.Invoke(text),
            () => PickFolderRequested?.Invoke(),
            path => OpenFolderRequested?.Invoke(path),
            slnx => OpenInVisualStudioRequested?.Invoke(slnx),
            CheckTask,
            () => Export = null);
        CheckTask(TaskEvent.ForAction("xaml"));
    }

    [RelayCommand]
    private void RequestView(string preset)
    {
        ViewPresetRequested?.Invoke(preset);
        CheckTask(TaskEvent.ForAction("preset"));
    }

    // ---------------- viewport callbacks ----------------

    public void OnViewportSelect(string? id)
    {
        SelectedObject = id is null ? null : Objects.FirstOrDefault(o => o.Id == id);
        if (id is not null)
        {
            CheckTask(TaskEvent.Select);
        }
    }

    public void OnViewportTransform(string id, double x, double y, double z, double ry, double scale, string mode, bool shift)
    {
        if (Objects.FirstOrDefault(o => o.Id == id) is not { } obj)
        {
            return;
        }

        PushUndo();
        obj.Quiet = true;
        obj.X = x;
        obj.Y = y;
        obj.Z = z;
        obj.RotationY = ry;
        obj.Scale = scale;
        obj.Quiet = false;
        CheckTask(TaskEvent.ForTransform(mode, shift));
    }

    public void OnViewportAction(string name) => CheckTask(TaskEvent.ForAction(name));

    public void ClearSelection() => SelectedObject = null;

    // ---------------- lesson lifecycle ----------------

    public void LoadLesson(Course course, int lessonIndex)
    {
        Lesson lesson = course.Lessons[lessonIndex];

        _quiet = true;
        Objects.Clear();
        _counters.Clear();
        foreach (SceneObjectState state in SceneSeeds.For(lesson.Seed))
        {
            Objects.Add(new SceneObjectViewModel(state) { Notify = CheckTask });
            _counters[state.Kind] = _counters.GetValueOrDefault(state.Kind) + 1;
        }

        SelectedObject = null;
        Tool = ToolCatalog.Select;
        _undoStack.Clear();
        _redoStack.Clear();
        CanUndo = false;
        CanRedo = false;
        LightIntensity = 1.1;
        LightAngle = 35;
        _quiet = false;

        FreeMode = false;
        Tutorial = new TutorialViewModel(
            course,
            lessonIndex,
            onExitToCourses: () =>
            {
                Tutorial = null;
                _shell.GoCourses();
            },
            onContinueFree: () =>
            {
                Tutorial = null;
                FreeMode = true;
            },
            onCompleted: () => _shell.MarkDone(course.Id, lesson));
    }

    public void EnterFreeMode()
    {
        FreeMode = true;
        Tutorial = null;
    }

    public int CountOf(ShapeKind kind) => Objects.Count(o => o.Kind == kind);

    // ---------------- helpers ----------------

    private void CheckTask(TaskEvent evt)
    {
        if (!_quiet)
        {
            Tutorial?.Advance(evt, CountOf);
        }
    }

    partial void OnLightIntensityChanged(double value)
    {
        if (!_quiet)
        {
            CheckTask(TaskEvent.ForLight("intensity"));
        }
    }

    partial void OnLightAngleChanged(double value)
    {
        if (!_quiet)
        {
            CheckTask(TaskEvent.ForLight("angle"));
        }
    }

    private static string NewId() =>
        $"o{Environment.TickCount64}{Random.Shared.Next(1000)}";

    private void UpdateSceneMeta()
    {
        OnPropertyChanged(nameof(SceneEmpty));
        OnPropertyChanged(nameof(ShowEmptyHint));
        OnPropertyChanged(nameof(ObjCount));
        OnPropertyChanged(nameof(ObjCountLabel));
    }

    private string Serialize() =>
        JsonSerializer.Serialize(Objects.Select(o => o.ToState()).ToList());

    private void PushUndo()
    {
        PushSnapshot(_undoStack, Serialize());
        _redoStack.Clear();
        CanUndo = true;
        CanRedo = false;
    }

    private static void PushSnapshot(List<string> stack, string snapshot)
    {
        stack.Add(snapshot);
        if (stack.Count > MaxUndo)
        {
            stack.RemoveAt(0);
        }
    }

    private void RestoreSnapshot(string snapshot)
    {
        List<SceneObjectState> states = JsonSerializer.Deserialize<List<SceneObjectState>>(snapshot) ?? [];

        _quiet = true;
        Objects.Clear();
        foreach (SceneObjectState state in states)
        {
            Objects.Add(new SceneObjectViewModel(state) { Notify = CheckTask });
        }

        SelectedObject = null;
        _quiet = false;

        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }
}
