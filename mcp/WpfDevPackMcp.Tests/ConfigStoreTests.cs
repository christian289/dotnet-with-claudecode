using WpfDevPackMcp.Configuration;
using Xunit;

namespace WpfDevPackMcp.Tests;

[Collection("env-serial")]
public sealed class ConfigStoreTests : IDisposable
{
    private readonly string _home;

    public ConfigStoreTests()
    {
        _home = Path.Combine(Path.GetTempPath(), "wdpmcp-home-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_home);
        Environment.SetEnvironmentVariable("WPFDEVPACK_REPO_PATH", null);
    }

    [Fact]
    public void Resolve_EnvVarWins_OverConfigFile()
    {
        File.WriteAllText(Path.Combine(_home, "config.json"),
            """{ "repoPath": "C:/from-file", "branch": "main" }""");
        Environment.SetEnvironmentVariable("WPFDEVPACK_REPO_PATH", "C:/from-env");

        var store = new ConfigStore(_home);
        var resolved = store.Resolve();

        Assert.NotNull(resolved);
        Assert.Equal("C:/from-env", resolved!.Path);
    }

    [Fact]
    public void Resolve_FromConfigFile_WhenNoEnv()
    {
        File.WriteAllText(Path.Combine(_home, "config.json"),
            """{ "repoPath": "C:/from-file", "branch": "dev" }""");

        var resolved = new ConfigStore(_home).Resolve();

        Assert.Equal("C:/from-file", resolved!.Path);
        Assert.Equal("dev", resolved.Branch);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenUnconfigured()
        => Assert.Null(new ConfigStore(_home).Resolve());

    [Fact]
    public void State_RoundTrips()
    {
        var store = new ConfigStore(_home);
        var stamp = DateTimeOffset.UtcNow;
        store.SaveState(new RepoState(stamp, Managed: true));

        var loaded = store.LoadState();
        Assert.True(loaded.Managed);
        Assert.Equal(stamp.ToUnixTimeSeconds(), loaded.LastPullUtc!.Value.ToUnixTimeSeconds());
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("WPFDEVPACK_REPO_PATH", null);
        if (Directory.Exists(_home))
        {
            Directory.Delete(_home, recursive: true);
        }
    }
}
