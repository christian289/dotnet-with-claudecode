#!/usr/bin/env dotnet

// Version Release Checker Hook (PostToolUse - Bash)
// Runs after Bash tool execution. Checks if the command was 'git push'
// and verifies: 1) GitHub release exists for current version
//               2) README counts match actual (Skills, Agents, MCP Servers)
//               3) GitHub profile counts match actual (Skills, Agents, MCP Servers)
// Input: stdin JSON with "tool_name" and "tool_input" fields

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
    return;

// Parse tool input to check if this was a git push command
string? command = null;
try
{
    using var doc = JsonDocument.Parse(input);
    if (doc.RootElement.TryGetProperty("tool_input", out var ti))
    {
        if (ti.TryGetProperty("command", out var cmd))
            command = cmd.GetString();
    }
}
catch
{
    return;
}

if (string.IsNullOrWhiteSpace(command))
    return;

// Only run for git push commands
if (!command.Contains("git push", StringComparison.OrdinalIgnoreCase))
    return;

var projectDir = Environment.GetEnvironmentVariable("CLAUDE_PROJECT_DIR")
    ?? Environment.CurrentDirectory;
var pluginRoot = Path.Combine(projectDir, "wpf-dev-pack");

// Only run when wpf-dev-pack exists in this repository
var pluginJsonPath = Path.Combine(pluginRoot, ".claude-plugin", "plugin.json");
if (!File.Exists(pluginJsonPath))
    return;

// Read current version from plugin.json
string? currentVersion = null;
try
{
    var pluginJson = File.ReadAllText(pluginJsonPath);
    using var jsonDoc = JsonDocument.Parse(pluginJson);
    if (jsonDoc.RootElement.TryGetProperty("version", out var versionProp))
        currentVersion = versionProp.GetString();
}
catch
{
    return;
}

if (string.IsNullOrWhiteSpace(currentVersion))
    return;

Console.WriteLine($"[WPF Dev Pack] Version Release Checker running for v{currentVersion}...");

var issues = new List<string>();

// Check 1: GitHub release exists for current version?
var tagName = $"wpf-dev-pack-v{currentVersion}";
Console.WriteLine($"  Checking GitHub release for tag '{tagName}'...");
var releaseExists = RunCommand("gh", $"release view {tagName} --repo christian289/dotnet-with-claudecode --json tagName", projectDir);
if (releaseExists.exitCode != 0)
{
    Console.WriteLine($"  ❌ No release found for '{tagName}'");
    issues.Add($"""[RELEASE MISSING] No GitHub release found for tag '{tagName}'. Create a release with: gh release create {tagName} --title "wpf-dev-pack v{currentVersion}" --notes "..." """);
}
else
{
    Console.WriteLine($"  ✅ Release exists for '{tagName}'");
}

// Count actual Skills, Agents, MCP Servers
var skillsDir = Path.Combine(pluginRoot, "skills");
int actualSkillCount = 0;
if (Directory.Exists(skillsDir))
{
    actualSkillCount = Directory.GetDirectories(skillsDir)
        .Where(d => !Path.GetFileName(d).StartsWith("."))
        .Count(d => File.Exists(Path.Combine(d, "SKILL.md")));
}

var agentsDir = Path.Combine(pluginRoot, "agents");
int actualAgentCount = 0;
if (Directory.Exists(agentsDir))
{
    actualAgentCount = Directory.GetFiles(agentsDir, "*.md")
        .Count(f => !Path.GetFileName(f).StartsWith("README", StringComparison.OrdinalIgnoreCase));
}

var mcpJsonPath = Path.Combine(pluginRoot, ".mcp.json");
int actualMcpCount = 0;
if (File.Exists(mcpJsonPath))
{
    try
    {
        using var mcpDoc = JsonDocument.Parse(File.ReadAllText(mcpJsonPath));
        if (mcpDoc.RootElement.TryGetProperty("mcpServers", out var servers))
            actualMcpCount = servers.EnumerateObject().Count();
    }
    catch { }
}

Console.WriteLine($"  Actual counts: Skills={actualSkillCount}, Agents={actualAgentCount}, MCP Servers={actualMcpCount}");

// Check 2: README counts match actual?
var readmePath = Path.Combine(pluginRoot, "README.md");
if (File.Exists(readmePath))
{
    var readmeContent = File.ReadAllText(readmePath);
    CheckReadmeCount(readmeContent, @"\*\*(\d+)\s+Skills\*\*", actualSkillCount, "Skills", "README.md", issues);
    CheckReadmeCount(readmeContent, @"\*\*(\d+)\s+Specialized\s+Agents\*\*", actualAgentCount, "Agents", "README.md", issues);
    CheckReadmeCount(readmeContent, @"\*\*(\d+)\s+MCP\s+Servers?\*\*", actualMcpCount, "MCP Servers", "README.md", issues);
}

// Check 3: GitHub profile counts match actual?
Console.WriteLine("  Checking GitHub profile counts...");
var profileResult = RunCommand("gh", "api repos/christian289/christian289/contents/README.md --jq .content", projectDir);
if (profileResult.exitCode == 0 && !string.IsNullOrWhiteSpace(profileResult.output))
{
    try
    {
        var profileContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(profileResult.output.Trim()));
        CheckReadmeCount(profileContent, @"\*\*(\d+)\s+Skills\*\*", actualSkillCount, "Skills", "GitHub Profile", issues);
        CheckReadmeCount(profileContent, @"\*\*(\d+)\s+.*?Agents?\*\*", actualAgentCount, "Agents", "GitHub Profile", issues);
        CheckReadmeCount(profileContent, @"\*\*(\d+)\s+MCP\s+Servers?\*\*", actualMcpCount, "MCP Servers", "GitHub Profile", issues);
    }
    catch
    {
        Console.WriteLine("  ⚠️ Failed to decode GitHub profile README, skipping.");
    }
}
else
{
    Console.WriteLine("  ⚠️ Could not fetch GitHub profile README, skipping.");
}

if (issues.Count > 0)
{
    Console.WriteLine($"[WPF Dev Pack] ❌ {issues.Count} issue(s) found after git push:");
    foreach (var issue in issues)
        Console.WriteLine($"  -> {issue}");
}
else
{
    Console.WriteLine("[WPF Dev Pack] ✅ All version release checks passed.");
}

// --- Helper methods ---

static void CheckReadmeCount(string content, string pattern, int actual, string label, string source, List<string> issues)
{
    var match = Regex.Match(content, pattern);
    if (match.Success && int.TryParse(match.Groups[1].Value, out int found))
    {
        if (found != actual)
        {
            Console.WriteLine($"  ❌ {source} {label}: says {found}, actual is {actual}");
            var target = source == "GitHub Profile"
                ? "Update christian289/christian289 README.md via gh api."
                : "Update both README.md and README.ko.md.";
            issues.Add($"[{source.ToUpper().Replace(" ", "_")} MISMATCH] {source} says {found} {label} but actual is {actual}. {target}");
        }
        else
        {
            Console.WriteLine($"  ✅ {source} {label} matches ({found})");
        }
    }
}

static (int exitCode, string output) RunCommand(string cmd, string args, string workingDir)
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process is null)
            return (-1, "");

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(10000);
        return (process.ExitCode, output);
    }
    catch
    {
        return (-1, "");
    }
}
