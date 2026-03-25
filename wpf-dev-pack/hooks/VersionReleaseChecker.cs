#!/usr/bin/env dotnet

// Version Release Checker Hook (Stop event)
// Checks if wpf-dev-pack version was bumped but release/sync is missing.
// Blocks stopping if: 1) No GitHub release for current version
//                     2) README skill count mismatch
//                     3) GitHub profile skill count mismatch
// Uses stderr for visible diagnostic output, stdout for JSON decision only.
// Input: stdin JSON with "reason" field

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

var pluginRoot = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT") ?? ".";
var projectDir = Environment.GetEnvironmentVariable("CLAUDE_PROJECT_DIR") ?? ".";

// Only run when working inside the wpf-dev-pack repository
var pluginJsonPath = Path.Combine(pluginRoot, ".claude-plugin", "plugin.json");
if (!File.Exists(pluginJsonPath))
{
    Log("plugin.json not found, skipping version check.");
    Approve();
    return;
}

// Read current version from plugin.json
string? currentVersion = null;
try
{
    var pluginJson = File.ReadAllText(pluginJsonPath);
    using var doc = JsonDocument.Parse(pluginJson);
    if (doc.RootElement.TryGetProperty("version", out var versionProp))
        currentVersion = versionProp.GetString();
}
catch
{
    Log("Failed to parse plugin.json, skipping version check.");
    Approve();
    return;
}

if (string.IsNullOrWhiteSpace(currentVersion))
{
    Log("No version found in plugin.json, skipping version check.");
    Approve();
    return;
}

Log($"[WPF Dev Pack] Version Release Checker running for v{currentVersion}...");

var issues = new List<string>();

// Check 1: GitHub release exists for current version?
var tagName = $"wpf-dev-pack-v{currentVersion}";
Log($"  Checking GitHub release for tag '{tagName}'...");
var releaseExists = RunCommand("gh", $"release view {tagName} --repo christian289/dotnet-with-claudecode --json tagName", projectDir);
if (releaseExists.exitCode != 0)
{
    Log($"  ❌ No release found for '{tagName}'");
    issues.Add($"""[RELEASE MISSING] No GitHub release found for tag '{tagName}'. Create a release with: gh release create {tagName} --title "wpf-dev-pack v{currentVersion}" --notes "..." """);
}
else
{
    Log($"  ✅ Release exists for '{tagName}'");
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

Log($"  Checking README skill count (actual: {actualSkillCount})...");
var readmePath = Path.Combine(pluginRoot, "README.md");
if (File.Exists(readmePath) && actualSkillCount > 0)
{
    var readmeContent = File.ReadAllText(readmePath);
    var match = Regex.Match(readmeContent, @"\*\*(\d+)\s+Skills\*\*");
    if (match.Success && int.TryParse(match.Groups[1].Value, out int readmeCount))
    {
        if (readmeCount != actualSkillCount)
        {
            Log($"  ❌ README says {readmeCount}, actual is {actualSkillCount}");
            issues.Add($"[README MISMATCH] README.md says {readmeCount} Skills but actual count is {actualSkillCount}. Update both README.md and README.ko.md.");
        }
        else
        {
            Log($"  ✅ README skill count matches ({readmeCount})");
        }
    }
}

// Check 3: GitHub profile skill count matches?
if (actualSkillCount > 0)
{
    Log("  Checking GitHub profile skill count...");
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
                    Log($"  ❌ GitHub profile says {profileCount}, actual is {actualSkillCount}");
                    issues.Add($"[PROFILE MISMATCH] GitHub profile says {profileCount} Skills but actual count is {actualSkillCount}. Update christian289/christian289 README.md via gh api.");
                }
                else
                {
                    Log($"  ✅ GitHub profile skill count matches ({profileCount})");
                }
            }
        }
        catch
        {
            Log("  ⚠️ Failed to decode GitHub profile README, skipping.");
        }
    }
    else
    {
        Log("  ⚠️ Could not fetch GitHub profile README, skipping.");
    }
}

if (issues.Count > 0)
{
    Log($"[WPF Dev Pack] ❌ {issues.Count} issue(s) found. Blocking stop.");
    Block(issues);
}
else
{
    Log("[WPF Dev Pack] ✅ All version release checks passed.");
    Approve();
}

// --- Helper methods ---

static void Log(string message)
{
    Console.Error.WriteLine(message);
}

static void Approve()
{
    Console.WriteLine("""{"decision":"approve"}""");
}

static void Block(List<string> issues)
{
    var issueLines = string.Join("\\n", issues.Select(i => $"  -> {EscapeJson(i)}"));
    var reason = $"""========================================\n[WPF Dev Pack] Version Release Check Failed\n{issueLines}\n========================================""";

    Console.WriteLine($$"""{"decision":"block","reason":"{{reason}}"}""");
}

static string EscapeJson(string s)
{
    return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
}

static (int exitCode, string output) RunCommand(string command, string args, string workingDir)
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
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
