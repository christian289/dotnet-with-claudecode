namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// Shared structure of the GIMP / Inkscape / Blender sections: four concept
/// bullets (with <c>**bold**</c> markup), a four-step workflow, shortcut chips,
/// and a closing footnote.
/// </summary>
public sealed record ToolGuideSection(
    string Number,
    string Title,
    string Tagline,
    IReadOnlyList<string> Bullets,
    IReadOnlyList<string> WorkflowSteps,
    IReadOnlyList<ShortcutChip> Shortcuts,
    string Footnote);
