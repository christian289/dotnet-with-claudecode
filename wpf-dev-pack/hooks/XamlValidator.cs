#!/usr/bin/env dotnet

// XAML Validator Hook
// Validates XAML files after Write/Edit operations for common issues.
// Input: stdin JSON with "tool_name" and "tool_input.file_path" fields

using System.Text.Json;
using System.Text.RegularExpressions;

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

if (string.IsNullOrEmpty(filePath))
    return;

var ext = Path.GetExtension(filePath).ToLowerInvariant();
if (ext != ".xaml")
    return;

if (!File.Exists(filePath))
    return;

var content = File.ReadAllText(filePath);
var issues = XamlValidationRules.Validate(content, filePath);

if (issues.Count > 0)
{
    Console.WriteLine($"[WPF Dev Pack] XAML validation warnings for {Path.GetFileName(filePath)}:");
    foreach (var issue in issues)
        Console.WriteLine($"  - {issue.Rule}: {issue.Message}");
}

/// <summary>
/// XAML validation rules using GeneratedRegex for optimal performance.
/// </summary>
internal static partial class XamlValidationRules
{
    public static List<(string Rule, string Message)> Validate(string content, string filePath)
    {
        var issues = new List<(string Rule, string Message)>();

        // Rule 1: Missing x:Key in Style without TargetType
        if (StyleWithoutKeyOrTargetType().IsMatch(content))
            issues.Add(("Missing x:Key in Style without TargetType", "Style should have either x:Key or TargetType attribute"));

        // Rule 2: Direct content in Generic.xaml
        if (filePath.Contains("generic.xaml", StringComparison.OrdinalIgnoreCase) &&
            StyleWithTargetType().IsMatch(content))
            issues.Add(("Direct content in Generic.xaml", "Generic.xaml should only contain MergedDictionaries, not direct styles"));

        // Rule 3: Missing xmlns:x namespace
        if (ResourceDictionaryWithoutXmlnsX().IsMatch(content))
            issues.Add(("Missing xmlns:x namespace", "ResourceDictionary should include xmlns:x namespace declaration"));

        // Rule 4: TemplateBinding outside ControlTemplate
        if (TemplateBindingOutsideControlTemplate().IsMatch(content))
            issues.Add(("TemplateBinding outside ControlTemplate", "TemplateBinding should only be used inside ControlTemplate"));

        // Rule 5: Missing PART_ prefix for template parts
        if (MissingPartPrefix().IsMatch(content))
            issues.Add(("Missing PART_ prefix for template parts", "Template parts should use PART_ prefix (e.g., PART_ContentHost)"));

        return issues;
    }

    [GeneratedRegex(@"<Style\s+(?!.*x:Key)(?!.*TargetType)", RegexOptions.IgnoreCase)]
    private static partial Regex StyleWithoutKeyOrTargetType();

    [GeneratedRegex(@"<Style\s+.*TargetType.*(?<!MergedDictionaries)", RegexOptions.IgnoreCase)]
    private static partial Regex StyleWithTargetType();

    [GeneratedRegex(@"<ResourceDictionary(?![\s\S]*xmlns:x=)", RegexOptions.IgnoreCase)]
    private static partial Regex ResourceDictionaryWithoutXmlnsX();

    [GeneratedRegex(@"<(?!ControlTemplate)[\w.]+[^>]*TemplateBinding", RegexOptions.IgnoreCase)]
    private static partial Regex TemplateBindingOutsideControlTemplate();

    [GeneratedRegex(@"x:Name=""(?!PART_)[A-Z][^""]*"".*(?:ContentPresenter|Border|Grid)", RegexOptions.IgnoreCase)]
    private static partial Regex MissingPartPrefix();
}
