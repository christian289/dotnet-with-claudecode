namespace PolyLab3DStudio.Core;

/// <summary>
/// Persisted user preferences. <see cref="English"/> is nullable: null means
/// "never toggled", falling back to the default (off), matching the design.
/// </summary>
public sealed record UserSettings(
    bool? English = null,
    bool Grid = true,
    double Sensitivity = 1.0,
    bool InvertZoom = false)
{
    public bool EnglishOn => English ?? false;
}
