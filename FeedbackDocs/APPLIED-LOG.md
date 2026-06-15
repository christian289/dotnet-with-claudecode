# FeedbackDocs Applied Log

This file is the canonical record of which feedback documents in this
directory have been reflected into the `wpf-dev-pack` plugin. Maintainers
append a row when a document is acted on (Applied / Partially applied /
Rejected / Superseded).

> **Name disambiguation:** `wpf-dev-pack/CHANGELOG.md` already exists and
> tracks the plugin's release notes. This file is a separate, narrower
> ledger that tracks one specific input flow — feedback documents in
> `FeedbackDocs/` — so the names are kept distinct on purpose.

## Why this exists

Feedback documents accumulate as a corpus. Without an explicit log it is
hard to tell which ones have already been integrated and which are still
open. Equally important: once a feedback document has shipped into the
plugin, its content should not be silently rewritten.

The `collecting-wpf-dev-pack-feedback` skill's PostToolUse hook is wired
to both `Write` and `Edit` of `*-wpf-dev-pack-feedback.md`. The Edit
trigger exists so the audit loop in Step 5 can fix violations during
authoring. But the same capability means an unknown later editor could
silently alter an already-reflected feedback document. This log freezes
the recorded state of every applied document, so any later modification
is visible as a divergence between the document and the row that
describes it (and the commit it cites).

## When to update

Use the `/applying-wpf-dev-pack-feedback` skill — it appends the row
automatically as part of the reflection workflow. Manual edits are
allowed for corrections (e.g., filling in the commit hash after a
commit), but the row layout below must be preserved.

The `collecting-wpf-dev-pack-feedback` skill writes new feedback
documents to the contributor's working directory, not to this folder.
Documents arrive here only via pull request, after the contributor has
self-reviewed them per `.github/CONTRIBUTING.md`.

## Status values

| Status | Meaning |
|--------|---------|
| `Pending` | In the corpus, not yet reviewed. |
| `Applied` | Fully reflected into the plugin. |
| `Partially applied` | Some items reflected; others deferred or dropped (see Notes). |
| `Rejected` | Reviewed and intentionally not applied (see Notes for reason). |
| `Superseded` | Replaced by a later feedback document (link in Notes). |

## Reflection log

Rows are ordered by `Date reflected`, oldest first. A new row is appended
at the bottom.

| File | Status | Date reflected | Reflected in (commit / PR) | Plugin version | Notes |
|------|--------|----------------|-----------------------------|----------------|-------|
| `customcontrol-authoring-upgrade-design.md` | Applied | 2026-05-20 | `51cfd77` | v1.6.4 | Design spec for CustomControl authoring upgrades U1-U6. Applied alongside unblocking the `collecting-wpf-dev-pack-feedback` skill. Originally archived as `2026-05-18-customcontrol-authoring-upgrade-design.md`; renamed 2026-05-27 to strip the date prefix per the updated collecting-skill naming rule. |
| `scottplot-margins-destructive-wpf-devpack-feedback.md` | Applied | 2026-05-21 | `104f56d` | v1.6.4 | Resulted in the `scottplot-axes-margins-destructive` skill (v1.6.4 consolidation release). Filename uses the pre-correction `wpf-devpack` spelling; the convention going forward is `wpf-dev-pack-feedback.md`. Originally archived as `2026-05-20-scottplot-margins-destructive-wpf-devpack-feedback.md`; renamed 2026-05-27 to strip the date prefix. |
| `wpf-splash-screen-foreground-handoff-wpf-devpack-feedback.md` | Applied | 2026-05-27 | `261e42f` | v1.6.4 (release pending) | New skill `implementing-wpf-splash-screen` (STA-thread splash + cross-thread foreground handoff via `MainWindow.Activate()` → `splash.Close()`, lock-free `Interlocked.CompareExchange` sentinel for Show↔Close race). `shutting-down-wpf-gracefully` augmented with §8 "Background Callback Racing Dispatcher Shutdown" (HasShutdownStarted guard + narrow catch on `InvalidOperationException` / `TaskCanceledException`). Cross-links added in `managing-wpf-application-lifecycle` and `threading-wpf-dispatcher`. Skill scaffolded via `/microsoft-docs:microsoft-skill-creator` with feedback-specific constraints. Version bump deferred to `/wpf-dev-pack-release` — maintainer should ship as v1.6.5. Filename uses the legacy `wpf-devpack` spelling. Saved without a date prefix per the updated collecting-skill naming rule. |
| `flaui-cross-process-input-wpf-dev-pack-feedback.md` | Partially applied | 2026-06-01 | `c55f85f` | v1.6.6 (release pending) | Augmented `flaui-cross-process-input`: item 1 (DPI-aware computed-coordinate scaling — Problem 6 subsection + `GetEffectiveDpiScale` helper) and item 2 (new Problem 7: `Keyboard.Type` silent text-entry failure → `ValuePattern.SetValue`), and item 4 (Problem 2 subsection: `Mouse.MoveTo` interpolation is slow → `Mouse.Position` for instant non-drag reposition). Item 4's finding originated from a legacy date-prefixed draft (`2026-06-01-flaui-cross-process-dpi-wpf-devpack-feedback.md`); it was merged into this document as item 4 and the legacy draft was removed (anonymity-violating, never committed). Quick Reference (+3 rows) and Diagnostic Checklist (+2 items) updated; frontmatter description extended for triggering. Item 3 (FlaUI input API throwing access-denied / `COMException` → full P/Invoke `SendInput`/`keybd_event` + `SetForegroundWindow` + stuck-modifier release) skipped by maintainer this session. Version bump deferred to `/wpf-dev-pack-release` — target v1.6.6. |
| `flaui-capture-resize-robustness-wpf-dev-pack-feedback.md` | Applied | 2026-06-08 | `TBD` | v1.7.1 (new knowledge topic; keyword co-update, no version bump) | New knowledge topic `flaui-capture-resize-robustness`: anchor capture/manipulation on UIA identifiers (`ByName`/`ByControlType`/`ByClassName`/`ByAutomationId`) + the element's runtime `BoundingRectangle` instead of absolute screen coordinates / fixed crop rectangles; `Capture.Element` follows live bounds on resize; partial-region capture computed from live bounds (`Capture.ElementRectangle` / `Capture.Rectangle`); "resize → re-run suite → passes without code change, captures regenerate" regression expectation. Keyword routing added in `WpfKeywordDetector.cs`: appended to the `flaui` array and the `ui test`/`automation test`/`ui 테스트`/`자동화 테스트` intent arrays, plus new keys `capture.element`/`captureimage`/`elementrectangle`/`창 리사이즈 캡처`. Cross-linked both ways with `flaui-wpf-element-discovery` and `flaui-cross-process-input`, keeping the boundary distinct per the feedback (capture/anchoring vs input-injection/DPI). FlaUI.Core v5.0.0 `Capture` API verified via HandMirror. Feedback's placeholder target `skills/<flaui-ui-test-capture>/SKILL.md` did not exist (FlaUI guidance lives as MCP knowledge topics), so applied as a new topic. No version bump per CLAUDE.md "Adding a knowledge topic" procedure — keyword co-update rides to the next release. |
| `msix-windows-app-sdk-wpf-dev-pack-feedback.md` | Applied | 2026-06-08 | `TBD` | (knowledge only, no plugin/MCP bump) | All 4 items applied as knowledge augmentations (no new topic, no `WpfKeywordDetector.cs` change). `publishing-wpf-apps/TOPIC.md`: new "Windows App SDK (self-contained) + MSIX gotchas" section — item 1 (default Content globbing breaks `pack://` image/icon → `XamlParseException`/`MSB3030`; fix via `<Content Remove>` + `<Resource>`/`<ApplicationIcon>`), item 2 (self-contained WASDK + custom-manifest MSIX multi-exe ambiguity → `--executable`; framework-dependent comparison), item 4 (packaged apps don't search PATH for native DLLs → `DllNotFoundException` 0x8007007E; `SetDefaultDllDirectories`+`AddDllDirectory` over all PATH dirs, packaged-gated). `managing-wpf-application-lifecycle/TOPIC.md`: new "§6 Programmatic Restart (packaged vs unpackaged)" — item 3 (`CoreApplication.RequestRestartAsync` deadlocks/no-relaunch in FullTrust WPF → synchronous `AppInstance.Restart` + inspect `AppRestartFailureReason`; unpackaged uses `Process.Start(Environment.ProcessPath)`+`Shutdown` with null guard; never `Process.Start` a packaged exe; `async void` try-catch / `AsyncDelegateCommand` note); Related Skills/References renumbered §6→§7, §7→§8. Both `>` summaries enriched with trigger terms. Placement = `TOPIC.md` (not sub-files) because `get_wpf_topic` serves only default/prism/advanced and `search_wpf_topics` indexes only `TOPIC.md` body. `AppInstance.Restart` signature + `AppRestartFailureReason` enum verified via Microsoft Learn MCP. Knowledge-only → no bump (feedback's own "version bump + README sync needed" note is outdated per CLAUDE.md). Applied on top of the still-uncommitted flaui apply (maintainer chose to proceed); only shared file is `APPLIED-LOG.md`. |
| `wpf-llm-chat-ui-wpf-dev-pack-feedback.md` | Applied | 2026-06-11 | `TBD` | v1.7.3 (knowledge topics: no bump; 5 new command skills: release pending `/wpf-dev-pack-release`) | Streaming LLM ChatClient surface (20 items) consolidated into 8 new knowledge topics + 1 augmentation + 5 new command skills. Topics (clusters A–D, all selected): `rendering-markdown-in-wpf` (items 1,3,14,18), `displaying-selectable-rich-text-in-wpf` (2,4,5,20), `styling-chat-bubbles-in-wpf` (6), `hosting-extensions-ai-chatclient-in-wpf-mvvm` (7,12,15), `sharing-httpclient-across-llm-sdks` (8,11), `consuming-mcp-tools-in-extensions-ai` (10), `storing-api-keys-and-binding-passwordbox-in-wpf` (13), `building-a-provider-settings-panel` (19); augmented existing `virtualizing-wpf-ui` with a variable-height selectable-bubble caveat (17). New command skills: `make-wpf-markdown-presenter`, `make-wpf-chat-bubble-template`, `make-wpf-chatclient-factory`, `make-wpf-chat-orchestrator`, and the one-button self-contained `make-wpf-chatclient` (composes the four + emits EnterCommandBehavior/StickToBottomBehavior/ChatView/ChatViewModel/DI; defaults to keyless `Provider.Mock`). Maintainer decision: items 9 (Enter-to-send) & 16 (pin-to-bottom) were NOT made separate topics/skills — folded into `make-wpf-chatclient` and cross-linked from `make-wpf-behavior`. All scaffolders carry a "verify SDK signatures with HandMirror before emitting" note (Microsoft.Extensions.AI / ModelContextProtocol / OpenAI / OllamaSharp / Anthropic.SDK / Google.GenAI). Co-updates: `skills/.claude/CLAUDE.md` routing (+2 rows), `make-wpf-behavior` cross-link. New topics link via `../<id>/TOPIC.md`; pre-existing topics still use stale `../<id>/SKILL.md` links (pre-rename) — left as-is, flagged for a future sweep. Nothing skipped. Knowledge topics need no bump (served live by WpfDevPackMcp); the 5 plugin skills need a release — run `/wpf-dev-pack-release` separately. Separately (NOT part of this reflection): the `/applying-wpf-dev-pack-feedback` skill itself was edited to remove its version-bump Step 4 per maintainer directive (the skill must never touch the version field). |

## Verifying integrity

To check whether an `Applied` document has been modified after the row
was added:

1. Look up the commit cited in the row (e.g., `104f56d`).
2. Compare `git show <commit>:FeedbackDocs/<file>` with the current
   working-tree contents of the same file.
3. Any non-trivial divergence is suspicious. Investigate via `git log`
   and `git blame` on the document.

This is a manual check; no hook enforces it. The Applied Log provides
the reference state needed to do the check at all.
