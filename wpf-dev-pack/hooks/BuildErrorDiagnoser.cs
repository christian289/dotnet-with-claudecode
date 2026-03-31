#!/usr/bin/env dotnet

// Build Error Auto-Diagnosis Hook
// Detects dotnet build/run failures in Bash tool output and suggests
// HandMirror MCP tools or relevant skills for error resolution.
// Input: stdin JSON with "tool_name", "tool_input.command", "tool_output.stdout/stderr"

using System.Text.Json;
using System.Text.RegularExpressions;

// Read JSON from stdin
var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
    return;

string? toolName = null;
string? command = null;
string? stdout = null;
string? stderr = null;

try
{
    using var doc = JsonDocument.Parse(input);

    if (doc.RootElement.TryGetProperty("tool_name", out var tn))
        toolName = tn.GetString();

    if (doc.RootElement.TryGetProperty("tool_input", out var ti))
    {
        if (ti.TryGetProperty("command", out var cmd))
            command = cmd.GetString();
    }

    if (doc.RootElement.TryGetProperty("tool_output", out var to))
    {
        if (to.TryGetProperty("stdout", out var so))
            stdout = so.GetString();
        if (to.TryGetProperty("stderr", out var se))
            stderr = se.GetString();
    }
}
catch
{
    return;
}

if (toolName is not "Bash")
    return;

if (string.IsNullOrEmpty(command))
    return;

// Only trigger for dotnet build/run/publish commands
if (!IsDotnetBuildCommand(command))
    return;

var output = $"{stdout}\n{stderr}";
if (string.IsNullOrWhiteSpace(output))
    return;

var diagnostics = AnalyzeBuildErrors(output);
if (diagnostics.Count == 0)
    return;

Console.WriteLine("[WPF Dev Pack] Build Error Auto-Diagnosis");
Console.WriteLine("------------------------------------------");

foreach (var diag in diagnostics.Take(5))
{
    Console.WriteLine($"  {diag.ErrorCode}: {diag.Message}");
    if (!string.IsNullOrEmpty(diag.Suggestion))
        Console.WriteLine($"    -> {diag.Suggestion}");
}

if (diagnostics.Count > 5)
    Console.WriteLine($"  (+{diagnostics.Count - 5} more errors)");

Console.WriteLine("------------------------------------------");

static bool IsDotnetBuildCommand(string command)
{
    var lower = command.ToLowerInvariant();
    return lower.Contains("dotnet build") ||
           lower.Contains("dotnet run") ||
           lower.Contains("dotnet publish") ||
           lower.Contains("dotnet test") ||
           lower.Contains("msbuild");
}

static List<BuildDiagnostic> AnalyzeBuildErrors(string output)
{
    var diagnostics = new List<BuildDiagnostic>();
    var seen = new HashSet<string>();

    foreach (Match match in ErrorPattern().Matches(output))
    {
        var code = match.Groups["code"].Value;
        var msg = match.Groups["msg"].Value.Trim();

        if (!seen.Add(code))
            continue;

        var suggestion = code switch
        {
            // Type/Namespace not found — HandMirror can inspect actual assemblies
            "CS0234" or "CS0246" =>
                "Use HandMirrorMcp `inspect_nuget_package` to verify exact namespace/type names",

            // Member not found
            "CS1061" or "CS0117" =>
                "Use HandMirrorMcp `inspect_nuget_package_type` to check exact method signatures",

            // Missing assembly reference
            "CS0012" =>
                "Use HandMirrorMcp `search_nuget_packages` to find the required NuGet package",

            // Ambiguous reference
            "CS0104" =>
                "Use HandMirrorMcp `inspect_nuget_package` to list namespaces and resolve ambiguity",

            // NuGet restore errors
            "NU1101" or "NU1102" or "NU1103" =>
                "Use HandMirrorMcp `search_nuget_packages` to find correct package name/version",

            // NuGet version conflicts
            "NU1605" or "NU1608" =>
                "Use HandMirrorMcp `compare_nuget_versions` to analyze version compatibility",

            // XAML parse errors
            _ when code.StartsWith("XDG") || code.StartsWith("MC") =>
                "XAML error — check `/wpf-dev-pack:customizing-controltemplate` or `/wpf-dev-pack:managing-styles-resourcedictionary`",

            // XAML type not found
            "XLS0414" or "XLS0418" =>
                "XAML type not found — verify xmlns namespace and assembly reference",

            _ => code switch
            {
                // Common build errors with general guidance
                "CS0103" => "Name not in scope — check using directives or variable declaration",
                "CS0029" => "Cannot implicitly convert type — check type compatibility",
                "CS0535" => "Interface member not implemented — check interface requirements",
                _ => ""
            }
        };

        diagnostics.Add(new BuildDiagnostic(code, msg, suggestion));
    }

    // Check for general build failure without specific error codes
    if (diagnostics.Count == 0 && output.Contains("Build FAILED", StringComparison.OrdinalIgnoreCase))
    {
        diagnostics.Add(new BuildDiagnostic(
            "BUILD",
            "Build failed — use HandMirrorMcp `explain_build_error` for detailed diagnosis",
            "Copy the full error output and use HandMirrorMcp `explain_build_error` tool"));
    }

    return diagnostics;
}

record BuildDiagnostic(string ErrorCode, string Message, string Suggestion);

internal static partial class ErrorPatterns
{
    [GeneratedRegex(@"error\s+(?<code>[A-Z]{2,3}\d{4})\s*:\s*(?<msg>[^\r\n]+)", RegexOptions.IgnoreCase)]
    internal static partial Regex ErrorPattern();
}

static Regex ErrorPattern() => ErrorPatterns.ErrorPattern();
