namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// One row of the tool comparison table. Cell text accepts <c>**bold**</c>
/// markup; <paramref name="IsMono"/> renders cells in the mono font and
/// <paramref name="IsLast"/> drops the bottom separator.
/// </summary>
public sealed record ComparisonRow(
    string Label,
    string Gimp,
    string Inkscape,
    string Blender,
    bool IsMono = false,
    bool IsLast = false);
