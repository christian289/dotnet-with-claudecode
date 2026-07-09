namespace PolyLab3DStudio.Controls;

/// <summary>Result of a finished tool drag, reported by <see cref="PolyViewport"/>.</summary>
public sealed record TransformCommit(
    string Id,
    double X,
    double Y,
    double Z,
    double RotationYDeg,
    double Scale,
    string Mode,
    bool Shift);
