namespace PolyLab3DStudio.Core;

/// <summary>
/// Completion condition of one tutorial task. <see cref="Type"/> selects which
/// optional fields apply (mirrors the design's check objects):
/// action / add / count / tool / transform / slider / light / select / delete / color.
/// </summary>
public sealed record TaskCheck(
    string Type,
    string? Action = null,
    ShapeKind? Shape = null,
    int Count = 0,
    string? Tool = null,
    string? Mode = null,
    bool Shift = false,
    string? Key = null);
