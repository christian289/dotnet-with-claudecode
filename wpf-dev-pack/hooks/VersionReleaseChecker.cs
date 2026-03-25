#!/usr/bin/env dotnet

// Version Release Checker Hook (Stop event)
// Checks if wpf-dev-pack version was bumped but release/sync is missing.
// Blocks stopping if: 1) No GitHub release for current version
//                     2) README skill count mismatch
//                     3) GitHub profile skill count mismatch
// Input: stdin JSON with "reason" field

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

var pluginRoot = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT") ?? ".";
var projectDir = Environment.GetEnvironmentVariable("CLAUDE_PROJECT_DIR") ?? ".";

// Only run when working inside the wpf-dev-pack repository
// wpf-dev-pack 저장소 내에서 작업할 때만 실행
var pluginJsonPath = Path.Combine(pluginRoot, ".claude-plugin", "plugin.json");
if (!File.Exists(pluginJsonPath))
{
    Approve();
    return;
}

// Read current version from plugin.json
// plugin.json에서 현재 버전 읽기
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
    Approve();
    return;
}

if (string.IsNullOrWhiteSpace(currentVersion))
{
    Approve();
    return;
}

var issues = new List<string>();

// Check 1: GitHub release exists for current version?
// 확인 1: 현재 버전의 GitHub Release가 존재하는가?
var tagName = $"wpf-dev-pack-v{currentVersion}";
var releaseExists = RunCommand("gh", $"release view {tagName} --repo christian289/dotnet-with-claudecode --json tagName", projectDir);
if (releaseExists.exitCode != 0)
{
    issues.Add($"[RELEASE MISSING] No GitHub release found for tag '{tagName}'. " +
               $"Create a release with: gh release create {tagName} --title \"wpf-dev-pack v{currentVersion}\" --notes \"...\"");
}

// Check 2: README skill count matches actual?
// 확인 2: README 스킬 수가 실제와 일치하는가?
var skillsDir = Path.Combine(pluginRoot, "skills");
int actualSkillCount = 0;
if (Directory.Exists(skillsDir))
{
    actualSkillCount = Directory.GetDirectories(skillsDir)
        .Where(d => !Path.GetFileName(d).StartsWith("."))
        .Count(d => File.Exists(Path.Combine(d, "SKILL.md")));
}

var readmePath = Path.Combine(pluginRoot, "README.md");
if (File.Exists(readmePath) && actualSkillCount > 0)
{
    var readmeContent = File.ReadAllText(readmePath);
    var match = Regex.Match(readmeContent, @"\*\*(\d+)\s+Skills\*\*");
    if (match.Success && int.TryParse(match.Groups[1].Value, out int readmeCount))
    {
        if (readmeCount != actualSkillCount)
        {
            issues.Add($"[README MISMATCH] README.md says {readmeCount} Skills but actual count is {actualSkillCount}. " +
                       $"Update both README.md and README.ko.md.");
        }
    }
}

// Check 3: GitHub profile skill count matches?
// 확인 3: GitHub 프로필 스킬 수가 일치하는가?
if (actualSkillCount > 0)
{
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
                    issues.Add($"[PROFILE MISMATCH] GitHub profile says {profileCount} Skills but actual count is {actualSkillCount}. " +
                               $"Update christian289/christian289 README.md via gh api.");
                }
            }
        }
        catch
        {
            // base64 decode failure — skip profile check
        }
    }
}

if (issues.Count > 0)
{
    Block(issues);
}
else
{
    Approve();
}

// --- Helper methods ---

static void Approve()
{
    Console.WriteLine("""{"decision":"approve"}""");
}

static void Block(List<string> issues)
{
    var reason = "========================================\\n" +
                 "[WPF Dev Pack] Version Release Check Failed\\n" +
                 string.Join("\\n", issues.Select(i => $"  -> {EscapeJson(i)}")) + "\\n" +
                 "========================================";

    Console.WriteLine($$$"""{"decision":"block","reason":"{{{reason}}}"}""");
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
