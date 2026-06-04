using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfDevPackMcp.Configuration;

/// <summary>
/// Reads the user-written repo config and persists server pull state under
/// a base directory (default: ~/.wpf-dev-pack-mcp). The env var
/// WPFDEVPACK_REPO_PATH overrides the config file's repoPath.
/// </summary>
public sealed class ConfigStore
{
    public const string RepoPathEnvVar = "WPFDEVPACK_REPO_PATH";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _baseDir;

    public ConfigStore(string? baseDir = null)
    {
        _baseDir = baseDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".wpf-dev-pack-mcp");
        Directory.CreateDirectory(_baseDir);
    }

    public string ConfigPath => Path.Combine(_baseDir, "config.json");
    public string StatePath => Path.Combine(_baseDir, "state.json");

    public ResolvedRepo? Resolve()
    {
        var config = LoadConfig();
        var branch = string.IsNullOrWhiteSpace(config?.Branch) ? "main" : config!.Branch;

        var fromEnv = Environment.GetEnvironmentVariable(RepoPathEnvVar);
        var path = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : config?.RepoPath;

        return string.IsNullOrWhiteSpace(path) ? null : new ResolvedRepo(path!, branch);
    }

    public RepoConfig? LoadConfig()
        => File.Exists(ConfigPath)
            ? JsonSerializer.Deserialize<RepoConfig>(File.ReadAllText(ConfigPath), JsonOptions)
            : null;

    public RepoState LoadState()
        => File.Exists(StatePath)
            ? JsonSerializer.Deserialize<RepoState>(File.ReadAllText(StatePath), JsonOptions)
              ?? new RepoState(null, false)
            : new RepoState(null, false);

    public void SaveState(RepoState state)
        => File.WriteAllText(StatePath, JsonSerializer.Serialize(state, JsonOptions));
}
