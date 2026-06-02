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
| `flaui-cross-process-input-wpf-dev-pack-feedback.md` | Partially applied | 2026-06-01 | `TBD` | v1.6.6 (release pending) | Augmented `flaui-cross-process-input`: item 1 (DPI-aware computed-coordinate scaling — Problem 6 subsection + `GetEffectiveDpiScale` helper) and item 2 (new Problem 7: `Keyboard.Type` silent text-entry failure → `ValuePattern.SetValue`), and item 4 (Problem 2 subsection: `Mouse.MoveTo` interpolation is slow → `Mouse.Position` for instant non-drag reposition). Item 4's finding originated from a legacy date-prefixed draft (`2026-06-01-flaui-cross-process-dpi-wpf-devpack-feedback.md`); it was merged into this document as item 4 and the legacy draft was removed (anonymity-violating, never committed). Quick Reference (+3 rows) and Diagnostic Checklist (+2 items) updated; frontmatter description extended for triggering. Item 3 (FlaUI input API throwing access-denied / `COMException` → full P/Invoke `SendInput`/`keybd_event` + `SetForegroundWindow` + stuck-modifier release) skipped by maintainer this session. Version bump deferred to `/wpf-dev-pack-release` — target v1.6.6. |

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
