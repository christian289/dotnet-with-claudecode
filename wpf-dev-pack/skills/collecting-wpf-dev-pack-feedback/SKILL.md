---
description: "Analyzes the current WPF working session and produces an accumulable wpf-dev-pack improvement-feedback markdown file in the current directory. Use when, after doing WPF work with wpf-dev-pack skills/scaffolders in a project OUTSIDE the dotnet-with-claudecode repo, you want to capture anti-patterns you hand-fixed, missing or outdated skill guidance, mistaken or missed triggers, and scaffolder gaps — without editing wpf-dev-pack directly. The output md is meant to be contributed to dotnet-with-claudecode/FeedbackDocs via a pull request."
argument-hint: "[topic]"
hooks:
  PostToolUse:
    - matcher: Write
      hooks:
        - type: command
          if: "Write(*-wpf-dev-pack-feedback.md)"
          command: dotnet "${CLAUDE_PLUGIN_ROOT}/hooks/FeedbackDocAuditor.cs"
          timeout: 10
    - matcher: Edit
      hooks:
        - type: command
          if: "Edit(*-wpf-dev-pack-feedback.md)"
          command: dotnet "${CLAUDE_PLUGIN_ROOT}/hooks/FeedbackDocAuditor.cs"
          timeout: 10
---

# Collecting wpf-dev-pack Feedback

This skill turns lessons learned in the current WPF session into a structured
improvement-feedback document for the `wpf-dev-pack` plugin. It does NOT modify
`wpf-dev-pack` and does NOT touch git — it only writes one markdown file to the
current working directory, which the user later contributes via pull request.

## Why this exists

WPF work happens in projects OUTSIDE the `dotnet-with-claudecode` repository, and
that repo's local clone path differs per machine. Editing `wpf-dev-pack` from a
foreign session is infeasible, so improvements are captured as an accumulable md
and contributed separately (see the repo CONTRIBUTING guide).

## Anonymity policy (MANDATORY)

The feedback document is a reusable artifact contributed across projects. It
must describe the technical phenomenon and its causal chain only. The
following content is PROHIBITED in any feedback document produced by this
skill:

- Project, solution, repository, product, or codename
- Team, developer, or user name; email; account handle
- Date or time of when the issue was encountered
- Absolute or repo-relative file paths from the originating codebase
- Class, namespace, or member names that are unique to the originating project
  (use generic placeholders such as `XxxView`, `XxxViewModel`, `XxxService`)
- Any other detail that can identify the originating project or person

Public framework / library / API names (e.g., `HelixToolkit`, `ScottPlot`,
`CommunityToolkit.Mvvm`, `Prism`, `DispatcherPriority.ApplicationIdle`) are
allowed because they are part of the technical context, not identifying
information.

## Workflow

Copy this checklist and track progress:

```
Feedback Collection Progress:
- [ ] Step 1: Auto-draft feedback items from this session
- [ ] Step 2: Augment via interview (AskUserQuestion)
- [ ] Step 3: Sanitize and rewrite each item as a causal description
- [ ] Step 4: Write the md to the current directory
- [ ] Step 5: Audit the completed md for identifying information
- [ ] Step 6: Report path and contribution instructions
```

### Step 1 — Auto-draft

Review THIS conversation/session and extract feedback candidates. Look for:

- wpf-dev-pack skills that were used, and where their guidance was inaccurate or insufficient
- Anti-patterns the user/you hand-fixed that are not yet encoded in any skill
- Skill or rule content that is missing or outdated
- Keywords that should have triggered a skill (or triggered the wrong one)
- `make-wpf-*` scaffolder output that was missing or wrong

For each candidate, capture concrete session evidence (what was attempted, what
broke, how it was fixed) and a concrete proposal (which file to add/change, what
kind: new skill / augment skill / scaffolder modernization / rule addition) and a
priority (High / Medium / Low).

If no candidate is found, tell the user there is nothing to report and STOP
without writing a file.

### Step 2 — Interview augmentation

Use the AskUserQuestion tool at least once (one or two questions max) to surface
items the auto-draft missed. Suggested questions:

- "Which skill triggered but gave wrong/incomplete guidance?"
- "Which pattern did you hand-write that should be scaffolded or documented?"

Fold the answers into the draft.

### Step 3 — Sanitize and rewrite as a causal description

Before writing the document, for every drafted item, rewrite the evidence so
that it reads as a generic technical phenomenon, not a project incident.

Apply these transformations:

| If the draft contains | Replace with |
|---|---|
| A project / solution / repo name | Omit entirely; refer to "a WPF project" or remove the sentence |
| A developer / team name | Omit entirely |
| A date / time / sprint reference | Omit entirely |
| An absolute or repo-relative path from the originating codebase | A generic shape such as `Views/XxxView.xaml`, `ViewModels/XxxViewModel.cs` |
| A project-specific class / namespace / member name | A neutral placeholder such as `XxxView`, `XxxViewModel`, `IXxxService` |
| "We tried X in project Y" | "When X is done with this API" |
| "It broke in our build" | "The control/binding/render path fails because …" |

Each item's evidence section must then describe:

1. **Phenomenon** — what is observed (the symptom)
2. **Cause** — the framework / library / API behavior that produces it
3. **Effect** — the resulting failure mode or wrong output

Keep the chain technical and reproducible by any reader, regardless of which
project they are working on.

### Step 4 — Write the document

Resolve a short kebab-case `<topic>` from the `$0` argument if provided, else
infer it from the session's main subject.

Write the file to `<cwd>/<topic>-wpf-dev-pack-feedback.md` (current working
directory root — do NOT search for or write into any other repo, and do NOT
run git). If a file with the same name already exists, append a short
disambiguating suffix (e.g., `-2`) rather than overwriting.

**Do NOT prefix the filename with a date** (no `YYYY-MM-DD-…`). The Anonymity
policy in this skill forbids dates inside the document body, and the same
rule applies to the filename — date prefixes leak when the issue was
encountered and defeat anonymity. The filename is exactly
`<topic>-wpf-dev-pack-feedback.md`, nothing prepended.

The document body language is not constrained — use whichever language the
session naturally produced (Korean, English, or mixed is all fine). Use this
exact structure:

```markdown
# wpf-dev-pack Feedback — <one-line title>

- **Purpose**: <why this feedback is needed>
- **Scope**: <count of new skills / augmentations / scaffolder modernizations; version bump and README sync notes>

---

## 0. Summary (priority)

| ID | Kind | Target | Priority | One-liner |
|----|------|--------|----------|-----------|
| 1  | New skill / Augment / Modernize / Rule | skills/<...>/SKILL.md | High/Medium/Low | ... |

---

## 1. <item title>

### Phenomenon and causality
<Phenomenon: what is observed.
Cause: which framework / library / API behavior produces it.
Effect: the resulting failure mode.
No project name, no developer name, no date, no originating file path,
no project-specific identifier.>

### Proposal (concrete change)
<Which file to add or modify, and what to change>

### Adjacent skill boundaries / cross-links
<Boundary with overlapping skills, and cross-link targets>
```

Add one numbered `## N.` section per feedback item.

### Step 5 — Audit the completed md

After the file is written, treat it as an untrusted artifact and run an
independent audit against the Anonymity policy. The drafting and the audit
must be separate passes — do NOT skip this step even when you are confident
the draft is clean.

A skill-scoped PostToolUse hook (declared in this skill's frontmatter,
implemented at `hooks/FeedbackDocAuditor.cs`) performs a pattern-based
first-pass audit on every Write/Edit of a `*-wpf-dev-pack-feedback.md` file
*while this skill is active*. The hook is automatically scoped to this
skill's lifetime, so it will not fire during unrelated work in other
sessions. On violation it exits with code 2, which surfaces the diagnostic
back to this conversation. Treat the hook as a safety net, not a
substitute: subtle identifiers (project-specific noun phrases, internal
product names) cannot be matched by regex and still require the model
audit below.

Read the just-written file back from disk (do not audit the in-memory draft),
and verify every checklist item below against the file content:

- [ ] No project / solution / repository / product / codename appears
- [ ] No team / developer / user name, email, or account handle appears
- [ ] No date or time of when the issue was encountered appears
- [ ] No absolute or repo-relative path from the originating codebase appears
- [ ] All project-specific class / namespace / member names have been
      replaced with neutral placeholders (`XxxView`, `XxxViewModel`,
      `IXxxService`, etc.)
- [ ] Each `## N.` item's evidence section reads as Phenomenon → Cause →
      Effect, not as a project incident report
- [ ] No remaining `작성일` / `출처` / "Date written" / "Source" /
      "Originating project" / "Encountered in" style metadata field exists
- [ ] The filename itself carries no date prefix — it is exactly
      `<topic>-wpf-dev-pack-feedback.md`, never `YYYY-MM-DD-<topic>-…`

Public framework / library / API names (`HelixToolkit`, `ScottPlot`,
`CommunityToolkit.Mvvm`, `Prism`, `DispatcherPriority.ApplicationIdle`, etc.)
are part of the technical context — they pass the audit.

If any checklist item fails:

1. Edit the file in place to fix the violation (replace identifying tokens
   with neutral placeholders, drop sentences that cannot be salvaged
   without identifying detail, or rewrite the evidence as a generic
   technical chain).
2. Re-read the file from disk and re-run the entire checklist.
3. Repeat until every item passes.

Do not proceed to Step 6 until the checklist is fully clean.

### Step 6 — Report

Report the absolute path of the written file, then instruct the user:

> 이 md를 `dotnet-with-claudecode` 저장소의 `FeedbackDocs/`에 넣어 Pull Request로
> 기여해주세요. 자세한 절차는 저장소의 CONTRIBUTING 문서를 참조하세요.

Do not commit, push, or attempt to locate the dotnet-with-claudecode repo.
