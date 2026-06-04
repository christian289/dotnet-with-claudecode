using Microsoft.Extensions.Logging;
using WpfDevPackMcp.Configuration;

namespace WpfDevPackMcp.Git;

public static partial class RepoRefresher
{
    private const string RepoUrl = "https://github.com/christian289/dotnet-with-claudecode";

    /// <summary>Pure decision: pull if forced, never pulled, or TTL elapsed.</summary>
    public static bool ShouldPull(DateTimeOffset? lastPull, DateTimeOffset now, TimeSpan ttl, bool force)
        => force || lastPull is null || (now - lastPull.Value) >= ttl;

    /// <summary>
    /// Best-effort: clone if missing, else pull when due. Never throws — logs
    /// failures and leaves the local clone usable. Returns the (possibly
    /// updated) RepoState to persist.
    /// </summary>
    public static RepoState EnsureFresh(
        ResolvedRepo repo, RepoState state, TimeSpan ttl, bool force,
        GitRunner git, ConfigStore store, ILogger logger)
    {
        var now = DateTimeOffset.UtcNow;
        if (!git.IsGitAvailable())
        {
            logger.LogWarning("git not found on PATH; serving local content as-is.");
            return state;
        }

        var isRepo = Directory.Exists(Path.Combine(repo.Path, ".git"));
        var managed = state.Managed;

        try
        {
            if (!isRepo)
            {
                Directory.CreateDirectory(repo.Path);
                logger.LogInformation("Cloning {Url} into {Path}...", RepoUrl, repo.Path);
                var clone = git.Run(repo.Path, "clone", "--branch", repo.Branch, RepoUrl, ".");
                if (!clone.Success)
                {
                    logger.LogWarning("git clone failed: {Err}", clone.StdErr);
                    return state;
                }

                managed = true;
                return Save(store, new RepoState(now, managed));
            }

            if (!ShouldPull(state.LastPullUtc, now, ttl, force))
            {
                return state;
            }

            if (managed)
            {
                git.Run(repo.Path, "fetch", "origin", repo.Branch);
                var reset = git.Run(repo.Path, "reset", "--hard", $"origin/{repo.Branch}");
                if (!reset.Success)
                {
                    logger.LogWarning("git reset failed: {Err}", reset.StdErr);
                }
            }
            else
            {
                var status = git.Run(repo.Path, "status", "--porcelain");
                if (status.Success && string.IsNullOrWhiteSpace(status.StdOut))
                {
                    var pull = git.Run(repo.Path, "pull", "--ff-only");
                    if (!pull.Success)
                    {
                        logger.LogWarning("git pull failed: {Err}", pull.StdErr);
                    }
                }
                else
                {
                    logger.LogInformation("Working tree dirty; skipping pull to protect local edits.");
                }
            }

            return Save(store, new RepoState(now, managed));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Repo refresh failed; serving local content as-is.");
            return state;
        }
    }

    private static RepoState Save(ConfigStore store, RepoState state)
    {
        store.SaveState(state);
        return state;
    }
}
