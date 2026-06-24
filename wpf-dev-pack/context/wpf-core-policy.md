# WPF Dev Pack — Core Policy

> Single source of truth for wpf-dev-pack's always-on policy. Injected at session
> start by the `CorePolicyLoader` SessionStart hook (the hook only delivers this
> file; edit the policy here, not in the hook). Plugin `CLAUDE.md` is not auto-loaded
> for installed users — always-on policy lives in `context/*.md`, and detailed rules
> ship as preloaded `wpf-rule-*` skills (injected into the relevant agents on spawn).

WPF knowledge is served on demand by the WpfDevPackMcp MCP server
(search-before-answer). Command skills are slash-invocable
(`/wpf-dev-pack:<skill-name>`). Complex tasks are delegated to specialized agents.
There is no keyword-detection hook.

---

## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read
this section:

1. **No ViewModelLocator** — use DI + DataTemplate mapping only (see prohibitions).
2. **No System.Windows in ViewModel** — BCL types only (enforced by the
   MvvmViolationDetector PostToolUse hook).
3. **Freeze all Freezable objects** — Brush, Pen, Geometry, Transform.
4. **Generic.xaml = MergedDictionaries hub only** — no inline styles/templates.
5. **Verify API signatures with HandMirror before writing code.**
6. **Single matching path per framework** — ViewModel First (CommunityToolkit.Mvvm,
   `Mappings.xaml`) or View First (Prism 9, `RegisterForNavigation`). Never mix.
7. **WPF knowledge topics are fetched via `WpfDevPackMcp get_wpf_topic(id)`** — not
   loaded from `skills/`.

Detailed elaboration of each rule lives in the bundled `wpf-rule-*` skills (preloaded
into the specialized agents on spawn) and in WpfDevPackMcp knowledge topics.

---

## MVVM Composition Style

wpf-dev-pack enforces a **single matching path per MVVM framework**, both based on
**Stateful ViewModel**. The composition direction differs by framework. For full
terminology and Microsoft references, see [`docs/TERMINOLOGY.md`](../docs/TERMINOLOGY.md).

| MVVM framework | Composition Direction | Mechanism | Wiring rules |
|---|---|---|---|
| CommunityToolkit.Mvvm (default) | **ViewModel First** | `Mappings.xaml` + implicit DataTemplate | `wpf-rule-view-viewmodel-wiring-communitytoolkit` skill |
| Prism 9 (alternative) | **View First** | `RegisterForNavigation` + `IRegionManager` | `wpf-rule-view-viewmodel-wiring-prism` skill |

> Pre-v1.6.4 docs uniformly labeled this as "View First MVVM". That single label
> was incorrect per Microsoft's official definition (the lookup key for
> `Mappings.xaml` is the ViewModel type → ViewModel First). v1.6.4 corrects the
> labels per path. The enforced code patterns are unchanged.

See the `wpf-rule-prohibitions` skill (P-001…P-004) for banned alternatives
(`ViewModelLocator`, code-behind `DataContext` assignment, Stateless VM pattern,
mixing the two paths, etc.).

---

## Core Rules

```
RULE 1: For WPF/C#/.NET questions → search WpfDevPackMcp topics before answering
        (search_wpf_topics → get_wpf_topic)
RULE 2: Delegate complex tasks to specialized agents
RULE 3: Announce command-skill activation
RULE 4: Select the most specific topic/skill when multiple match
RULE 5: wpf-architect MUST conduct Requirements Interview before analysis
```

The full Requirements Interview specification (adaptive Path A/B/C/D) is embedded in
`agents/wpf-architect.md`.

---

## Trigger Priority

1. **Explicit slash command** (`/wpf-dev-pack:skill-name`) → command skills.
2. **WPF knowledge** → search/fetch via WpfDevPackMcp (`search_wpf_topics` →
   `get_wpf_topic`).
3. **Context-based inference** → delegate to a specialized agent.

## Trigger Behavior

**On trigger:**
1. Announce: "WPF Dev Pack: Activating `skill-name` skill."
2. Check the active MVVM framework (see `mvvm-framework` knowledge / project
   references).
3. Load content:
   - **Knowledge topics** → call `WpfDevPackMcp get_wpf_topic(id[, variant])`.
   - **Command skills** → invoked via slash command (`/wpf-dev-pack:<skill-name>`).
   - **CommunityToolkit.Mvvm command skills** → SKILL.md.
   - **Prism 9 command skills** → PRISM.md if present, otherwise SKILL.md.
4. Generate/modify code per guidelines and active framework rules.

**Silent application** (no announcement):
- `formatting-wpf-csharp-code` — applied automatically by the `CodeFormatter`
  PostToolUse hook on `.cs` / `.xaml` edits.

**Multiple keywords:**
1. Most specific first (e.g., "drawingcontext" > "performance").
2. Related skills can be referenced in parallel.
3. Ask the user if there is a conflict.

---

## Agent Delegation

Delegate complex tasks to specialized agents. Agents are also user-invocable via the
`/agents` interface.

### Path-specific routing (Requirements Interview paths)

| Path | Selection | Primary Agent |
|------|-----------|---------------|
| B | Code quality review | `wpf-code-reviewer` |
| B | Performance analysis | `wpf-performance-optimizer` |
| B | Architecture diagnosis | `wpf-architect` (self) |
| B | Open-source code analysis | `wpf-architect` (self) |
| B | Codebase-wide audit | `wpf-code-auditor` |
| D | UI display problem | `wpf-xaml-designer` |
| D | Data problem | `wpf-data-binding-expert` |
| D | Performance problem | `wpf-performance-optimizer` |
| D | Crash / exception | `wpf-code-reviewer` |
| D | Build / configuration error | `wpf-code-reviewer` |

### Task-type mapping

| Task Type | Agent | Trigger |
|-----------|-------|---------|
| Architecture analysis | `wpf-architect` | "architecture", "best practice" |
| Code review | `wpf-code-reviewer` | "review", "MVVM violation" |
| Full-codebase audit | `wpf-code-auditor` | "audit", "codebase-wide consistency", pattern sweep |
| CustomControl development | `wpf-control-designer` | CustomControl creation |
| XAML styles/themes | `wpf-xaml-designer` | ControlTemplate, Style |
| MVVM implementation | `wpf-mvvm-expert` | ViewModel, Command |
| Data binding | `wpf-data-binding-expert` | Complex bindings |
| Performance optimization | `wpf-performance-optimizer` | Performance issues |
