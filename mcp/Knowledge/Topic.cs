namespace WpfDevPackMcp.Knowledge;

/// <summary>One knowledge topic = one directory under wpf-dev-pack/knowledge/.</summary>
public sealed record Topic(
    string Id,
    string Title,
    string Summary,
    IReadOnlyList<string> Companions);
