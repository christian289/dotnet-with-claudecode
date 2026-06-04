namespace WpfDevPackMcp.Configuration;

/// <summary>User-written config (~/.wpf-dev-pack-mcp/config.json).</summary>
public sealed record RepoConfig(string? RepoPath, string Branch = "main");

/// <summary>Server-managed pull state (~/.wpf-dev-pack-mcp/state.json).</summary>
public sealed record RepoState(DateTimeOffset? LastPullUtc, bool Managed);

/// <summary>Fully resolved target the server will read from.</summary>
public sealed record ResolvedRepo(string Path, string Branch);
