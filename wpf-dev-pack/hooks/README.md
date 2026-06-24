[🇰🇷 한국어](./README.ko.md)

# Hooks

Event hooks that run automatically during Claude Code operations.

## Hook List

| Hook | Trigger | Description |
|------|---------|-------------|
| **DotnetVersionChecker** | SessionStart | Verifies .NET SDK 10.0.300+ is installed (required by all hooks, which are C# file-based apps). Emits a high-visibility red warning with the install/update URL if missing or too old. Caches per day to avoid spam. |
| **LanguagePreferenceLoader** | SessionStart | Reads `.claude/wpf-dev-pack.local.md` at session start; if a `language:` field is present, emits a directive into the system context so Claude responds in that language for the rest of the session. |
| **WpfAuthoringRulesLoader** | SessionStart | Injects an always-on, enforced rule set for authoring WPF ControlTemplates / Styles / animations (required `PART_` names per stock control, animation safety, Setter-on-Freezable → MC4111, `StaticResource` forward-reference, the `(UIElement.Children)[n]` path trap, runtime verification). Plugin-bundled rules are not auto-loaded for installed users, so these ship as a hook. Full detail in the `animating-wpf-controltemplates` MCP topic. |
| **HandMirrorReminder** | PreToolUse (context7 / Microsoft Learn) | When a .NET/NuGet documentation lookup runs, reminds the agent to verify exact namespaces/signatures with HandMirrorMcp before writing code. |
| **RepoPathGuard** | PreToolUse (WpfDevPackMcp) | Blocks `WpfDevPackMcp` tool calls when the knowledge repo path is unconfigured, instructing the user to run `/wpf-dev-pack:set-repo-path`. |
| **McpDependencyChecker** | UserPromptSubmit | Checks for required MCP servers (context7, serena, microsoft-learn) once per session and warns if any are missing. |
| **XamlValidator** | PostToolUse (Edit/Write `*.xaml`) | Validates XAML syntax after edits. |
| **MvvmViolationDetector** | PostToolUse (Edit/Write `*.cs`) | Flags MVVM layer violations (e.g. `System.Windows` UI types in a ViewModel) after C# edits. |
| **CodeFormatter** | PostToolUse (Edit/Write `*.cs` / `*.xaml`) | Formats C# (`dotnet format`) and XAML (XamlStyler) after file modifications. |
| **BuildErrorDiagnoser** | PostToolUse (Bash) | Parses build output after Bash commands and explains CS/NU/MSB errors with HandMirrorMcp next-steps. |
| **FeedbackDocAuditor** | PostToolUse (skill-scoped) | Audits `*-wpf-dev-pack-feedback.md` files for identifying information; exits with code 2 on violation. Wired through `skills/collecting-wpf-dev-pack-feedback/SKILL.md` frontmatter (not `hooks.json`), so it fires only while that skill is active. |

## Files

| File | Description |
|------|-------------|
| `hooks.json` | Hook configuration and triggers |
| `DotnetVersionChecker.cs` | .NET SDK 10.0.300+ presence/version check (SessionStart) |
| `LanguagePreferenceLoader.cs` | Per-project language preference loader (SessionStart) |
| `WpfAuthoringRulesLoader.cs` | Injects enforced WPF ControlTemplate/Style/animation authoring rules (SessionStart) |
| `HandMirrorReminder.cs` | Reminder to verify .NET APIs with HandMirrorMcp (PreToolUse) |
| `RepoPathGuard.cs` | Blocks `WpfDevPackMcp` calls until the knowledge repo path is set (PreToolUse) |
| `McpDependencyChecker.cs` | Required-MCP availability check (UserPromptSubmit) |
| `XamlValidator.cs` | XAML syntax validation |
| `MvvmViolationDetector.cs` | MVVM violation detection in C# edits |
| `CodeFormatter.cs` | Code formatting with XamlStyler and dotnet format |
| `BuildErrorDiagnoser.cs` | Build error (CS/NU/MSB) diagnosis after Bash |
| `FeedbackDocAuditor.cs` | Anonymity audit for newly written feedback documents |

## How It Works

1. **SessionStart hooks** run when a session begins
2. **UserPromptSubmit hooks** run when you submit a prompt
3. **PreToolUse hooks** run before Claude executes a tool
4. **PostToolUse hooks** run after a tool completes
5. Hooks can modify behavior or provide feedback

## Requirements

- .NET SDK 10.0.300+ for hook execution
- XamlStyler for XAML formatting (optional)
