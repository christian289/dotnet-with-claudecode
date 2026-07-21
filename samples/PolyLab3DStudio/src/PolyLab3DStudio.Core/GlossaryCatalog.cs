using System.Xml.Linq;

namespace PolyLab3DStudio.Core;

/// <summary>
/// The shared PolyLab glossary. The data lives in the embedded
/// <c>Glossary.xml</c> resource (originally transcribed from the design's
/// <c>glossary.js</c>) so contributors can add or edit terms without touching
/// C# code. Feeds the 3D 사전 screen and the term-tip hover cards on the
/// reference screens.
/// </summary>
public static class GlossaryCatalog
{
    private const string ResourceName = "PolyLab3DStudio.Core.Glossary.xml";

    /// <summary>Category filter list for the 3D 사전 screen (전체 first).</summary>
    public static IReadOnlyList<string> Categories { get; }

    public static IReadOnlyList<GlossaryTerm> All { get; }

    static GlossaryCatalog()
    {
        using Stream stream = typeof(GlossaryCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded glossary resource '{ResourceName}' is missing.");

        XElement glossary = XDocument.Load(stream).Root
            ?? throw new InvalidOperationException("Glossary.xml has no root element.");

        Categories = [.. RequiredElement(glossary, "categories").Elements("category").Select(c => c.Value)];
        All = [.. RequiredElement(glossary, "terms").Elements("term").Select(ParseTerm)];
    }

    private static GlossaryTerm ParseTerm(XElement term) => new(
        RequiredAttribute(term, "word"),
        RequiredAttribute(term, "english"),
        RequiredAttribute(term, "category"),
        RequiredElement(term, "short").Value,
        RequiredElement(term, "detail").Value,
        (string?)term.Attribute("docUrl"));

    private static XElement RequiredElement(XElement parent, string name) =>
        parent.Element(name)
        ?? throw new InvalidOperationException(
            $"Glossary.xml: <{parent.Name}> element '{(string?)parent.Attribute("word") ?? parent.Name.LocalName}' is missing required child <{name}>.");

    private static string RequiredAttribute(XElement term, string name) =>
        (string?)term.Attribute(name)
        ?? throw new InvalidOperationException(
            $"Glossary.xml: <term> '{(string?)term.Attribute("word") ?? "(no word)"}' is missing required attribute '{name}'.");
}
