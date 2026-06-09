using WpfDevPackMcp.Knowledge;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class TopicCatalogTests : IDisposable
{
    private readonly string _root;          // repo root
    private readonly string _knowledge;     // <root>/knowledge

    public TopicCatalogTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "wdpmcp-" + Guid.NewGuid().ToString("N"));
        _knowledge = Path.Combine(_root, "knowledge");
        WriteTopic("implementing-communitytoolkit-mvvm",
            "# CommunityToolkit MVVM\n\n> Implements MVVM using CommunityToolkit.\n\nbody",
            prism: "# Prism variant\n");
        WriteTopic("virtualizing-wpf-ui",
            "# Virtualizing\n\n> Implements WPF UI virtualization for large data sets.\n\nbody");
    }

    private void WriteTopic(string id, string topic, string? prism = null)
    {
        var dir = Path.Combine(_knowledge, id);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "TOPIC.md"), topic);
        if (prism is not null)
        {
            File.WriteAllText(Path.Combine(dir, "PRISM.md"), prism);
        }
    }

    [Fact]
    public void List_ReturnsAllTopics_SortedById()
    {
        var catalog = new TopicCatalog(_root);
        var ids = catalog.List().Select(t => t.Id).ToArray();
        Assert.Equal(["implementing-communitytoolkit-mvvm", "virtualizing-wpf-ui"], ids);
    }

    [Fact]
    public void List_PopulatesSummaryFromBlockquote_AndCompanions()
    {
        var catalog = new TopicCatalog(_root);
        var mvvm = catalog.List().Single(t => t.Id == "implementing-communitytoolkit-mvvm");
        Assert.Equal("Implements MVVM using CommunityToolkit.", mvvm.Summary);
        Assert.Contains("PRISM.md", mvvm.Companions);
    }

    [Fact]
    public void GetContent_Default_ReturnsTopicMd()
    {
        var catalog = new TopicCatalog(_root);
        var content = catalog.GetContent("virtualizing-wpf-ui", "default");
        Assert.Contains("# Virtualizing", content);
    }

    [Fact]
    public void GetContent_Prism_ReturnsPrismMd()
    {
        var catalog = new TopicCatalog(_root);
        var content = catalog.GetContent("implementing-communitytoolkit-mvvm", "prism");
        Assert.Contains("# Prism variant", content);
    }

    [Fact]
    public void GetContent_UnknownId_Throws()
        => Assert.Throws<TopicNotFoundException>(() => new TopicCatalog(_root).GetContent("nope", "default"));

    [Fact]
    public void Search_MatchesTitleAndSummary_RanksHigher()
    {
        var catalog = new TopicCatalog(_root);
        var hits = catalog.Search("mvvm", maxResults: 5);
        Assert.NotEmpty(hits);
        Assert.Equal("implementing-communitytoolkit-mvvm", hits[0].Id);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
        => Assert.Empty(new TopicCatalog(_root).Search("zzz-nonexistent", maxResults: 5));

    [Fact]
    public void GetContent_UnknownVariant_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() =>
            new TopicCatalog(_root).GetContent("virtualizing-wpf-ui", "PRISM"));

    [Fact]
    public void Variants_ReturnsPrismAndDefault_WhenPrismExists()
    {
        var variants = new TopicCatalog(_root).Variants("implementing-communitytoolkit-mvvm");
        Assert.Contains("default", variants);
        Assert.Contains("prism", variants);
    }

    [Fact]
    public void Variants_DefaultOnly_WhenNoCompanions()
        => Assert.Equal(["default"], new TopicCatalog(_root).Variants("virtualizing-wpf-ui"));

    [Fact]
    public void Invalidate_ClearsCache_NewTopicBecomesVisible()
    {
        var catalog = new TopicCatalog(_root);
        var before = catalog.List().Count;
        WriteTopic("zzz-new-topic", "# New\n\n> new summary\n\nbody");
        Assert.Equal(before, catalog.List().Count);   // cache still warm
        catalog.Invalidate();
        Assert.Equal(before + 1, catalog.List().Count); // refreshed
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
