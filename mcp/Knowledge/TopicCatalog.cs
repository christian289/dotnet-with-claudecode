namespace WpfDevPackMcp.Knowledge;

public sealed class TopicNotFoundException : Exception
{
    public string Id { get; }

    public TopicNotFoundException(string id)
        : base($"Unknown WPF topic id: '{id}'.") => Id = id;

    public TopicNotFoundException(string id, string variant)
        : base($"Unknown WPF topic id: '{id}', variant: '{variant}'.") => Id = id;
}

public sealed record SearchHit(string Id, string Title, string Snippet, int Score);

/// <summary>
/// Scans &lt;repoRoot&gt;/knowledge/*/TOPIC.md into a topic
/// catalog. Reads directly from the local filesystem (a git clone), so a
/// rescan reflects the latest pulled content. Results are cached until
/// <see cref="Invalidate"/> is called.
/// </summary>
public sealed class TopicCatalog(string repoRoot)
{
    private const string DefaultVariant = "default";
    private const int SnippetMaxLength = 160;

    private readonly string _knowledgeRoot =
        Path.Combine(repoRoot, "knowledge");

    private readonly Lock _gate = new();
    private IReadOnlyList<Topic>? _cache;
    private Dictionary<string, string>? _bodies;

    public void Invalidate()
    {
        lock (_gate)
        {
            _cache = null;
            _bodies = null;
        }
    }

    public IReadOnlyList<Topic> List()
    {
        EnsureScanned();
        lock (_gate)
        {
            return _cache!;
        }
    }

    private void EnsureScanned()
    {
        lock (_gate)
        {
            if (_cache is not null)
            {
                return;
            }

            var (topics, bodies) = Scan();
            _cache = topics;
            _bodies = bodies;
        }
    }

    public string GetContent(string id, string variant)
    {
        var file = ResolveVariantFile(RequireDir(id), variant);
        if (!File.Exists(file))
        {
            throw new TopicNotFoundException(id, variant);
        }

        return File.ReadAllText(file);
    }

    public IReadOnlyList<string> Variants(string id)
    {
        var dir = RequireDir(id);
        var list = new List<string> { DefaultVariant };
        if (File.Exists(Path.Combine(dir, "PRISM.md")))
        {
            list.Add("prism");
        }

        if (File.Exists(Path.Combine(dir, "ADVANCED.md")))
        {
            list.Add("advanced");
        }

        return list;
    }

    public IReadOnlyList<SearchHit> Search(string query, int maxResults)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var terms = query.ToLowerInvariant()
            .Split([' ', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0)
        {
            return [];
        }

        EnsureScanned();
        var hits = new List<SearchHit>();
        foreach (var topic in _cache!)
        {
            var body = _bodies![topic.Id];
            var id = topic.Id.ToLowerInvariant();
            var title = topic.Title.ToLowerInvariant();
            var summary = topic.Summary.ToLowerInvariant();

            var score = 0;
            foreach (var term in terms)
            {
                if (id.Contains(term)) score += 10;
                if (title.Contains(term)) score += 6;
                if (summary.Contains(term)) score += 4;
                if (body.Contains(term)) score += 1;
            }

            if (score > 0)
            {
                hits.Add(new SearchHit(topic.Id, topic.Title, Snippet(topic.Summary), score));
            }
        }

        return hits
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.Id, StringComparer.Ordinal)
            .Take(maxResults)
            .ToList();
    }

    private static string Snippet(string summary)
        => summary.Length <= SnippetMaxLength ? summary : summary[..SnippetMaxLength] + "…";

    private string RequireDir(string id)
    {
        var dir = Path.Combine(_knowledgeRoot, id);
        if (!Directory.Exists(dir) || !File.Exists(Path.Combine(dir, "TOPIC.md")))
        {
            throw new TopicNotFoundException(id);
        }

        return dir;
    }

    private static string ResolveVariantFile(string dir, string variant) => variant switch
    {
        "default"  => Path.Combine(dir, "TOPIC.md"),
        "prism"    => Path.Combine(dir, "PRISM.md"),
        "advanced" => Path.Combine(dir, "ADVANCED.md"),
        _ => throw new ArgumentException($"Unknown variant: '{variant}'.", nameof(variant)),
    };

    private (List<Topic> Topics, Dictionary<string, string> Bodies) Scan()
    {
        var topics = new List<Topic>();
        var bodies = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(_knowledgeRoot))
        {
            return (topics, bodies);
        }

        foreach (var dir in Directory.EnumerateDirectories(_knowledgeRoot).OrderBy(d => d, StringComparer.Ordinal))
        {
            var topicFile = Path.Combine(dir, "TOPIC.md");
            if (!File.Exists(topicFile))
            {
                continue;
            }

            var id = Path.GetFileName(dir);
            var text = File.ReadAllText(topicFile);
            var summary = TopicDocReader.ReadSummary(text) ?? string.Empty;
            var title = TopicDocReader.ReadTitle(text) ?? id;
            var companions = Directory.EnumerateFiles(dir, "*.md")
                .Select(Path.GetFileName)
                .Where(n => n is not null && !string.Equals(n, "TOPIC.md", StringComparison.Ordinal))
                .Select(n => n!)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

            topics.Add(new Topic(id, title, summary, companions));
            bodies[id] = text.ToLowerInvariant();
        }

        return (topics, bodies);
    }
}
