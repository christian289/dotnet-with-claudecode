namespace WpfDevPackMcp.Knowledge;

/// <summary>One knowledge topic = one directory under the repo-root knowledge/.</summary>
public sealed record Topic(
    string Id,
    string Title,
    string Summary,
    IReadOnlyList<string> Companions);
