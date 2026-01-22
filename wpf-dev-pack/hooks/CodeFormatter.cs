#!/usr/bin/env dotnet

// Code Formatter Hook
// Formats XAML and C# files after Write or Edit operations.
// Input: stdin JSON with "tool_name" and "tool_input.file_path" fields

using System.Diagnostics;
using System.Text.Json;

// Read JSON from stdin
var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
    return;

string? toolName = null;
string? filePath = null;

try
{
    using var doc = JsonDocument.Parse(input);

    if (doc.RootElement.TryGetProperty("tool_name", out var tn))
        toolName = tn.GetString();

    if (doc.RootElement.TryGetProperty("tool_input", out var ti))
    {
        if (ti.TryGetProperty("file_path", out var fp))
            filePath = fp.GetString();
    }
}
catch
{
    return;
}

if (toolName is not "Write" and not "Edit")
    return;

if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
    return;

var ext = Path.GetExtension(filePath).ToLowerInvariant();
var workspaceRoot = FindWorkspaceRoot(Path.GetDirectoryName(filePath)!);

if (ext == ".xaml")
    FormatXaml(filePath, workspaceRoot);
else if (ext == ".cs")
    FormatCSharp(filePath);

static string FindWorkspaceRoot(string startPath)
{
    var current = startPath;
    while (!string.IsNullOrEmpty(current) && current != Path.GetPathRoot(current))
    {
        if (Directory.Exists(Path.Combine(current, ".git")) ||
            Directory.GetFiles(current, "*.sln").Length > 0 ||
            Directory.GetFiles(current, "*.slnx").Length > 0)
            return current;
        current = Path.GetDirectoryName(current);
    }
    return startPath;
}

static string? FindClosestCsproj(string filePath)
{
    var current = Path.GetDirectoryName(filePath);
    while (!string.IsNullOrEmpty(current) && current != Path.GetPathRoot(current))
    {
        var csproj = Directory.GetFiles(current, "*.csproj").FirstOrDefault();
        if (csproj is not null)
            return csproj;
        current = Path.GetDirectoryName(current);
    }
    return null;
}

static void FormatXaml(string filePath, string workspaceRoot)
{
    var configPath = Path.Combine(workspaceRoot, "Settings.XamlStyler");

    // dnx 옵션: -y (확인 프롬프트 자동 수락)
    // -- 구분자 이후 XamlStyler.Console 인자 전달
    // dnx option: -y (auto-accept confirmation prompt)
    // Arguments after -- separator are passed to XamlStyler.Console
    var toolArgs = File.Exists(configPath)
        ? $"-f \"{filePath}\" -c \"{configPath}\""
        : $"-f \"{filePath}\"";

    var dnxArgs = $"/c dnx -y XamlStyler.Console -- {toolArgs}";

    try
    {
        var psi = new ProcessStartInfo("cmd.exe", dnxArgs)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        process?.WaitForExit(30000);

        if (process?.ExitCode == 0)
            Console.WriteLine($"Formatted XAML: {Path.GetFileName(filePath)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"XamlStyler error: {ex.Message}");
    }
}

static void FormatCSharp(string filePath)
{
    var csproj = FindClosestCsproj(filePath);
    if (csproj is null)
    {
        Console.WriteLine($"No .csproj found for: {Path.GetFileName(filePath)}");
        return;
    }

    try
    {
        var psi = new ProcessStartInfo("dotnet", $"format \"{csproj}\" --include \"{filePath}\" --no-restore")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        process?.WaitForExit(30000);

        if (process?.ExitCode == 0)
            Console.WriteLine($"Formatted C#: {Path.GetFileName(filePath)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"dotnet format error: {ex.Message}");
    }
}
