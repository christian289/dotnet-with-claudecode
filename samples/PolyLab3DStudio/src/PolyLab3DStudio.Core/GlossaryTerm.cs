namespace PolyLab3DStudio.Core;

/// <summary>
/// One glossary entry of the shared PolyLab term dictionary
/// (design's <c>glossary.js</c>): word, English name, category,
/// short tooltip text, and the full dictionary description.
/// WPF-specific terms additionally carry a Microsoft Learn doc link.
/// </summary>
public sealed record GlossaryTerm(
    string Word,
    string English,
    string Category,
    string Short,
    string Detail,
    string? DocUrl = null);
