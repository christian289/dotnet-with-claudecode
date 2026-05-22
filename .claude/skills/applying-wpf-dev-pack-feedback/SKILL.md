---
description: "Applies one wpf-dev-pack feedback document to the plugin. The maintainer points it at a feedback markdown file (typically delivered via a contributor PR); the skill walks through each item, makes the proposed changes, moves the document into FeedbackDocs/, and appends a row to FeedbackDocs/APPLIED-LOG.md. Use only as a maintainer of the dotnet-with-claudecode repository when reflecting a contributor's feedback md."
argument-hint: "<feedback-md-path>"
disable-model-invocation: true
hooks:
  PreToolUse:
    - matcher: Write
      hooks:
        - type: command
          command: dotnet "${CLAUDE_PROJECT_DIR}/.claude/skills/applying-wpf-dev-pack-feedback/hooks/MicrosoftSkillCreatorReminder.cs"
          timeout: 5
---

# Applying wpf-dev-pack Feedback

This skill turns one feedback markdown file (produced by the
`/wpf-dev-pack:collecting-wpf-dev-pack-feedback` skill in a foreign WPF
session) into concrete changes inside the `wpf-dev-pack` plugin, then
records the application in `FeedbackDocs/APPLIED-LOG.md` and parks the
source document inside `FeedbackDocs/`.

It is the *maintainer counterpart* of the collecting skill.

## When NOT to use this

- The feedback document has not been read manually yet — read it first.
- The working tree is not clean — apply the feedback against a known
  baseline so the resulting commit is reviewable.
- `$0` was not provided — the skill needs exactly one argument: the path
  to the feedback markdown file (absolute, or relative to repo root).
- More than one feedback document at a time — invoke the skill per file.
  Batch application is intentionally not supported.

## Workflow

Copy this checklist and track progress:

```
Apply Progress:
- [ ] Step 1: Validate input and load the feedback md
- [ ] Step 2: Plan: review the summary, decide which items to apply
- [ ] Step 3: Apply each selected item (one at a time)
- [ ] Step 4: Version bump + README sync (only if needed)
- [ ] Step 5: Move the md into FeedbackDocs/
- [ ] Step 6: Append a row to FeedbackDocs/APPLIED-LOG.md
- [ ] Step 7: Surface a commit message draft for the maintainer
```

### Step 1 — Validate input and load

Treat `$0` as the path to the feedback markdown file. Reject early if:

- `$0` is empty or missing.
- The file does not exist at the given path.
- The file name does not end with `-wpf-dev-pack-feedback.md`. Legacy
  `-wpf-devpack-feedback.md` is also accepted, for backwards
  compatibility with already-archived contributions.

Read the file. Identify:

- The H1 title line (one-line summary of the feedback).
- The `## 0. Summary` table (priority overview).
- Each `## N.` item with its `Phenomenon and causality`, `Proposal`,
  and `Adjacent skill boundaries / cross-links` subsections.

Confirm the file's current location:

- If the file already lives inside `FeedbackDocs/`, note it — Step 5
  will be a no-op move.
- Otherwise, remember the source path so Step 5 can move it after the
  apply completes.

### Step 2 — Plan

Show the maintainer:

- The H1 title (one-line summary).
- The `## 0. Summary` table verbatim.

Then use AskUserQuestion (multi-select) to ask which numbered items to
apply this session. If there are more than 4 items, page through them in
groups of up to 4 options each. Include an `All` option for short lists.

For each item the maintainer skips, capture the reason in scratch
state — it will become part of the APPLIED-LOG row (Status =
`Partially applied`, with the reason in Notes).

### Step 3 — Apply each selected item

For each selected item, treat the item's `Proposal (concrete change)`
section as the spec. Typical change kinds and where they land:

| Change kind | Where to apply |
|------|-------|
| New skill | `wpf-dev-pack/skills/<name>/SKILL.md` + update `wpf-dev-pack/skills/.claude/CLAUDE.md` (Keyword-Skill Mapping + Skill Category Index) + cross-link from related skills |
| Skill augmentation | Edit the existing `SKILL.md` to add the missing guidance |
| Scaffolder modernization | Update the relevant `make-wpf-*` skill template |
| Rule addition | Add to `wpf-dev-pack/.claude/rules/<rule>.md` |
| Prism 9 companion | Add `PRISM.md` next to the affected `SKILL.md` (see `wpf-dev-pack/.claude/rules/dotnet/wpf/mvvm-framework.md`) |

Apply items one at a time:

1. Restate the proposal in your own words and the concrete files you
   intend to touch. Wait for the maintainer to confirm before editing.
2. Make the change.
3. If the change touches `*.xaml` or `*.cs`, the existing plugin-level
   hooks (`XamlValidator`, `MvvmViolationDetector`, `CodeFormatter`)
   will fire automatically. Surface any diagnostic they produce.
4. Move on to the next item only after the current one is fully done.

If applying an item requires a decision the feedback does not specify,
ask the maintainer. Do not guess.

#### Hook reminder: prefer microsoft-skill-creator when scaffolding a new skill

This skill ships with a skill-scoped PreToolUse hook
(`hooks/MicrosoftSkillCreatorReminder.cs`) that fires automatically
when you are about to write a NEW `SKILL.md` inside `wpf-dev-pack/skills/`.
On fire, the hook reminds you to:

1. Check whether `microsoft-docs:microsoft-skill-creator` is in the
   available-skills list, AND whether the Microsoft Learn MCP server
   (`microsoft-learn` / `plugin_microsoft-docs_microsoft-learn`) is
   registered.
2. If both are present and the new skill concerns a Microsoft / .NET /
   Azure / WPF / VS Code / Bicep / similar topic, **prefer scaffolding
   via `/microsoft-docs:microsoft-skill-creator`** rather than writing
   the skill from scratch. The skill-creator generates a hybrid skill
   grounded in official Microsoft docs, which significantly reduces
   hallucinated API signatures.
3. If either is missing, recommend installing the microsoft-docs
   plugin / registering the MCP before authoring.

The hook cannot probe runtime MCP / skill availability — that visibility
belongs to you. The hook is a reminder; do the check before proceeding.

### Step 4 — Version bump + README sync (only if needed)

Per `.claude/CLAUDE.md` (Plugin Version Update Checklist), keep these
in lockstep when cutting a release:

- `wpf-dev-pack/.claude-plugin/plugin.json` — `version`
- `wpf-dev-pack/README.md` — version badge
- `wpf-dev-pack/README.ko.md` — version badge

Ask the maintainer whether this batch of changes constitutes a release.

- **Yes**: ask for the target version (e.g., `1.6.5`), update all three
  files, and remember the version for Step 6.
- **No** (docs-only, trivial fix, or part of a larger pending release):
  skip this step entirely. The Step 6 `Plugin version` column will
  carry the *current* version suffixed with `(docs only)` or
  `(no version bump)` as appropriate.

If the feedback adds a new skill or changes existing skill behavior,
the default answer is **Yes** — surface that nuance to the maintainer.

### Step 5 — Move the md into FeedbackDocs/

Move the feedback file into `FeedbackDocs/`, preserving its file name.
Use the platform's native move:

- Windows (PowerShell): `Move-Item <src> FeedbackDocs/`
- Unix-like: `mv <src> FeedbackDocs/`

If the file already lives inside `FeedbackDocs/`, this step is a no-op
— skip it.

If the destination already has a file with the same name (rare, but
possible when re-applying a previously rejected feedback), stop and ask
the maintainer how to resolve. Do not overwrite silently.

### Step 6 — Append a row to FeedbackDocs/APPLIED-LOG.md

Open `FeedbackDocs/APPLIED-LOG.md`, find the "Reflection log" table,
and append one row at the bottom.

| Column | How to fill |
|---|---|
| **File** | File name only (no path), e.g. `<topic>-wpf-dev-pack-feedback.md` — backticked. |
| **Status** | `Applied` if every selected item was reflected; `Partially applied` if any item was skipped; `Rejected` if all items were declined. |
| **Date reflected** | Today's date (`YYYY-MM-DD`) per the conversation's current date. |
| **Reflected in (commit / PR)** | Leave as `TBD`. The maintainer fills in the commit hash after committing. The skill does NOT auto-commit. |
| **Plugin version** | The version the change ships in (e.g., `v1.6.5`). If Step 4 was skipped, write the unchanged version + `(docs only)` or `(no version bump)`. |
| **Notes** | Item-level deviations, partial-application reasons, cross-links to follow-up issues, file-rename caveats, etc. |

If `FeedbackDocs/APPLIED-LOG.md` does not exist (it should — the repo
ships with it), stop and report this to the maintainer rather than
recreating it from scratch; recreation risks losing prior rows.

### Step 7 — Surface a commit message draft

Do NOT run `git add`, `git commit`, or `git push`. Print a suggested
commit message that the maintainer can review and apply, following the
conventions in `.github/CONTRIBUTING.md`:

```
<type>(wpf-dev-pack): <one-line summary derived from the feedback H1>

- Reflects FeedbackDocs/<filename>
- Applied items: <ids>
- Skipped items: <ids> (<reason>)         # omit line if none
- Version: <unchanged | vX.Y.Z>
```

`<type>` selection:

- `feat:` — new skill or behavior addition.
- `fix:` — correction to existing behavior.
- `docs:` — docs-only change (rules, README, CONTRIBUTING).
- `refactor:` — internal reorganization with no behavior change.
- `chore:` — other (build, tooling).

After printing, remind the maintainer:

1. Review the diff.
2. Commit (squash if necessary — per `.github/CONTRIBUTING.md` the repo
   prefers a small number of meaningful commits over many step-by-step
   commits).
3. Update the `Reflected in (commit / PR)` column of the APPLIED-LOG
   row with the resulting commit hash before pushing.

## Notes

- This skill is NOT auto-triggered. It must be invoked explicitly with
  one md file at a time.
- The skill does not run `git add`, `git commit`, or `git push`. The
  filesystem changes (move + APPLIED-LOG update + plugin code) are
  presented to the maintainer as a single reviewable diff.
- If you stop mid-workflow, leave the APPLIED-LOG row OUT. A row in the
  log should mean "this is reflected and about to be committed"; an
  orphan row is confusing.
- Backwards compatibility: the legacy filename suffix
  `-wpf-devpack-feedback.md` (without the second hyphen) is accepted at
  Step 1 so already-archived contributions can be re-processed if
  needed. New contributions should use `-wpf-dev-pack-feedback.md`.
