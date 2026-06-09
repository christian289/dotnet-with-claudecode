# WPF Dev Pack - Auto-Trigger System

Automatically activates relevant skills when WPF/C#/.NET keywords are detected.

---

## Required Plugin Dependencies

All agents require these Claude Code plugins to be installed:

| Plugin | MCP Server | Purpose |
|--------|-----------|---------|
| **context7** | context7 | Library/framework documentation |
| **microsoft-docs** | microsoft-learn | Official Microsoft documentation |
| **csharp-lsp** | csharp | C# LSP code intelligence |

## Required MCPs (NOT installed as Claude Code plugins)

The following MCP server is required by agents but **must NOT be registered through the Claude Code plugin path** — Claude Code's built-in tool descriptions strongly bias the model away from using its tools when registered that way. Install directly as an MCP server via `uv`.

| MCP Server | Purpose | Installation |
|---|---|---|
| **serena** | Semantic code analysis, symbol navigation | Install directly via `uv` per the [Quick Start](https://github.com/oraios/serena#quick-start). See the [Attention note in the Serena Claude Code docs](https://oraios.github.io/serena/02-usage/030_clients.html#claude-code) for the rationale. |

---

## MVVM Composition Style

wpf-dev-pack enforces a **single matching path per MVVM framework**, both
based on **Stateful ViewModel**. The composition direction differs by
framework. For full terminology and Microsoft references, see
[`docs/TERMINOLOGY.md`](../docs/TERMINOLOGY.md).

| MVVM framework | Composition Direction | Mechanism | Wiring rules |
|---|---|---|---|
| CommunityToolkit.Mvvm (default) | **ViewModel First** | `Mappings.xaml` + implicit DataTemplate | `rules/view-viewmodel-wiring-communitytoolkit.md` |
| Prism 9 (alternative) | **View First** | `RegisterForNavigation` + `IRegionManager` | `rules/view-viewmodel-wiring-prism.md` |

> Pre-v1.6.4 docs uniformly labeled this as "View First MVVM". That single
> label was incorrect per Microsoft's official definition (lookup key for
> `Mappings.xaml` is the ViewModel type → ViewModel First). v1.6.4 corrects
> the labels per path. The enforced code patterns are unchanged.

See `rules/prohibitions.md` (P-001…P-004) for banned alternatives
(`ViewModelLocator`, code-behind `DataContext` assignment, Stateless VM
pattern, mixing the two paths, etc.).

---

## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read this section:

1. **No ViewModelLocator** — Use DI + DataTemplate mapping only (`rules/prohibitions.md`)
2. **No System.Windows in ViewModel** — BCL types only (`rules/mvvm-constraints.md`)
3. **Freeze all Freezable objects** — Brush, Pen, Geometry (`rules/freezable-performance.md`)
4. **Generic.xaml = MergedDictionaries hub only** (`rules/resourcedictionary-patterns.md`)
5. **Verify API signatures with HandMirror before writing code**
6. **Single matching path per framework** — ViewModel First (CommunityToolkit, `Mappings.xaml`) or View First (Prism, `RegisterForNavigation`). See `docs/TERMINOLOGY.md` and `rules/` for framework-specific wiring.
7. **WPF knowledge topics are fetched via `WpfDevPackMcp get_wpf_topic(id)`** — not loaded from `skills/`.

---

## Per-Project Language Preference

The plugin supports a per-project response-language preference, read by
the `LanguagePreferenceLoader` SessionStart hook at the start of every
new conversation.

- **Configure**: run `/wpf-dev-pack:configuring-wpf-dev-pack-language`
  to write `.claude/wpf-dev-pack.local.md` with a `language:` field
  (BCP-47 code, e.g. `ko`, `en`, `ja`, `zh`).
- **Effect**: from the next session onward, the hook injects a directive
  into the system context telling Claude to respond in that language.
  The current session is not affected by an in-session change because
  SessionStart hooks fire only at session start.
- **Scope**: applies to user-facing responses within the wpf-dev-pack
  context. Skill content language policy (SKILL.md body, code comments)
  is unaffected — it remains English.
- **Override**: the user can always override in-conversation
  ("respond in English" / "한글로 답해줘"). The hook only sets the
  default for the session.
- **Revert**: delete `.claude/wpf-dev-pack.local.md` or remove its
  `language:` field. The hook will then emit nothing, and the plugin's
  default language behavior applies.

The file is personal and is covered by the repo's `.gitignore`
(`.claude/*.local.md`).

## .NET Version Configuration

### Version Selection Rules

1. **Minimum supported version**: **.NET 8** (C# 12)
2. **User specifies version** → Use that version with corresponding C# version
3. **No specification** → Use **latest stable .NET** (currently .NET 10)

### .NET ↔ C# Version Mapping

| .NET Version | C# Version | TargetFramework | Key Features |
|--------------|------------|-----------------|--------------|
| .NET 10 | C# 14 | `net10.0-windows` | Extensions, field keyword |
| .NET 9 | C# 13 | `net9.0-windows` | params collections, lock object |
| .NET 8 | C# 12 | `net8.0-windows` | Primary constructors, collection expressions |
| .NET 7 | C# 11 | `net7.0-windows` | Raw string literals, list patterns |
| .NET 6 | C# 10 | `net6.0-windows` | Global using, file-scoped namespace |
| .NET 5 | C# 9 | `net5.0-windows` | Records, init-only, top-level statements |
| .NET Core 3.1 | C# 8 | `netcoreapp3.1` | Nullable reference types, async streams |
| .NET Framework 4.8 | C# 7.3 | `net48` | Tuples, pattern matching, local functions |

> **Update Policy**: When new .NET version releases, add new row to this table.
> Last updated: 2026-01 (Latest stable: .NET 10)

### Code Generation Rules

When generating WPF projects or code:

```
IF user specifies ".NET X":
    Use netX.0-windows + C# version from mapping table
ELSE:
    Use latest stable .NET from mapping table (top row)
```

- Always use **maximum C# features** available for the target .NET version
- Use `Microsoft.Extensions.Hosting` matching the .NET major version
- Example: .NET 10 → `Microsoft.Extensions.Hosting` 10.x

---

## Core Rules

```
RULE 1: Detect WPF/C#/.NET keywords → Activate relevant skills
RULE 2: Delegate complex tasks to specialized agents
RULE 3: Announce skill activation (except silent triggers)
RULE 4: Select most specific skill when multiple match
RULE 5: wpf-architect MUST conduct Requirements Interview before analysis
```

---

## Requirements Interview System

When `wpf-architect` is invoked, conduct an **adaptive path-based interview** using AskUserQuestion:

| Path | Task Type | Steps | Focus |
|------|-----------|-------|-------|
| **A** | Create new project | 7 | concept → architecture → scale → complexity → libraries → feature areas |
| **B** | Analyze/improve | 5 | analysis goal → analysis mode → scope → output format |
| **C** | Implement feature | 5 | feature description → implementation approach → libraries → feature areas |
| **D** | Debug/fix | 4 | symptom → problem type → problem area |

**Keyword Analysis**: At free-input steps (A-2, B-2, C-2, D-2), detect keywords and auto-set defaults for subsequent steps.

See `agents/wpf-architect.md` for full interview specification.

## Trigger Priority

1. **Explicit slash command** (`/wpf-dev-pack:skill-name`) → Highest
2. **Keyword-based auto-trigger** → See `skills/.claude/CLAUDE.md`
3. **Context-based inference** → From conversation

## Trigger Behavior

**On Trigger:**
1. Announce: "WPF Dev Pack: Activating `skill-name` skill."
2. Check `.claude/rules/dotnet/wpf/mvvm-framework.md` for active MVVM framework
3. Load content:
   - **Knowledge topics** → call `WpfDevPackMcp get_wpf_topic(id[, variant])` to fetch from MCP
   - **Command skills** → invoked via slash command (`/wpf-dev-pack:<skill-name>`)
   - **CommunityToolkit.Mvvm command skills** → SKILL.md
   - **Prism 9 command skills** → PRISM.md if present, otherwise SKILL.md
4. Generate/modify code per guidelines and active framework rules

**Silent Triggers** (no announcement):
- `formatting-wpf-csharp-code`
- `using-xaml-property-element-syntax`
- `managing-literal-strings`

**Multiple Keywords:**
1. Most specific first (e.g., "drawingcontext" > "performance")
2. Related skills can be referenced in parallel
3. Ask user if conflict

---

## Adding a New Skill — Required Co-updates

**Adding a knowledge topic** (WPF knowledge, served via MCP — NOT a plugin skill):
1. Create `knowledge/<id>/TOPIC.md` (at the repo root, outside the plugin) with the topic content. **No YAML frontmatter.** The first `# H1` is the title; put a one-line `> summary` blockquote directly under the H1 — the MCP catalog (`TopicDocReader`) reads title from the first H1 and summary from the first `>` blockquote.
2. Add the topic's keyword(s) to `wpf-dev-pack/hooks/WpfKeywordDetector.cs` (the keyword→id routing table).
3. No plugin skill registration, no version bump, no MCP rebuild — the server picks it up on next `git pull`.

**Adding a command skill** (slash-invocable plugin skill under `skills/`):

When adding a new skill at `skills/<skill-name>/SKILL.md`, these files MUST be updated together:

1. **`skills/.claude/CLAUDE.md`** — add a row to the retained-command table.
2. **Adjacent existing SKILL.md files** — when topics overlap, add a cross-link to the new skill (`See [...](../skill-name/SKILL.md)`).
3. **Skills that need a Prism 9 branch** — author a `PRISM.md` companion file (see `mvvm-framework.md`).
4. **Foundation + Application skill pairs** — author the two skills separately and cross-reference. Foundation skill describes the mechanism / general principle; Application skill applies it to a specific scenario (e.g., `preventing-dispatcher-deadlock` + `shutting-down-wpf-gracefully`).
