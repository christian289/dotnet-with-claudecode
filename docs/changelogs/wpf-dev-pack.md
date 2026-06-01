# wpf-dev-pack Changelog

All notable changes to **wpf-dev-pack** are documented in this file.
The format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to Semantic Versioning.

Version bumps for this plugin are produced by the `/wpf-dev-pack-release`
skill only. Do not edit `version` fields by hand — see the repository
`.claude/CLAUDE.md` "Plugin Version Update Checklist" section.

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
