[🇰🇷 한국어](./README.ko.md)

# Hooks

Event hooks that run automatically during Claude Code operations.

## Hook List

| Hook | Trigger | Description |
|------|---------|-------------|
| **WpfKeywordDetector** | PreToolUse | Detects WPF/C#/.NET keywords and auto-activates relevant skills |
| **CodeFormatter** | PostToolUse | Formats C# and XAML code after file modifications |
| **McpDependencyChecker** | PreToolUse | Checks for required MCP server availability |
| **XamlValidator** | PostToolUse | Validates XAML syntax after edits |
| **FeedbackDocAuditor** | PostToolUse (skill-scoped) | Audits `*-wpf-dev-pack-feedback.md` files for identifying information; exits with code 2 on violation. Wired through `skills/collecting-wpf-dev-pack-feedback/SKILL.md` frontmatter (not `hooks.json`), so it fires only while that skill is active. |
| **LanguagePreferenceLoader** | SessionStart | Reads `.claude/wpf-dev-pack.local.md` at session start; if a `language:` field is present, emits a directive into the system context so Claude responds in that language for the rest of the session. |
| **DotnetVersionChecker** | SessionStart | Verifies that .NET SDK 10.0.300 or higher is installed (required by all wpf-dev-pack hooks, which are C# file-based apps). If missing or below the required version, emits a high-visibility red warning with the install / update URL. Caches per day to avoid spam. |

## Files

| File | Description |
|------|-------------|
| `hooks.json` | Hook configuration and triggers |
| `WpfKeywordDetector.cs` | Keyword detection logic |
| `CodeFormatter.cs` | Code formatting with XamlStyler and dotnet format |
| `McpDependencyChecker.cs` | MCP dependency verification |
| `XamlValidator.cs` | XAML syntax validation |
| `FeedbackDocAuditor.cs` | Anonymity audit for newly written feedback documents |
| `LanguagePreferenceLoader.cs` | Per-project language preference loader (SessionStart) |
| `DotnetVersionChecker.cs` | .NET SDK 10.0.300+ presence/version check (SessionStart) |

## How It Works

1. **PreToolUse hooks** run before Claude executes a tool
2. **PostToolUse hooks** run after a tool completes
3. Hooks can modify behavior or provide feedback

## Requirements

- .NET SDK 10.0.300+ for hook execution
- XamlStyler for XAML formatting (optional)
