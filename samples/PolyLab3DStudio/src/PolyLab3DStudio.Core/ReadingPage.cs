namespace PolyLab3DStudio.Core;

public sealed record ReadingPage(string Title, IReadOnlyList<string> Paragraphs, string? Code = null);
