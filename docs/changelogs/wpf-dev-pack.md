# wpf-dev-pack Changelog

All notable changes to **wpf-dev-pack** are documented in this file.
The format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to Semantic Versioning.

Version bumps for this plugin are produced by the `/wpf-dev-pack-release`
skill only. Do not edit `version` fields by hand — see the repository
`.claude/CLAUDE.md` "Plugin Version Update Checklist" section.

---

## v1.7.4

### Added
- **Streaming LLM chat skills** — `make-wpf-chatclient` (one-button full client)
  plus four component scaffolders: `make-wpf-chatclient-factory`,
  `make-wpf-chat-orchestrator`, `make-wpf-chat-bubble-template`,
  `make-wpf-markdown-presenter`.
- **MCP configuration skills** — `show-wpf-dev-pack-config` (prints the
  `config.json` / `state.json` paths + values and env overrides),
  `set-repo-branch` (sets the tracked branch in `config.json`, preserving
  repoPath), `set-repo-managed` (sets the server-managed flag in `state.json`
  that controls destructive `git reset --hard` vs non-destructive refresh).
- **`WpfAuthoringRulesLoader` SessionStart hook** — injects enforced WPF
  ControlTemplate / Style / animation authoring rules into every session
  (required `PART_` names, animation safety, Setter-on-Freezable → MC4111,
  `StaticResource` forward-reference, the `(UIElement.Children)[n]` path trap,
  enter/exit pairing, runtime verification). Plugin `.claude/rules` are not
  auto-loaded for installed users, so the rules ship as a hook.
- **Knowledge topics** (served on demand via WpfDevPackMcp):
  `animating-wpf-controltemplates`, `building-swappable-wpf-themes`,
  `setting-up-flaui-ui-tests`, `hosting-extensions-ai-chatclient-in-wpf-mvvm`,
  `sharing-httpclient-across-llm-sdks`, `consuming-mcp-tools-in-extensions-ai`,
  `rendering-markdown-in-wpf`, `displaying-selectable-rich-text-in-wpf`,
  `styling-chat-bubbles-in-wpf`, `storing-api-keys-and-binding-passwordbox-in-wpf`,
  `building-a-provider-settings-panel`.

### Changed
- `make-wpf-custom-control` cross-links the new `animating-wpf-controltemplates`
  topic; several existing topics gained cross-links to it.
- hooks `README.md` / `README.ko.md` and `skills/.claude/CLAUDE.md` routing table
  updated for the new hook and skills.

## v1.7.3

### Fixed
- **`make-wpf-custom-control` no longer emits an uncompilable override.** The
  generated control declared `protected override void OnIsEnabledChanged(...)`,
  which has no base method to override (CS0115); `IsEnabled` changes are now
  handled via the `IsEnabledChanged` event subscribed in the constructor.
- **`make-wpf-project` default scaffold now builds.** It referenced an undefined
  `IDialogService` (CS0246) and omitted the `$0.ViewModels` global using; the
  service example was removed (services come from `make-wpf-service`) and the
  missing using added. Verified end-to-end with `dotnet build` (CTK 8.4.* /
  Hosting 10.0.* / Behaviors.Wpf 1.1.* restore; custom control + Generic.xaml +
  ThemeInfo XAML compiles).

### Changed
- **`make-wpf-*` generators standardized into one consistent, buildable app.**
  `make-wpf-project` uses the `.WpfApp` suffix (matching the `make-wpf-viewmodel`
  / `make-wpf-service` project lookups) and scaffolds ViewModel-First navigation
  (`Mappings.xaml` + `ContentControl` host + `CurrentViewModel`);
  `make-wpf-service` places the interface in `.Core` when there is no
  `.Abstractions` (avoids a broken back-reference); `make-wpf-custom-control`
  auto-creates `Themes/Generic.xaml` + `ThemeInfo` when absent;
  `make-wpf-converter` is unified on the MarkupExtension pattern
  (`converter-patterns.md` aligned); `make-wpf-usercontrol` uses a design-time
  `d:DataContext` only (drops the prohibited inline `DataContext`, P-001-c).
- **Docs de-staled after the knowledge→MCP / keyword-router-removal refactor.**
  README, `.claude/CLAUDE.md`, `skills/.claude/CLAUDE.md` (and `.ko` mirrors)
  drop the obsolete keyword-router / auto-trigger framing; `skills/README` was
  rewritten (it had listed ~50 knowledge topics as skills); `hooks/README` now
  lists all 10 hooks with correct triggers; the agents README + delegation guide
  add the missing `wpf-code-auditor`; the orphan `sequential-thinking` MCP entry
  was removed; two hooks no longer print invalid
  `/wpf-dev-pack:<knowledge-topic>` slash commands.

### Stats
- Skills: 11 · Agents: 10 · MCP Servers: 2

---

## v1.7.2

### Removed
- **`WpfKeywordDetector` keyword-router hook removed** (~520 lines). WPF
  knowledge/skill triggering no longer flows through a UserPromptSubmit keyword
  router; it is now driven by the `WpfDevPackMcp` MCP server's own instructions
  (`search_wpf_topics` / `get_wpf_topic`). `hooks.json` no longer registers the
  detector (10 hooks remain).
- **`wpf-dev-pack/knowledge/` removed from the plugin package.** The knowledge
  topics now live at the repository root (`knowledge/<id>/TOPIC.md`) and are
  served on demand by `WpfDevPackMcp` from the repo clone, so they are no longer
  bundled into the installed plugin (~16k fewer lines shipped).

### Changed
- **`WpfDevPackMcp` is now a discoverable MCP server package on NuGet.** It is
  pinned in `.mcp.json` via `dnx`, and the server advertises usage instructions
  so agents reach for it without a keyword hook.

### Stats
- Skills: 11 · Agents: 10 · MCP Servers: 2

---

## v1.7.1

### Fixed
- **RepoPathGuard no longer fails open on cold start.** The PreToolUse hook that blocks `WpfDevPackMcp` tool calls when the knowledge repo path is unset used a 5s timeout; a cold `dotnet` file-based-app compile exceeded it, so the hook was killed and the call proceeded (PreToolUse hooks fail open on timeout) — after which the agent would auto-search for a clone and self-run `set-repo-path`. Bumped the hook `timeout` to 30s so the deny reliably lands (warm runs are ~750ms; only the first call per plugin version pays the cold-compile cost).

### Changed
- **"Repo not configured" messages now defer to the user.** Both the `RepoPathGuard` deny reason and the server-side `RepoNotConfiguredException` state that this is a one-time USER setup and instruct the agent NOT to search the filesystem or run `set-repo-path` itself. Defense in depth: the MCP server's code-level check backstops the hook if it ever fails open again.
- Re-pinned `WpfDevPackMcp` to `@0.1.2` in `.mcp.json` (and README examples); 0.1.2 ships the reworded server-side message.

### Stats
- Skills: 11 · Agents: 10 · MCP Servers: 2

---

## v1.7.0

### Changed
- **WPF knowledge skills migrated to the `WpfDevPackMcp` MCP server.** The ~50 knowledge topics (MVVM, rendering, threading, styling, 3rd-party libraries, .NET common, Prism 9 companions, testing) are no longer bundled under `skills/`. They are served on demand by `WpfDevPackMcp` (`get_wpf_topic` / `search_wpf_topics`) from a local clone of `christian289/dotnet-with-claudecode` (`wpf-dev-pack/knowledge/<id>/TOPIC.md`). This removes ~50 skill descriptions from the session skill listing (session-context savings) while keeping the content editable as plain Markdown — no MCP rebuild or plugin version bump is needed to update knowledge.
- `WpfKeywordDetector` hook now routes knowledge keywords to `WpfDevPackMcp get_wpf_topic(<id>)` and command keywords to `/wpf-dev-pack:<skill>`.
- `applying-wpf-dev-pack-feedback` now targets the knowledge base (`wpf-dev-pack/knowledge/<id>/TOPIC.md`) instead of a skill.
- `.mcp.json` now registers two servers: `HandMirrorMcp` and `WpfDevPackMcp` (pinned `@0.1.1`).
- `README.md` / `README.ko.md`: the "Skills by Category" breakdown is replaced by the 11 bundled command skills plus a knowledge-via-MCP note; stats updated.

### Added
- New command skill: `set-repo-path` — configures the local repo-clone path `WpfDevPackMcp` reads knowledge from (writes `~/.wpf-dev-pack-mcp/config.json`, or set `WPFDEVPACK_REPO_PATH`).
- New hook: `RepoPathGuard` (PreToolUse) — blocks `WpfDevPackMcp` tool calls when the repo path is unconfigured, with guidance on running `/wpf-dev-pack:set-repo-path`.

### Removed
- ~50 WPF knowledge `SKILL.md` files removed from `skills/` (relocated to `knowledge/<id>/TOPIC.md`; YAML frontmatter dropped — title = first `# H1`, summary = first `>` blockquote).

### Stats
- Skills: 11 · Agents: 10 · MCP Servers: 2

---

## v1.6.5

### Added
- New skill: `implementing-wpf-splash-screen` — STA-thread splash with a dedicated `Dispatcher`; strict Activate-before-Close ordering for the cross-thread foreground handoff to MainWindow (Win32 `SetForegroundWindow` grant rules); lock-free `Interlocked.CompareExchange` sentinel for the Show vs Close race; AXAML must not depend on `App.xaml` resources (cross-thread `Freezable`); idempotent `Close()`.
- New skill: `configuring-wpf-dev-pack-language` — writes `.claude/wpf-dev-pack.local.md` with a BCP-47 `language:` field consumed by the `LanguagePreferenceLoader` SessionStart hook.
- New hook: `LanguagePreferenceLoader` (SessionStart) — injects the per-project response-language directive into the session context.
- New hook: `DotnetVersionChecker` (SessionStart) — verifies .NET SDK 10.0.300+ and emits a high-visibility warning if missing or too old.
- New hook: `FeedbackDocAuditor` (PostToolUse, scoped to `collecting-wpf-dev-pack-feedback`) — pattern-based anonymity audit of newly written `*-wpf-dev-pack-feedback.md` files; exits with code 2 on violation.
- "Per-Project Language Preference" section added to `.claude/CLAUDE.md`.

### Changed
- `collecting-wpf-dev-pack-feedback` hardened: mandatory Anonymity policy; separate Sanitize (Step 3) and independent Audit (Step 5) passes; explicit no-date-prefix filename rule (output is exactly `<topic>-wpf-dev-pack-feedback.md`); document body language no longer constrained to Korean.
- `shutting-down-wpf-gracefully`: new "Background Callback Racing Dispatcher Shutdown" section (`HasShutdownStarted` guard + narrow catch on `InvalidOperationException` / `TaskCanceledException`).
- `managing-wpf-application-lifecycle`, `threading-wpf-dispatcher`: Related Skills cross-links to `implementing-wpf-splash-screen`.
- wpf-dev-pack `CLAUDE.md` files (plugin / skills / agents) translated to English; Korean mirror added under `.claude.ko/`.

### Moved
- Per-plugin changelog relocated from `wpf-dev-pack/CHANGELOG.md` to `docs/changelogs/wpf-dev-pack.md`.

### Stats
- Skills: 60 · Agents: 10 · MCP Servers: 1

---

## v1.6.4

### Added
- New skill: `containing-control-decorative-overflow` (focus-ring / hover-glow clipping diagnosis).
- New skill: `scottplot-axes-margins-destructive` — documents that ScottPlot 5's `plot.Axes.Margins(double, double)` replaces the `AutoScaler` instance and immediately calls `AutoScale()`, wiping prior state such as `InvertedY`. Provides the in-place `FractionalAutoScaler.SetMarginsX/Y` fix for WPF reactive hot paths (IValueConverter, PropertyChanged, slider drag) where the same setup runs many times.
- `scottplot-syncing-modifier-keys-for-mousewheel` §6 Related Skills cross-link to the new skill (shared theme: ScottPlot 5 cookbook-once APIs vs WPF reactive re-entry).
- `docs/TERMINOLOGY.md` and `docs/TERMINOLOGY.ko.md` — single source of truth for MVVM composition terms.
- `CHANGELOG.md` — this file.
- `Required MCPs` section in `README.md`, `README.ko.md`, and `.claude/CLAUDE.md` (Serena listed as direct-MCP install via `uv`, not as a Claude Code plugin).
- `.github/CONTRIBUTING.ko.md` (Korean counterpart); FeedbackDocs contribution flow and squash-before-PR notes added to `.github/CONTRIBUTING.md`.
- `authoring-wpf-controls` §3.4 Visual State Naming Contract; §4 Multi-Constraint Coerce Ordering.
- `managing-wpf-popup-focus` §5.8 SelectionChanged-driven close pattern; §5.9 acrylic vs solid surface brush.
- `managing-wpf-popup-focus`, `managing-literal-strings`, `optimizing-wpf-memory`: cross-links and new subsections per the FeedbackDocs upgrade design.

### Changed
- `make-wpf-custom-control` scaffold modernized: Step numbering fixed; nested `VisualStates` / `TemplateParts` const classes; `Pressed` state reachable; coerce example with multi-constraint ordering; overlay-layer opacity animation (avoids the `(Background).(SolidColorBrush.Color)` trap with shared / frozen / DynamicResource brushes); read-only DP and RoutedEvent stubs.
- `optimizing-wpf-memory`: added "native resources already copied during conversion" subsection (`Mat` → `BitmapSource` pattern; do not retain the source as a field).
- `collecting-wpf-dev-pack-feedback`: `disable-model-invocation` frontmatter removed.
- `.claude/rules/prohibitions.md`: expanded from one rule to P-001…P-004 with Quick Reference table.
- `.claude/CLAUDE.md`: "MVVM Approach: View First" replaced by a per-framework table (CommunityToolkit / Prism); Essential rule #6 reworded.
- `.claude/rules/view-viewmodel-wiring-communitytoolkit.md` / `view-viewmodel-wiring-prism.md`: AI-agent anchor blocks added.
- `README.md` / `README.ko.md`: MVVM Composition Style section corrected; Serena moved from Required Claude Code plugins to Required MCPs.
- `.claude-plugin/plugin.json`: description updated to reflect per-framework MVVM composition.
- `skills/.claude/CLAUDE.md`: Keyword Mapping and Category Index entries for the new skill.

### Fixed
- "View First MVVM" label corrected across docs to per-path classification, matching Microsoft's official definitions:
  - CommunityToolkit.Mvvm path → ViewModel First Composition (lookup key is the ViewModel type).
  - Prism 9 path → View First Composition (lookup key is the View name string).
- Serena MCP setup clarified: it is incompatible with the Claude Code plugin path due to built-in tool-description bias; install directly via `uv`. References: [Serena Claude Code Attention note](https://oraios.github.io/serena/02-usage/030_clients.html#claude-code), [Quick Start](https://github.com/oraios/serena#quick-start).

### Moved
- `2026-05-18-customcontrol-authoring-upgrade-design.md` from `wpf-dev-pack/docs/superpowers/specs/` to repo-root `FeedbackDocs/`.
