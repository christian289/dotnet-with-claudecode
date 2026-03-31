#!/usr/bin/env dotnet

// HandMirror Reminder Hook (PreToolUse)
// Detects context7 or Microsoft Learn MCP tool calls related to .NET/NuGet,
// and reminds Claude to also use HandMirrorMcp for API accuracy verification.
// Input: stdin JSON with "tool_name" and "tool_input" fields

using System.Text.Json;

var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
    return;

string? toolName = null;
string? toolInput = null;

try
{
    using var doc = JsonDocument.Parse(input);
    if (doc.RootElement.TryGetProperty("tool_name", out var tn))
        toolName = tn.GetString();
    if (doc.RootElement.TryGetProperty("tool_input", out var ti))
        toolInput = ti.GetRawText();
}
catch
{
    return;
}

if (string.IsNullOrWhiteSpace(toolName))
    return;

// Target MCP tool patterns
var isContext7 = toolName.Contains("context7", StringComparison.OrdinalIgnoreCase);
var isMicrosoftLearn = toolName.Contains("microsoft", StringComparison.OrdinalIgnoreCase)
    && (toolName.Contains("learn", StringComparison.OrdinalIgnoreCase)
        || toolName.Contains("docs", StringComparison.OrdinalIgnoreCase)
        || toolName.Contains("code_sample", StringComparison.OrdinalIgnoreCase));

if (!isContext7 && !isMicrosoftLearn)
    return;

// Check if the query is .NET/NuGet related
var lowerInput = (toolInput ?? "").ToLowerInvariant();

string[] dotnetIndicators =
[
    "nuget", "nupkg", "csproj", "dotnet", ".net",
    "namespace", "assembly", "dll", "exe",
    "system.", "microsoft.",
    "wpf", "xaml", "avalonia",
    "entityframework", "ef core", "efcore",
    "aspnet", "asp.net", "blazor", "maui",
    "dapper", "mediatr", "autofac", "serilog",
    "newtonsoft", "json.net",
    "communitytoolkit", "prism",
    "fluentvalidation", "polly", "refit",
    "sqlite", "sqlserver", "npgsql",
    "grpc", "signalr", "minimal api"
];

bool isDotNetRelated = false;
foreach (var indicator in dotnetIndicators)
{
    if (lowerInput.Contains(indicator))
    {
        isDotNetRelated = true;
        break;
    }
}

if (!isDotNetRelated)
    return;

// Output reminder to also use HandMirror
Console.WriteLine("""
    [WPF Dev Pack] .NET API/NuGet query detected.
    Use HandMirrorMcp tools to verify API accuracy and reduce hallucinations:
      - inspect_nuget_package: List namespaces/types in a NuGet package
      - inspect_nuget_package_type: Get exact method signatures
      - search_nuget_packages: Search packages by keyword
      - get_type_info: Inspect local assembly (.dll/.exe) types
      - explain_build_error: Diagnose CS/NU build errors
      - analyze_csproj: Analyze project file for issues
    """);
