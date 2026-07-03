using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

public sealed partial class SceneObjectViewModel(PrimitiveKind kind, string name, string colorHex) : ObservableObject
{
    public PrimitiveKind Kind { get; } = kind;

    [ObservableProperty] private string _name = name;

    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _positionY;
    [ObservableProperty] private double _positionZ;

    [ObservableProperty] private double _rotationX;
    [ObservableProperty] private double _rotationY;
    [ObservableProperty] private double _rotationZ;

    [ObservableProperty] private double _scale = 1.0;

    [ObservableProperty] private string _colorHex = colorHex;
}
