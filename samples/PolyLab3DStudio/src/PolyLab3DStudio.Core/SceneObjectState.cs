namespace PolyLab3DStudio.Core;

/// <summary>
/// Immutable snapshot of one scene object — the unit of undo/redo history,
/// seeding, and code generation.
/// </summary>
public sealed record SceneObjectState(
    string Id,
    ShapeKind Kind,
    string Name,
    double X,
    double Y,
    double Z,
    double RotationY,
    double Scale,
    string ColorHex,
    double Roughness,
    double Metalness,
    bool HeightColormap = false,
    int PointCount = 0);
