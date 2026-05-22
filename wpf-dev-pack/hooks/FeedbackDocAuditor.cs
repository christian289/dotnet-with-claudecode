#!/usr/bin/env dotnet

// Feedback Document Auditor Hook
// Audits wpf-dev-pack feedback markdown files for identifying information
// after Write/Edit operations.
//
// Wired through:
//   skills/collecting-wpf-dev-pack-feedback/SKILL.md frontmatter `hooks:`
// NOT through the plugin-global hooks/hooks.json. As a skill-scoped hook,
// it fires only while the `collecting-wpf-dev-pack-feedback` skill is
// active, so it will not run during unrelated edits to feedback markdown
// files in other sessions.
//
// Matches: files whose name ends with "-wpf-dev-pack-feedback.md"
// Skips:   files located inside any "FeedbackDocs" directory. The skill
//          itself writes only to the working directory outside that
//          corpus, but this check remains as defense in depth in case a
//          maintainer ever invokes the skill while editing inside it.
//
// On violation: writes a diagnostic to stderr and exits with code 2,
// which Claude Code surfaces back to the model as a system feedback
// message. The model is then expected to re-run Step 5 (Audit) of the
// collecting-wpf-dev-pack-feedback skill.
//
// This pattern-based hook is a first-line safety net. It catches the
// mechanically detectable cases (forbidden metadata fields, absolute
// paths, emails, dates, missing required sections). Subtle identifiers
// such as project-specific noun phrases cannot be detected here and
// still require the model audit in Step 5.
//
// Input: stdin JSON with "tool_name" and "tool_input.file_path" fields

using System.Text.Json;
using System.Text.RegularExpressions;

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

    if (doc.RootElement.TryGetProperty("tool_input", out var ti) &&
        ti.TryGetProperty("file_path", out var fp))
        filePath = fp.GetString();
}
catch
{
    return;
}

if (toolName is not "Write" and not "Edit")
    return;

if (string.IsNullOrEmpty(filePath))
    return;

var fileName = Path.GetFileName(filePath);
if (!fileName.EndsWith("-wpf-dev-pack-feedback.md", StringComparison.OrdinalIgnoreCase))
    return;

// Skip files that live inside a FeedbackDocs directory. Those are
// already-accepted corpus artifacts; this hook only protects newly
// created documents in the working directory.
var normalizedDir = (Path.GetDirectoryName(filePath) ?? string.Empty)
    .Replace('\\', '/')
    .ToLowerInvariant();
if (normalizedDir.Contains("/feedbackdocs"))
    return;

if (!File.Exists(filePath))
    return;

var content = File.ReadAllText(filePath);
var violations = FeedbackDocAuditor.Audit(content);

if (violations.Count == 0)
    return;

Console.Error.WriteLine($"[wpf-dev-pack] Feedback document audit failed: {fileName}");
Console.Error.WriteLine();
foreach (var v in violations)
{
    Console.Error.WriteLine($"  - {v.RuleId}: {v.Message}");
    if (!string.IsNullOrEmpty(v.Evidence))
        Console.Error.WriteLine($"      evidence: {v.Evidence}");
}
Console.Error.WriteLine();
Console.Error.WriteLine("Re-run Step 5 (Audit) of `collecting-wpf-dev-pack-feedback`:");
Console.Error.WriteLine("  1. Edit the document to remove the identifying tokens above.");
Console.Error.WriteLine("  2. Replace project-specific class / namespace / member names");
Console.Error.WriteLine("     with neutral placeholders (XxxView, XxxViewModel, IXxxService).");
Console.Error.WriteLine("  3. Rewrite each item as a Phenomenon -> Cause -> Effect chain");
Console.Error.WriteLine("     that any reader could reproduce without knowing the origin.");
Console.Error.WriteLine();
Console.Error.WriteLine("This pattern-based hook catches only the mechanical cases.");
Console.Error.WriteLine("After fixing, re-run the full Step 5 model audit before reporting.");

Environment.Exit(2);


internal sealed record AuditViolation(string RuleId, string Message, string Evidence);

internal static partial class FeedbackDocAuditor
{
    public static List<AuditViolation> Audit(string content)
    {
        var violations = new List<AuditViolation>();
        var withoutCode = StripCodeBlocks(content);

        foreach (Match m in ProhibitedMetadataField().Matches(content))
        {
            violations.Add(new AuditViolation(
                "F-001",
                "Prohibited metadata field. Feedback documents must not record when, where, or by whom the issue was encountered.",
                Truncate(m.Value.Trim(), 80)));
        }

        foreach (Match m in WindowsAbsolutePath().Matches(withoutCode))
        {
            violations.Add(new AuditViolation(
                "F-002",
                "Originating absolute Windows path. Replace with a generic shape such as `Views/XxxView.xaml`, or remove the sentence.",
                Truncate(m.Value, 80)));
        }

        foreach (Match m in UnixAbsolutePath().Matches(withoutCode))
        {
            violations.Add(new AuditViolation(
                "F-003",
                "Originating absolute Unix path. Replace with a generic shape, or remove.",
                Truncate(m.Value, 80)));
        }

        foreach (Match m in EmailAddress().Matches(withoutCode))
        {
            violations.Add(new AuditViolation(
                "F-004",
                "Email-shaped token. Remove or replace.",
                m.Value));
        }

        foreach (Match m in DateLiteral().Matches(withoutCode))
        {
            violations.Add(new AuditViolation(
                "F-005",
                "Date literal outside a code block. Do not record when the issue was encountered.",
                m.Value));
        }
        foreach (Match m in KoreanDateLiteral().Matches(withoutCode))
        {
            violations.Add(new AuditViolation(
                "F-005",
                "Korean date literal outside a code block. Do not record when the issue was encountered.",
                m.Value));
        }

        if (!RequiredH1().IsMatch(content))
        {
            violations.Add(new AuditViolation(
                "F-006",
                "Missing required H1 header (`# wpf-dev-pack Feedback - <title>` or `# wpf-dev-pack 피드백 - <title>`).",
                string.Empty));
        }

        if (!RequiredSummary().IsMatch(content))
        {
            violations.Add(new AuditViolation(
                "F-007",
                "Missing required Summary section (`## 0. Summary` or `## 0. 요약`).",
                string.Empty));
        }

        if (!AtLeastOneNumberedItem().IsMatch(content))
        {
            violations.Add(new AuditViolation(
                "F-008",
                "No numbered feedback item found (`## 1.`, `## 2.`, ...). At least one item is required.",
                string.Empty));
        }

        return violations;
    }

    private static string StripCodeBlocks(string content)
    {
        // Replace fenced code blocks with blank-line-preserving whitespace so
        // line-anchored regex still aligns. Then strip inline code spans.
        var noFenced = FencedCodeBlock().Replace(content, m =>
            new string('\n', CountChar(m.Value, '\n')));
        return InlineCode().Replace(noFenced, string.Empty);
    }

    private static int CountChar(string s, char c)
    {
        var count = 0;
        for (var i = 0; i < s.Length; i++)
            if (s[i] == c) count++;
        return count;
    }

    private static string Truncate(string s, int max)
    {
        s = s.Replace('\r', ' ').Replace('\n', ' ');
        return s.Length <= max ? s : s[..max] + "...";
    }

    // F-001: bullet-list metadata fields that record origin.
    [GeneratedRegex(
        @"^\s*-\s*\*\*(?:작성일|출처|Date written|Source|Originating project|Encountered in)\*\*",
        RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex ProhibitedMetadataField();

    // F-002: drive-letter Windows absolute paths. Drive letter is case
    // insensitive in Windows; accept both forms.
    [GeneratedRegex(@"\b[A-Za-z]:[\\/][A-Za-z0-9_.\\/\-]+")]
    private static partial Regex WindowsAbsolutePath();

    // F-003: Unix absolute paths under common top-level user / system dirs.
    // Negative lookbehind excludes word chars, colons, and dots before the
    // leading slash so URL paths (e.g., https://host/users/...) do not match.
    [GeneratedRegex(@"(?<![A-Za-z0-9_:.])/(?:Users|home|mnt|var|opt|etc|root|tmp)/[A-Za-z0-9_./\-]+")]
    private static partial Regex UnixAbsolutePath();

    [GeneratedRegex(@"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b")]
    private static partial Regex EmailAddress();

    [GeneratedRegex(@"\b\d{4}-\d{2}-\d{2}\b")]
    private static partial Regex DateLiteral();

    [GeneratedRegex(@"\d{4}년\s*\d{1,2}월(?:\s*\d{1,2}일)?")]
    private static partial Regex KoreanDateLiteral();

    [GeneratedRegex(@"^#\s+wpf-dev-pack\s+(?:Feedback|피드백)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex RequiredH1();

    [GeneratedRegex(@"^##\s+0\.\s+(?:Summary|요약)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex RequiredSummary();

    [GeneratedRegex(@"^##\s+\d+\.\s+\S", RegexOptions.Multiline)]
    private static partial Regex AtLeastOneNumberedItem();

    [GeneratedRegex(@"```[\s\S]*?```")]
    private static partial Regex FencedCodeBlock();

    [GeneratedRegex(@"`[^`\n]+`")]
    private static partial Regex InlineCode();
}
