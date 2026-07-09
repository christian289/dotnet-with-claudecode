namespace PolyLab3DStudio.ViewModels;

/// <summary>A keyboard shortcut chip. Accent chips get the orange key badge.</summary>
public sealed record ShortcutChip(string Key, string Label, bool IsAccent = false);
