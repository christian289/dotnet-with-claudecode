namespace PolyLab3DStudio.Core;

/// <summary>
/// One creatable shape: Korean label, English name, spawn height, and the icon
/// path data from the design (WPF geometry mini-language compatible).
/// </summary>
public sealed record ShapeDefinition(
    ShapeKind Kind,
    string Label,
    string English,
    double DefaultY,
    string IconData,
    bool IsCloud);
