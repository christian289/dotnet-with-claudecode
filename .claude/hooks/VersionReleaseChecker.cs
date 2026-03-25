#!/usr/bin/env dotnet

// Version Release Checker Hook (PostToolUse - Bash)
// Runs after Bash tool execution. Checks if the command was 'git push'
// and verifies: 1) GitHub release exists for current version
//               2) README skill count matches actual
//               3) GitHub profile skill count matches
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

// Check 2: README skill count matches actual?
var skillsDir = Path.Combine(pluginRoot, "skills");
int actualSkillCount = 0;
if (Directory.Exists(skillsDir))
{
    actualSkillCount = Directory.GetDirectories(skillsDir)
        .Where(d => !Path.GetFileName(d).StartsWith("."))
        .Count(d => File.Exists(Path.Combine(d, "SKILL.md")));
}

Console.WriteLine($"  Checking README skill count (actual: {actualSkillCount})...");
var readmePath = Path.Combine(pluginRoot, "README.md");
if (File.Exists(readmePath) && actualSkillCount > 0)
{
    var readmeContent = File.ReadAllText(readmePath);
    var match = Regex.Match(readmeContent, @"\*\*(\d+)\s+Skills\*\*");
    if (match.Success && int.TryParse(match.Groups[1].Value, out int readmeCount))
    {
        if (readmeCount != actualSkillCount)
        {
            Console.WriteLine($"  ❌ README says {readmeCount}, actual is {actualSkillCount}");
            issues.Add($"[README MISMATCH] README.md says {readmeCount} Skills but actual count is {actualSkillCount}. Update both README.md and README.ko.md.");
        }
        else
        {
            Console.WriteLine($"  ✅ README skill count matches ({readmeCount})");
        }
    }
}

// Check 3: GitHub profile skill count matches?
if (actualSkillCount > 0)
{
    Console.WriteLine("  Checking GitHub profile skill count...");
    var profileResult = RunCommand("gh", "api repos/christian289/christian289/contents/README.md --jq .content", projectDir);
    if (profileResult.exitCode == 0 && !string.IsNullOrWhiteSpace(profileResult.output))
    {
        try
        {
            var profileContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(profileResult.output.Trim()));
            var profileMatch = Regex.Match(profileContent, @"\*\*(\d+)\s+Skills\*\*");
            if (profileMatch.Success && int.TryParse(profileMatch.Groups[1].Value, out int profileCount))
            {
                if (profileCount != actualSkillCount)
                {
                    Console.WriteLine($"  ❌ GitHub profile says {profileCount}, actual is {actualSkillCount}");
                    issues.Add($"[PROFILE MISMATCH] GitHub profile says {profileCount} Skills but actual count is {actualSkillCount}. Update christian289/christian289 README.md via gh api.");
                }
                else
                {
                    Console.WriteLine($"  ✅ GitHub profile skill count matches ({profileCount})");
                }
            }
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
