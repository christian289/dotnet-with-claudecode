#!/usr/bin/env dotnet

// Validates a local clone path of christian289/dotnet-with-claudecode and
// records it to ~/.wpf-dev-pack-mcp/config.json so WpfDevPackMcp can read it.
// Usage: dotnet SetWpfDevPackRepoPath.cs <repo-path> [branch]

var path = args.Length > 0 ? args[0]?.Trim().Trim('"') : null;
var branch = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1].Trim() : "main";

if (string.IsNullOrWhiteSpace(path))
{
    Console.Error.WriteLine("ERROR: repo path argument is required.");
    Console.Error.WriteLine("Usage: /wpf-dev-pack:set-repo-path <path-to-local-clone> [branch]");
    return 1;
}

var full = Path.GetFullPath(path);
var warnings = new List<string>();

if (!Directory.Exists(full))
{
    warnings.Add($"Path does not exist yet: {full}. WpfDevPackMcp will clone the repo there on first use.");
}
else
{
    if (!Directory.Exists(Path.Combine(full, ".git")))
    {
        warnings.Add($"No .git found at {full}. If it is empty, the server will clone into it; otherwise point at a real clone.");
    }

    if (!Directory.Exists(Path.Combine(full, "wpf-dev-pack")))
    {
        warnings.Add("No 'wpf-dev-pack' folder found at the path — confirm this is the dotnet-with-claudecode repo root.");
    }
}

var baseDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wpf-dev-pack-mcp");
Directory.CreateDirectory(baseDir);

var configPath = Path.Combine(baseDir, "config.json");
var repoPathJson = full.Replace('\\', '/').Replace("\"", "\\\"");
var branchJson = branch.Replace("\"", "\\\"");
var json = $"{{\n  \"repoPath\": \"{repoPathJson}\",\n  \"branch\": \"{branchJson}\"\n}}";
File.WriteAllText(configPath, json);

Console.WriteLine($"WpfDevPackMcp repo path set:");
Console.WriteLine($"  repoPath = {full}");
Console.WriteLine($"  branch   = {branch}");
Console.WriteLine($"  written  = {configPath}");
foreach (var w in warnings)
{
    Console.WriteLine($"  ! {w}");
}

return 0;
