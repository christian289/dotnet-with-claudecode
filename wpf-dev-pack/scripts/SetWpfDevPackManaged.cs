#!/usr/bin/env dotnet

// Sets the 'Managed' flag in ~/.wpf-dev-pack-mcp/state.json (preserving
// LastPullUtc). Managed controls how WpfDevPackMcp refreshes its clone:
//   true  -> git fetch + git reset --hard origin/<branch> (DESTRUCTIVE; only
//            safe for a dedicated clone the server owns)
//   false -> git pull --ff-only when clean, skip when dirty (non-destructive;
//            required when repoPath is your active working repo)
// Note: file-based dotnet apps run with reflection-based JsonSerializer
// disabled, so this reads with JsonDocument (DOM) and writes JSON by hand.
// Usage: dotnet SetWpfDevPackManaged.cs <true|false>

using System.Text.Json;

var raw = args.Length > 0 ? args[0]?.Trim().Trim('"').ToLowerInvariant() : null;
bool? managed = raw switch
{
    "true" or "1" or "on" or "yes" => true,
    "false" or "0" or "off" or "no" => false,
    _ => null,
};

if (managed is null)
{
    Console.Error.WriteLine("ERROR: argument must be true or false.");
    Console.Error.WriteLine("Usage: /wpf-dev-pack:set-repo-managed <true|false>");
    return 1;
}

var baseDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wpf-dev-pack-mcp");
Directory.CreateDirectory(baseDir);
var statePath = Path.Combine(baseDir, "state.json");

string? lastPullRaw = null;
if (File.Exists(statePath))
{
    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(statePath));
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            if (string.Equals(p.Name, "LastPullUtc", StringComparison.OrdinalIgnoreCase)
                && p.Value.ValueKind == JsonValueKind.String)
            {
                lastPullRaw = p.Value.GetString();
            }
        }
    }
    catch { /* malformed state — it will be rewritten cleanly below */ }
}

var lastPullJson = lastPullRaw is null ? "null" : $"\"{lastPullRaw.Replace("\"", "\\\"")}\"";
var managedJson = managed.Value ? "true" : "false";
var json = $"{{\n  \"LastPullUtc\": {lastPullJson},\n  \"Managed\": {managedJson}\n}}";
File.WriteAllText(statePath, json);

Console.WriteLine("WpfDevPackMcp state updated:");
Console.WriteLine($"  Managed     = {managedJson}");
Console.WriteLine($"  LastPullUtc = {lastPullRaw ?? "(null)"}");
Console.WriteLine($"  written     = {statePath}");
Console.WriteLine();
if (managed.Value)
    Console.WriteLine("  ! Managed=true: on refresh the server runs `git fetch` + `git reset --hard origin/<branch>` on the configured repoPath. Use ONLY for a dedicated clone the server owns — NOT your active working repo (it discards uncommitted changes and resets the branch).");
else
    Console.WriteLine("  Managed=false: refresh is non-destructive (ff-only pull when clean, skip when dirty). Safe when repoPath is your working repo.");

return 0;
