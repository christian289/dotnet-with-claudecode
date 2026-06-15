#!/usr/bin/env dotnet

// Prints the absolute paths and current values of the WpfDevPackMcp
// configuration (config.json) and server pull state (state.json), plus the
// effective repo-path resolution (the WPFDEVPACK_REPO_PATH env var overrides
// config.json's repoPath) and the pull-TTL override.
// Usage: dotnet ShowWpfDevPackConfig.cs

var baseDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wpf-dev-pack-mcp");
var configPath = Path.Combine(baseDir, "config.json");
var statePath = Path.Combine(baseDir, "state.json");

Console.WriteLine("WpfDevPackMcp configuration");
Console.WriteLine($"  base dir : {baseDir}");
Console.WriteLine();

Console.WriteLine($"config.json : {configPath}");
Console.WriteLine(ReadOrNote(configPath, "(not found — run /wpf-dev-pack:set-repo-path <path> to create it)"));
Console.WriteLine();

Console.WriteLine($"state.json  : {statePath}");
Console.WriteLine(ReadOrNote(statePath, "(not found — the server creates it on first refresh)"));
Console.WriteLine();

var env = Environment.GetEnvironmentVariable("WPFDEVPACK_REPO_PATH");
var ttl = Environment.GetEnvironmentVariable("WPFDEVPACK_PULL_TTL_MINUTES");
Console.WriteLine("environment overrides");
Console.WriteLine($"  WPFDEVPACK_REPO_PATH        = {(string.IsNullOrWhiteSpace(env) ? "(unset)" : env)}");
Console.WriteLine($"  WPFDEVPACK_PULL_TTL_MINUTES = {(string.IsNullOrWhiteSpace(ttl) ? "(unset; default 60)" : ttl)}");
Console.WriteLine();
Console.WriteLine("Note: when WPFDEVPACK_REPO_PATH is set it OVERRIDES config.json's repoPath.");
Console.WriteLine("Change values with: /wpf-dev-pack:set-repo-path, /wpf-dev-pack:set-repo-branch, /wpf-dev-pack:set-repo-managed");

return 0;

static string ReadOrNote(string path, string missingNote)
{
    if (!File.Exists(path))
        return "  " + missingNote;
    try
    {
        var text = File.ReadAllText(path).Replace("\r\n", "\n").TrimEnd();
        return "  " + text.Replace("\n", "\n  ");
    }
    catch (Exception ex)
    {
        return $"  (could not read: {ex.Message})";
    }
}
