#!/usr/bin/env dotnet

// Sets the 'branch' field in ~/.wpf-dev-pack-mcp/config.json (the branch the
// WpfDevPackMcp server tracks), preserving the existing repoPath.
// Note: file-based dotnet apps run with reflection-based JsonSerializer
// disabled, so this reads with JsonDocument (DOM) and writes JSON by hand.
// Usage: dotnet SetWpfDevPackBranch.cs <branch>

using System.Text.Json;

var branch = args.Length > 0 ? args[0]?.Trim().Trim('"') : null;
if (string.IsNullOrWhiteSpace(branch))
{
    Console.Error.WriteLine("ERROR: branch argument is required.");
    Console.Error.WriteLine("Usage: /wpf-dev-pack:set-repo-branch <branch>");
    return 1;
}

var baseDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wpf-dev-pack-mcp");
Directory.CreateDirectory(baseDir);
var configPath = Path.Combine(baseDir, "config.json");

string? repoPath = null;
if (File.Exists(configPath))
{
    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            if (string.Equals(p.Name, "repoPath", StringComparison.OrdinalIgnoreCase)
                && p.Value.ValueKind == JsonValueKind.String)
            {
                var v = p.Value.GetString();
                if (!string.IsNullOrWhiteSpace(v)) repoPath = v;
            }
        }
    }
    catch { /* malformed config — it will be rewritten cleanly below */ }
}

var branchJson = branch.Replace("\"", "\\\"");
var json = repoPath is null
    ? $"{{\n  \"branch\": \"{branchJson}\"\n}}"
    : $"{{\n  \"repoPath\": \"{repoPath.Replace('\\', '/').Replace("\"", "\\\"")}\",\n  \"branch\": \"{branchJson}\"\n}}";
File.WriteAllText(configPath, json);

Console.WriteLine("WpfDevPackMcp branch set:");
Console.WriteLine($"  branch   = {branch}");
Console.WriteLine($"  repoPath = {repoPath ?? "(unset)"}");
Console.WriteLine($"  written  = {configPath}");
if (repoPath is null)
    Console.WriteLine("  ! repoPath is not set yet — run /wpf-dev-pack:set-repo-path <path> to make the MCP usable.");
Console.WriteLine("  Takes effect on the next WpfDevPackMcp refresh.");

return 0;
