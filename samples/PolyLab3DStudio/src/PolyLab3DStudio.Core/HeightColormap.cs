namespace PolyLab3DStudio.Core;

/// <summary>
/// Height (Y) colormap for point clouds, low → high, built from the studio's
/// own palette. Single source for the viewport and both export generators.
/// </summary>
public static class HeightColormap
{
    public static IReadOnlyList<(double Offset, string Hex)> Stops { get; } =
    [
        (0.0, "#4C8DF5"),
        (0.25, "#37A6A0"),
        (0.5, "#7BC96F"),
        (0.75, "#FFC24B"),
        (1.0, "#F0562F"),
    ];
}
