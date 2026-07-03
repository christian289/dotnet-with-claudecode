using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    // Low-poly studio palette: warm coral, sand, sage, sky, lavender, slate.
    public static IReadOnlyList<string> Palette { get; } =
    [
        "#E0705A", "#E5A84B", "#8FBF7F", "#5FA8C9", "#9B8AC4", "#7D8A99",
    ];

    private static readonly Dictionary<PrimitiveKind, string> KindLabels = new()
    {
        [PrimitiveKind.Cube] = "Cube",
        [PrimitiveKind.Sphere] = "Sphere",
        [PrimitiveKind.Cylinder] = "Cylinder",
        [PrimitiveKind.Cone] = "Cone",
        [PrimitiveKind.Torus] = "Torus",
        [PrimitiveKind.Plane] = "Plane",
    };

    private readonly Dictionary<PrimitiveKind, int> _nameCounters = [];
    private int _paletteCursor;

    public ObservableCollection<SceneObjectViewModel> SceneObjects { get; } = [];

    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateSelectedCommand))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [ObservableProperty] private SceneObjectViewModel? _selectedObject;

    [ObservableProperty] private string _statusMessage = "Ready";

    public bool HasSelection => SelectedObject is not null;

    public int ObjectCount => SceneObjects.Count;

    public MainViewModel()
    {
        SceneObjects.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ObjectCount));

        // Seed the scene so the viewport is not empty on first launch.
        AddPrimitive(PrimitiveKind.Cube);
        AddPrimitive(PrimitiveKind.Sphere);
        SelectedObject = SceneObjects[0];

        SceneObjects[1].PositionX = 1.4;
        SceneObjects[1].PositionZ = -0.6;
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void AddPrimitive(PrimitiveKind kind)
    {
        int count = _nameCounters.TryGetValue(kind, out int n) ? n + 1 : 1;
        _nameCounters[kind] = count;

        string color = Palette[_paletteCursor];
        _paletteCursor = (_paletteCursor + 1) % Palette.Count;

        SceneObjectViewModel item = new(kind, $"{KindLabels[kind]} {count}", color);
        SceneObjects.Add(item);
        SelectedObject = item;
        StatusMessage = $"Added {item.Name}";
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void RemoveSelected()
    {
        if (SelectedObject is not SceneObjectViewModel target)
        {
            return;
        }

        int index = SceneObjects.IndexOf(target);
        SceneObjects.Remove(target);
        SelectedObject = SceneObjects.Count > 0 ? SceneObjects[Math.Min(index, SceneObjects.Count - 1)] : null;
        StatusMessage = $"Removed {target.Name}";
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void DuplicateSelected()
    {
        if (SelectedObject is not SceneObjectViewModel source)
        {
            return;
        }

        AddPrimitive(source.Kind);

        SceneObjectViewModel copy = SceneObjects[^1];
        copy.PositionX = source.PositionX + 0.5;
        copy.PositionY = source.PositionY;
        copy.PositionZ = source.PositionZ + 0.5;
        copy.RotationX = source.RotationX;
        copy.RotationY = source.RotationY;
        copy.RotationZ = source.RotationZ;
        copy.Scale = source.Scale;
        copy.ColorHex = source.ColorHex;
        StatusMessage = $"Duplicated {source.Name}";
    }

    [RelayCommand]
    private void ClearScene()
    {
        SceneObjects.Clear();
        SelectedObject = null;
        _nameCounters.Clear();
        _paletteCursor = 0;
        StatusMessage = "Scene cleared";
    }

    [RelayCommand]
    private void ApplyColor(string colorHex)
    {
        if (SelectedObject is not SceneObjectViewModel target)
        {
            return;
        }

        target.ColorHex = colorHex;
        StatusMessage = $"Color applied to {target.Name}";
    }
}
