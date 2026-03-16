#!/usr/bin/env dotnet

// MCP Dependency Checker Hook
// Checks for required MCP servers at runtime and warns if missing.
// Runs once per session using a temporary file cache.
// Input: stdin JSON (ignored - runs independently)

var tempDir = Path.GetTempPath();
var today = DateTime.Now.ToString("yyyy-MM-dd");
var cacheFile = Path.Combine(tempDir, $"wpf-dev-pack-mcp-check-{today}.txt");

// Check if already ran today
// 오늘 이미 실행했는지 확인
if (File.Exists(cacheFile))
    return;

// Mark as checked for this session
// 이 세션에서 체크 완료로 표시
File.WriteAllText(cacheFile, DateTime.Now.ToString("o"));

// Required MCP servers for wpf-dev-pack agents
// wpf-dev-pack 에이전트에 필요한 MCP 서버
var requiredMcps = new[] { "context7", "serena", "sequential-thinking", "handmirror" };
var missingMcps = new List<string>();

// Search for MCP configuration in common locations
// 일반적인 위치에서 MCP 설정 검색
var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var possiblePaths = new[]
{
    Path.Combine(homeDir, ".claude", ".mcp.json"),
    Path.Combine(homeDir, ".claude", "mcp.json"),
    Path.Combine(homeDir, ".mcp.json"),
    Path.Combine(Environment.CurrentDirectory, ".mcp.json"),
    Path.Combine(Environment.CurrentDirectory, ".claude", ".mcp.json")
};

var foundMcps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

foreach (var path in possiblePaths)
{
    if (!File.Exists(path))
        continue;

    try
    {
        var content = File.ReadAllText(path);
        foreach (var mcp in requiredMcps)
        {
            // Simple check: MCP name appears in the JSON file
            // 간단한 체크: MCP 이름이 JSON 파일에 나타남
            if (content.Contains($"\"{mcp}\"", StringComparison.OrdinalIgnoreCase))
                foundMcps.Add(mcp);
        }
    }
    catch
    {
        // Ignore read errors
        // 읽기 오류 무시
    }
}

foreach (var mcp in requiredMcps)
{
    if (!foundMcps.Contains(mcp))
        missingMcps.Add(mcp);
}

if (missingMcps.Count == 0)
    return;

// Output warning message
// 경고 메시지 출력
Console.WriteLine();
Console.WriteLine("⚠️ [WPF Dev Pack] REQUIRED MCP servers are missing!");
Console.WriteLine($"⚠️ Missing: {string.Join(", ", missingMcps)}");
Console.WriteLine();
Console.WriteLine("❌ The following agents CANNOT be used without these MCPs:");
Console.WriteLine("   - wpf-architect, wpf-architect-low");
Console.WriteLine("   - wpf-code-reviewer, wpf-code-reviewer-low");
Console.WriteLine("   - wpf-control-designer, wpf-xaml-designer");
Console.WriteLine("   - wpf-mvvm-expert, wpf-data-binding-expert");
Console.WriteLine("   - wpf-performance-optimizer, serena-initializer");
Console.WriteLine();
Console.WriteLine("⚠️ WARNING: If you proceed without installing these MCPs,");
Console.WriteLine("   agent responses may be INACCURATE or INCOMPLETE.");
Console.WriteLine();
Console.WriteLine("📦 Install missing MCPs by adding to ~/.claude/.mcp.json:");
Console.WriteLine("   See: https://github.com/christian289/dotnet-with-claudecode/tree/main/wpf-dev-pack#required-mcp-dependencies");
Console.WriteLine();
