#!/usr/bin/env dotnet

// Core Policy Loader Hook (SessionStart)
//
// Injects wpf-dev-pack's always-on core policy into every Claude Code session.
// Plugin CLAUDE.md and .claude/rules files are NOT auto-loaded for installed
// users (plugins deliver context only through skills, agents, and hooks), so the
// core policy is shipped as dedicated documents under context/ and delivered here.
//
// The documents are the single source of truth; this hook only reads and emits
// them. To change the policy, edit context/*.md — not this file.
//
// Input:  stdin JSON (SessionStart payload). Consumed but unused.
// Output: the concatenated context/*.md content on stdout (system context).

// Consume stdin for protocol compatibility, even though we don't read it.
_ = Console.In.ReadToEnd();

// ${CLAUDE_PLUGIN_ROOT} is exported to hook processes; use it to locate the
// plugin-bundled context/ directory regardless of the session's cwd.
var pluginRoot = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT");
if (string.IsNullOrEmpty(pluginRoot))
    return;

var contextDir = Path.Combine(pluginRoot, "context");
if (!Directory.Exists(contextDir))
    return;

string[] files;
try
{
    files = Directory.GetFiles(contextDir, "*.md", SearchOption.TopDirectoryOnly);
    // Deterministic, alphabetical order so injection is stable across sessions.
    Array.Sort(files, StringComparer.OrdinalIgnoreCase);
}
catch
{
    return;
}

if (files.Length == 0)
    return;

Console.WriteLine("[wpf-dev-pack] Core policy — ENFORCED this session (source: context/*.md).");
Console.WriteLine();

foreach (var file in files)
{
    string content;
    try
    {
        content = File.ReadAllText(file);
    }
    catch
    {
        // Skip an unreadable file rather than failing the whole hook.
        continue;
    }

    Console.WriteLine(content.TrimEnd());
    Console.WriteLine();
}
