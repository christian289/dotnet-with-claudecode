#!/usr/bin/env dotnet

// Microsoft Skill Creator Reminder Hook (PreToolUse, skill-scoped)
//
// Wired through:
//   .claude/skills/applying-wpf-dev-pack-feedback/SKILL.md frontmatter
//
// Fires before a Write tool call whose target is a SKILL.md inside a
// wpf-dev-pack skill directory AND when the file does not yet exist
// (i.e., a NEW skill is being created as part of applying a feedback
// document).
//
// On fire, the hook emits a reminder telling Claude to:
//   1) Check whether the Microsoft Learn MCP server / microsoft-docs
//      plugin is available in the current environment.
//   2) If yes, prefer scaffolding the new skill via the
//      `/microsoft-docs:microsoft-skill-creator` skill, because the
//      feedback typically concerns Microsoft / .NET technologies and
//      a skill grounded in official docs drifts less.
//   3) If not, recommend installing the microsoft-docs plugin (or
//      registering the MCP) before authoring the skill.
//
// The hook itself cannot probe runtime MCP / available-skills state.
// Claude has that visibility and is expected to do the actual check
// after seeing this reminder.
//
// Input: stdin JSON (PreToolUse payload). Read for `tool_name` and
//        `tool_input.file_path`.

using System.Text.Json;

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

if (toolName is not "Write")
    return;
if (string.IsNullOrEmpty(filePath))
    return;

// Normalize for path matching.
var normalized = filePath.Replace('\\', '/');
var lower      = normalized.ToLowerInvariant();

// Must target a SKILL.md inside a wpf-dev-pack skills directory.
// Accept "wpf-dev-pack/skills/<name>/SKILL.md" and the legacy no-hyphen
// variant just in case. Knowledge topics are now TOPIC.md (not SKILL.md),
// so they are intentionally excluded here.
var matchesPath =
    (lower.Contains("/wpf-dev-pack/skills/") || lower.Contains("/wpf-devpack/skills/")) &&
    lower.EndsWith("/skill.md");

if (!matchesPath)
    return;

// Only fire for NEW files. If the SKILL.md already exists, the Write
// is updating an existing skill — no need to suggest a scaffolder.
if (File.Exists(filePath))
    return;

Console.WriteLine();
Console.WriteLine("[applying-wpf-dev-pack-feedback] About to create a NEW skill:");
Console.WriteLine($"  {normalized}");
Console.WriteLine();
Console.WriteLine("BEFORE writing the SKILL.md from scratch, check the available-skills");
Console.WriteLine("list and MCP servers of the current session:");
Console.WriteLine();
Console.WriteLine("  1) Is `microsoft-docs:microsoft-skill-creator` in available skills?");
Console.WriteLine("  2) Is the Microsoft Learn MCP server (microsoft-learn or");
Console.WriteLine("     plugin_microsoft-docs_microsoft-learn) registered and reachable?");
Console.WriteLine();
Console.WriteLine("If YES to both, and the new skill is about a Microsoft / .NET / Azure /");
Console.WriteLine("WPF / VS Code / Bicep / etc. technology — PREFER scaffolding via:");
Console.WriteLine();
Console.WriteLine("    /microsoft-docs:microsoft-skill-creator");
Console.WriteLine();
Console.WriteLine("It generates a hybrid skill grounded in official Microsoft docs,");
Console.WriteLine("which significantly reduces hallucinated API signatures and outdated");
Console.WriteLine("patterns compared to ground-up authoring.");
Console.WriteLine();
Console.WriteLine("If either is missing, recommend installing the microsoft-docs plugin or");
Console.WriteLine("registering the Microsoft Learn MCP before authoring the skill — the");
Console.WriteLine("resulting skill will be more accurate.");
Console.WriteLine();
Console.WriteLine("If the topic is NOT a Microsoft / .NET technology (rare for");
Console.WriteLine("wpf-dev-pack feedback), continue with normal authoring.");
Console.WriteLine();
