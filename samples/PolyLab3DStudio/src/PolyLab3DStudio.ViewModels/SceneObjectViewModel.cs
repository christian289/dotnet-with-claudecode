using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// Observable wrapper around one scene object. Property edits raise the design's
/// task events through <see cref="Notify"/> (rotate/scale → transform, roughness/
/// metalness → slider); position edits raise none, matching the design.
/// </summary>
public sealed partial class SceneObjectViewModel : ObservableObject
{
    public SceneObjectViewModel(SceneObjectState state)
    {
        Id = state.Id;
        Kind = state.Kind;
        Name = state.Name;
        _x = state.X;
        _y = state.Y;
        _z = state.Z;
        _rotationY = state.RotationY;
        _scale = state.Scale;
        _colorHex = state.ColorHex;
        _roughness = state.Roughness;
        _metalness = state.Metalness;
        _heightColormap = state.HeightColormap;
        _pointCount = state.PointCount;
    }

    public string Id { get; }

    public ShapeKind Kind { get; }

    public string Name { get; }

    public string IconData => ShapeCatalog.Get(Kind).IconData;

    public bool IsCloud => ShapeCatalog.Get(Kind).IsCloud;

    /// <summary>Set by the owning studio; receives task events from user edits.</summary>
    public Action<TaskEvent>? Notify { get; set; }

    /// <summary>Suppresses task events while state is applied programmatically (undo, drag commit).</summary>
    public bool Quiet { get; set; }

    [ObservableProperty] private double _x;

    [ObservableProperty] private double _y;

    [ObservableProperty] private double _z;

    [NotifyPropertyChangedFor(nameof(RotationYLabel))]
    [ObservableProperty] private double _rotationY;

    [NotifyPropertyChangedFor(nameof(ScaleLabel))]
    [ObservableProperty] private double _scale;

    [ObservableProperty] private string _colorHex;

    [NotifyPropertyChangedFor(nameof(RoughnessLabel))]
    [ObservableProperty] private double _roughness;

    [NotifyPropertyChangedFor(nameof(MetalnessLabel))]
    [ObservableProperty] private double _metalness;

    [ObservableProperty] private bool _heightColormap;

    [NotifyPropertyChangedFor(nameof(PointCountLabel))]
    [ObservableProperty] private int _pointCount;

    public string PointCountLabel => PointCount.ToString("N0", CultureInfo.InvariantCulture);

    public string RotationYLabel => string.Create(CultureInfo.InvariantCulture, $"{RotationY:0}°");

    public string ScaleLabel => string.Create(CultureInfo.InvariantCulture, $"×{Scale:0.##}");

    public string RoughnessLabel => Roughness.ToString("0.00", CultureInfo.InvariantCulture);

    public string MetalnessLabel => Metalness.ToString("0.00", CultureInfo.InvariantCulture);

    public SceneObjectState ToState() =>
        new(Id, Kind, Name, X, Y, Z, RotationY, Scale, ColorHex, Roughness, Metalness, HeightColormap, PointCount);

    public void Apply(SceneObjectState state)
    {
        Quiet = true;
        X = state.X;
        Y = state.Y;
        Z = state.Z;
        RotationY = state.RotationY;
        Scale = state.Scale;
        ColorHex = state.ColorHex;
        Roughness = state.Roughness;
        Metalness = state.Metalness;
        HeightColormap = state.HeightColormap;
        PointCount = state.PointCount;
        Quiet = false;
    }

    partial void OnRotationYChanged(double value) => Emit(TaskEvent.ForTransform("rotate"));

    partial void OnScaleChanged(double value) => Emit(TaskEvent.ForTransform("scale"));

    partial void OnRoughnessChanged(double value) => Emit(TaskEvent.ForSlider("roughness"));

    partial void OnMetalnessChanged(double value) => Emit(TaskEvent.ForSlider("metalness"));

    private void Emit(TaskEvent evt)
    {
        if (!Quiet)
        {
            Notify?.Invoke(evt);
        }
    }
}
