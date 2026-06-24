# dotnet-with-claudecode Project Configuration

## Repository Overview

This repository is a **marketplace repository** that hosts .NET-focused
Claude Code plugins.

- `.claude/rules` at the repository root contains only universal rules
  that apply to every plugin.
- Plugin-specific settings (hooks, permissions, etc.) live inside each
  individual plugin directory.
- Currently hosted plugins: `wpf-dev-pack` (single).

## Bilingual Documentation Convention (`.claude.ko/`)

This repository keeps Korean translations of the `.claude/` rules in
a parallel `.claude.ko/` directory that mirrors `.claude/`'s folder
structure. The convention is:

- **Files under `.claude/`** — **the source of truth that the AI reads.**
  English. All rule loading, system-prompt injection, and automated
  behavior is driven by these files.
- **Files under `.claude.ko/`** — **human reference only. The AI does
  not read these.** They are Korean translations maintained for the
  maintainer and any Korean-reading contributors. The directory lives
  outside `.claude/`, so Claude Code's `.claude/rules/**/*.md`
  auto-loader does not pick them up.

For every `.claude/<path>.md`, the Korean mirror lives at
`.claude.ko/<same-path>.md`. The `.ko` suffix is no longer needed on
individual filenames — the directory name carries the language
indicator.

When updating a rule or guideline, edit the file under `.claude/`
first, then mirror the change into the corresponding `.claude.ko/`
file. Do not reverse the direction — if the two diverge, the
`.claude/` version wins.

This convention applies to:

- `CLAUDE.md` at root level (`.claude/CLAUDE.md` ↔ `.claude.ko/CLAUDE.md`).
- Every file under `.claude/rules/` (mirrored under `.claude.ko/rules/`).
- Repository-facing documents that live outside `.claude/` — these use
  the in-place `<name>.ko.md` suffix because they have no auto-load
  concern: `README.md` / `README.ko.md`, the CONTRIBUTING pair under
  `.github/`, the hooks READMEs under `wpf-dev-pack/hooks/`, etc.

It does not apply to plugin-bundled, AI-facing content, which is
English-only by skill-content policy (see
`.claude/rules/claude-skills/best-practices.md`): `SKILL.md` files and
the session-injected policy docs under `wpf-dev-pack/context/` (delivered
by the `CorePolicyLoader` SessionStart hook). Plugins do not carry
`CLAUDE.md` files — a plugin `CLAUDE.md` is not auto-loaded for installed
users, so plugin context ships through skills, agents, and hooks instead.

## Directory Layout

```
dotnet-with-claudecode/
├── .claude/
│   ├── CLAUDE.md                 # this file (repo-wide configuration)
│   └── rules/                    # rules shared across all plugins
│       ├── claude-rules/         # how to write memory/rule files
│       ├── claude-skills/        # how to write Skills
│       ├── dotnet/               # C#, WPF, AvaloniaUI, spreadsheet
│       ├── secure-coding/        # secure-coding guidelines
│       └── preferences.md        # default behavior (response language, etc.)
├── mcp/                          # WpfDevPackMcp MCP server source (outside the plugin)
├── knowledge/                    # MCP knowledge topics (served by WpfDevPackMcp; outside the plugin so it is not bundled)
├── wpf-dev-pack/                 # WPF-focused plugin (currently the only hosted plugin)
├── FeedbackDocs/                 # accumulated wpf-dev-pack feedback md files from foreign sessions
├── archive-skills/               # legacy skills superseded by the microsoft-docs MCP
└── docs/                         # project documentation
```

## Plugin Version Update Checklist

**Version bumps are performed via the `/wpf-dev-pack-release` skill only.**
That skill updates the files below in lockstep. No other workflow
(feedback application, new skill addition, hook addition, documentation
edits, anything else) touches the `version` field directly. When a
change requires a version bump, complete the work and then invoke
`/wpf-dev-pack-release` separately.

Files that the release skill updates:

- `<plugin>/.claude-plugin/plugin.json` — `version` field
- `<plugin>/README.md` — version badge
- `<plugin>/README.ko.md` — version badge
- `docs/changelogs/<plugin>.md` — new version entry

**WpfDevPackMcp NuGet package versioning:** The `WpfDevPackMcp` MCP server is
distributed as a separate NuGet package and pinned in `wpf-dev-pack/.mcp.json`
via its `dnx` version specifier. Its version is independent of the plugin
version. Knowledge-only edits (files under `knowledge/`) require
**NO plugin version bump and NO MCP republish** — the server reads knowledge
content live from the repo on each call. Only changes shipped inside the plugin
package itself (command skills, hooks, rules) require a plugin version bump
via `/wpf-dev-pack-release`.

**Publishing `WpfDevPackMcp` to NuGet (each release):** NuGet versions are
immutable, so bump first and re-pin last.

1. Verify: `dotnet test mcp/WpfDevPackMcp.Tests`, then exercise the tools with
   the MCP Inspector (`npx @modelcontextprotocol/inspector "<built exe>"`).
2. Bump `<Version>` in `mcp/WpfDevPackMcp.csproj`.
3. `dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg`
4. `dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg --source https://api.nuget.org/v3/index.json --api-key <KEY>`
5. After nuget.org indexes it (verify with `dnx WpfDevPackMcp@<ver> --yes`),
   update the `dnx` pin in `wpf-dev-pack/.mcp.json` (`@<old>` → `@<ver>`).

Keep the API key secret (never commit). The first one-time setup (API key,
package-id ownership) and full detail are in
[`mcp/README.md`](../mcp/README.md), which also covers build and MCP Inspector
usage.

## Maintainer Workflow

Slash commands a maintainer of this repository uses regularly:

| Command | Scope | Purpose |
|---------|-------|---------|
| `/applying-wpf-dev-pack-feedback <file.md>` | repo | Reflect one FeedbackDocs entry into `wpf-dev-pack`, move the source md into `FeedbackDocs/`, and append a row to `FeedbackDocs/APPLIED-LOG.md`. Does not commit. |
| `/wpf-dev-pack:configuring-wpf-dev-pack-language [code]` | plugin | Write `.claude/wpf-dev-pack.local.md` with a BCP-47 `language:` field. Takes effect from the next session. |
| `/wpf-dev-pack:set-repo-path <path>` | plugin | Configure the local clone path WpfDevPackMcp reads knowledge from. Required before the MCP tools work. |
| `/wpf-dev-pack-release` | repo | The only path to bump the plugin version. Updates `plugin.json`, both README badges, and `docs/changelogs/wpf-dev-pack.md` in lockstep. See "Plugin Version Update Checklist" above. |

### Local Plugin Testing

To test in-progress plugin changes against the local checkout:

```
/plugin marketplace remove dotnet-claude-plugins
/plugin marketplace add <absolute-path-to-this-repo>
/plugin install wpf-dev-pack@dotnet-claude-plugins
/reload-plugins      # after every subsequent change
```

Alternative for a single isolated test session: launch
`claude --plugin-dir <absolute-path>/wpf-dev-pack`.

## AvaloniaUI Skills

The AvaloniaUI-specific skills currently managed in this project.

> **📌 Note**: WPF-related skills have been migrated into [wpf-dev-pack](./wpf-dev-pack).

| Skill | Description |
|-------|-------------|
| `configuring-avalonia-dependency-injection` | AvaloniaUI DI setup (GenericHost) |
| `designing-avalonia-customcontrol-architecture` | AvaloniaUI CustomControl architecture |
| `structuring-avalonia-projects` | AvaloniaUI project / solution structure |
| `using-avalonia-collectionview` | DataGridCollectionView, ReactiveUI patterns |
| `fixing-avaloniaui-radialgradientbrush` | Workaround for RadialGradientBrush compatibility |
| `converting-html-css-to-wpf-xaml` | HTML/CSS → WPF XAML conversion |
