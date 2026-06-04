using Microsoft.Extensions.Logging;
using WpfDevPackMcp.Configuration;
using WpfDevPackMcp.Git;

namespace WpfDevPackMcp.Knowledge;

public sealed class RepoNotConfiguredException()
    : Exception(
        "WPF knowledge repo path is not configured. This requires a one-time USER setup — " +
        "do NOT auto-detect the repo or run set-repo-path yourself; ask the user to run " +
        "/wpf-dev-pack:set-repo-path <path>.");

/// <summary>
/// Facade the MCP tools depend on: resolves the repo, refreshes (TTL/forced),
/// and exposes catalog operations. Throws RepoNotConfiguredException when no
/// path is set (the RepoPathGuard hook normally prevents reaching here).
/// </summary>
public sealed class KnowledgeService(ConfigStore store, GitRunner git, ILogger<KnowledgeService> logger)
{
    private static readonly TimeSpan Ttl = ReadTtl();
    private readonly Lock _gate = new();
    private TopicCatalog? _catalog;
    private string? _catalogRoot;

    public void EnsureReady(bool force = false)
    {
        var repo = store.Resolve() ?? throw new RepoNotConfiguredException();
        var state = RepoRefresher.EnsureFresh(repo, store.LoadState(), Ttl, force, git, store, logger);
        _ = state;

        lock (_gate)
        {
            if (_catalog is null || !string.Equals(_catalogRoot, repo.Path, StringComparison.Ordinal))
            {
                _catalog = new TopicCatalog(repo.Path);
                _catalogRoot = repo.Path;
            }
            else if (force)
            {
                _catalog.Invalidate();
            }
        }
    }

    public TopicCatalog Catalog
    {
        get
        {
            lock (_gate)
            {
                return _catalog ?? throw new RepoNotConfiguredException();
            }
        }
    }

    private static TimeSpan ReadTtl()
    {
        var raw = Environment.GetEnvironmentVariable("WPFDEVPACK_PULL_TTL_MINUTES");
        return int.TryParse(raw, out var m) && m >= 0 ? TimeSpan.FromMinutes(m) : TimeSpan.FromMinutes(60);
    }
}
