#!/usr/bin/env dotnet

// Language Preference Loader Hook (SessionStart)
//
// At the start of every Claude Code session in a project where the
// wpf-dev-pack plugin is installed, this hook checks for the file
// .claude/wpf-dev-pack.local.md. If the file exists and a `language:`
// field is present (typically inside its YAML frontmatter), the hook
// writes a short directive to stdout instructing Claude to respond in
// that language for the remainder of the session.
//
// If the file is absent or has no `language:` field, the hook emits
// nothing and exits cleanly. The plugin's default language behavior
// then applies.
//
// Configured by the user via:
//   /wpf-dev-pack:configuring-wpf-dev-pack-language
//
// Input:  stdin JSON (SessionStart payload). Consumed but unused.
// Output: a system-context directive on stdout, when applicable.

using System.Text.RegularExpressions;

// Consume stdin for protocol compatibility, even though we don't read it.
_ = Console.In.ReadToEnd();

var configPath = Path.Combine(".claude", "wpf-dev-pack.local.md");
if (!File.Exists(configPath))
    return;

string content;
try
{
    content = File.ReadAllText(configPath);
}
catch
{
    return;
}

var lang = LanguagePreference.Extract(content);
if (string.IsNullOrEmpty(lang))
    return;

var displayName = LanguagePreference.DisplayName(lang);

Console.WriteLine($"[wpf-dev-pack] Per-project language preference active: {displayName} (`{lang}`).");
Console.WriteLine();
Console.WriteLine($"For the remainder of this session, respond to the user in {displayName} (BCP-47 `{lang}`).");
Console.WriteLine("This overrides any conflicting default language preference for wpf-dev-pack-scoped interactions.");
Console.WriteLine("Skill content language policy (SKILL.md body, code comments) is unaffected — it remains English.");
Console.WriteLine();
Console.WriteLine("Source: .claude/wpf-dev-pack.local.md");
Console.WriteLine("To change: run /wpf-dev-pack:configuring-wpf-dev-pack-language");
Console.WriteLine("To revert: delete the file or remove its `language:` field");


internal static partial class LanguagePreference
{
    public static string? Extract(string content)
    {
        var match = LanguageField().Match(content);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    public static string DisplayName(string code) => code.ToLowerInvariant() switch
    {
        "ko" or "kr" or "ko-kr" => "Korean",
        "en" or "en-us" or "en-gb" => "English",
        "ja" or "jp" or "ja-jp" => "Japanese",
        "zh" or "zh-cn" or "zh-tw" or "zh-hk" => "Chinese",
        "es" or "es-es" or "es-mx" => "Spanish",
        "fr" or "fr-fr" => "French",
        "de" or "de-de" => "German",
        _ => code
    };

    // Match `language:` on its own line, tolerant of leading whitespace
    // and optional surrounding quotes. Picks the first occurrence in
    // the file, which in practice means the YAML frontmatter entry.
    [GeneratedRegex(@"^\s*language\s*:\s*[""']?([\w-]+)[""']?\s*$",
        RegexOptions.Multiline)]
    private static partial Regex LanguageField();
}
