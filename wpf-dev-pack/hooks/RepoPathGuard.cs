#!/usr/bin/env dotnet

// PreToolUse guard: blocks WpfDevPackMcp tool calls when the knowledge repo
// path is not configured, with a message pointing to /wpf-dev-pack:set-repo-path.
// Input: stdin JSON with "tool_name". Output: PreToolUse permission decision.

using System.Text.Json;

var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
{
    return;
}

string? toolName = null;
try
{
    using var doc = JsonDocument.Parse(input);
    if (doc.RootElement.TryGetProperty("tool_name", out var t))
    {
        toolName = t.GetString();
    }
}
catch
{
    return;
}

// Only guard our own MCP server's tools.
if (string.IsNullOrEmpty(toolName) ||
    toolName.IndexOf("WpfDevPackMcp", StringComparison.OrdinalIgnoreCase) < 0)
{
    return;
}

if (IsConfigured())
{
    return; // allow
}

var reason =
    "WpfDevPackMcp is not configured: the knowledge repo path is unset. " +
    "Run /wpf-dev-pack:set-repo-path <path-to-local-clone-of-christian289/dotnet-with-claudecode> first.";

var reasonJson = reason.Replace("\\", "\\\\").Replace("\"", "\\\"");
var outputJson =
    "{\"hookSpecificOutput\":{" +
        "\"hookEventName\":\"PreToolUse\"," +
        "\"permissionDecision\":\"deny\"," +
        $"\"permissionDecisionReason\":\"{reasonJson}\"" +
    "}}";
Console.WriteLine(outputJson);

static bool IsConfigured()
{
    var fromEnv = Environment.GetEnvironmentVariable("WPFDEVPACK_REPO_PATH");
    if (!string.IsNullOrWhiteSpace(fromEnv))
    {
        return true;
    }

    var configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".wpf-dev-pack-mcp", "config.json");
    if (!File.Exists(configPath))
    {
        return false;
    }

    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
        return doc.RootElement.TryGetProperty("repoPath", out var p)
            && !string.IsNullOrWhiteSpace(p.GetString());
    }
    catch
    {
        return false;
    }
}
