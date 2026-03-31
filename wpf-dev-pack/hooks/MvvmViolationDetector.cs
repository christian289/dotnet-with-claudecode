#!/usr/bin/env dotnet

// MVVM Violation Detector Hook
// Detects System.Windows references in ViewModel files after Write/Edit operations.
// Excludes: .UI projects, Converter files, Service files, View code-behind files.
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
if (ext != ".cs")
    return;

if (!File.Exists(filePath))
    return;

// Only check ViewModel files
if (!IsViewModelFile(filePath))
    return;

// Skip excluded patterns
if (IsExcludedFile(filePath))
    return;

var content = File.ReadAllText(filePath);
var violations = DetectViolations(content);

if (violations.Count > 0)
{
    var fileName = Path.GetFileName(filePath);
    Console.WriteLine($"[WPF Dev Pack] MVVM Violation Warning in {fileName}:");
    foreach (var violation in violations)
        Console.WriteLine($"  - {violation}");
    Console.WriteLine("  Tip: ViewModel should only reference BCL types and MVVM framework packages.");
    Console.WriteLine("  See `/wpf-dev-pack:implementing-communitytoolkit-mvvm` for MVVM best practices.");
}

static bool IsViewModelFile(string filePath)
{
    var fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
    var dirPath = Path.GetDirectoryName(filePath)?.ToLowerInvariant() ?? "";

    // File name contains "viewmodel" or "vm"
    if (fileName.Contains("viewmodel") || fileName.EndsWith("vm"))
        return true;

    // File is in a ViewModels directory
    if (dirPath.Contains("viewmodels") || dirPath.Contains("viewmodel"))
        return true;

    return false;
}

static bool IsExcludedFile(string filePath)
{
    var lower = filePath.Replace('\\', '/').ToLowerInvariant();

    // Exclude .UI projects (CustomControl libraries)
    if (lower.Contains(".ui/") || lower.Contains(".ui\\"))
        return true;

    // Exclude Converter files
    if (lower.Contains("/converters/") || lower.Contains("\\converters\\"))
        return true;
    if (lower.Contains("converter.cs"))
        return true;

    // Exclude Service files
    if (lower.Contains("/services/") || lower.Contains("\\services\\"))
        return true;

    // Exclude View code-behind
    if (lower.EndsWith(".xaml.cs"))
        return true;

    return false;
}

static List<string> DetectViolations(string content)
{
    var violations = new List<string>();

    // Check for WPF namespace references
    if (SystemWindowsUsing().IsMatch(content))
        violations.Add("'using System.Windows.*' detected — ViewModel must not reference WPF namespaces");

    // Check for PresentationFramework types
    if (PresentationFrameworkRef().IsMatch(content))
        violations.Add("PresentationFramework type reference detected (Visibility, Brush, etc.)");

    // Check for WPF-specific types commonly misused in ViewModels
    if (WpfTypeUsage().IsMatch(content))
        violations.Add("WPF-specific type used (ICollectionView, CollectionViewSource, Dispatcher, etc.)");

    return violations;
}

internal static partial class ViolationPatterns
{
    // Detects: using System.Windows; using System.Windows.Controls; etc.
    // Excludes: using System.Windows.Input (System.Windows.Input.ICommand is allowed in ViewModels)
    [GeneratedRegex(@"^\s*using\s+System\.Windows(?!\.Input\s*;)(\.\w+)*\s*;", RegexOptions.Multiline)]
    internal static partial Regex SystemWindowsUsing();

    // Detects direct PresentationFramework type usage
    [GeneratedRegex(@"\b(Visibility\s*\.(?:Visible|Collapsed|Hidden)|System\.Windows\.Visibility|Thickness\b|CornerRadius\b|FontWeight\b|SolidColorBrush\b|Brush\b(?!\s*=))")]
    internal static partial Regex PresentationFrameworkRef();

    // Detects WPF types that should not be in ViewModels
    [GeneratedRegex(@"\b(ICollectionView\b|CollectionViewSource\b|Dispatcher\b(?!Priority)|DispatcherObject\b|DependencyObject\b|DependencyProperty\b|FrameworkElement\b|UIElement\b)")]
    internal static partial Regex WpfTypeUsage();
}

static Regex SystemWindowsUsing() => ViolationPatterns.SystemWindowsUsing();
static Regex PresentationFrameworkRef() => ViolationPatterns.PresentationFrameworkRef();
static Regex WpfTypeUsage() => ViolationPatterns.WpfTypeUsage();
