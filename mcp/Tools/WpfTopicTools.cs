using System.ComponentModel;
using ModelContextProtocol.Server;
using WpfDevPackMcp.Knowledge;

namespace WpfDevPackMcp.Tools;

[McpServerToolType]
public sealed class WpfTopicTools(KnowledgeService knowledge)
{
    [McpServerTool(Name = "list_wpf_topics")]
    [Description("Lists all WPF knowledge topics with a one-line summary and available companion files (PRISM.md, ADVANCED.md, references).")]
    public IReadOnlyList<Topic> ListWpfTopics()
    {
        knowledge.EnsureReady();
        return knowledge.Catalog.List();
    }

    [McpServerTool(Name = "get_wpf_topic")]
    [Description("Returns the full markdown for one WPF knowledge topic. variant: 'default' (TOPIC.md), 'prism' (PRISM.md), or 'advanced' (ADVANCED.md).")]
    public string GetWpfTopic(
        [Description("Topic id, e.g. 'implementing-communitytoolkit-mvvm' (a knowledge/ directory name).")] string id,
        [Description("Which variant to return: default | prism | advanced.")] string variant = "default")
    {
        knowledge.EnsureReady();
        try
        {
            return knowledge.Catalog.GetContent(id, variant);
        }
        catch (TopicNotFoundException)
        {
            var ids = string.Join(", ", knowledge.Catalog.List().Select(t => t.Id));
            return $"Topic or variant not found: id='{id}', variant='{variant}'.\nAvailable ids: {ids}";
        }
    }

    [McpServerTool(Name = "search_wpf_topics")]
    [Description("Searches WPF knowledge topics by keyword over id, title, summary, and body. Returns ranked matches.")]
    public IReadOnlyList<SearchHit> SearchWpfTopics(
        [Description("Search query (one or more keywords).")] string query,
        [Description("Maximum number of results (default 8).")] int maxResults = 8)
    {
        knowledge.EnsureReady();
        return knowledge.Catalog.Search(query, maxResults);
    }

    [McpServerTool(Name = "refresh_wpf_knowledge")]
    [Description("Forces an immediate git pull of the knowledge repo and rescans the catalog. Returns the topic count after refresh.")]
    public string RefreshWpfKnowledge()
    {
        knowledge.EnsureReady(force: true);
        return $"Refreshed. {knowledge.Catalog.List().Count} topics available.";
    }
}
