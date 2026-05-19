# collecting-wpf-dev-pack-feedback Skill Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a human-only wpf-dev-pack skill that analyzes a WPF working session and emits an accumulable improvement-feedback markdown file, plus migrate one existing feedback-style spec into a new `FeedbackDocs/` folder and sync version/index metadata.

**Architecture:** A new `disable-model-invocation: true` skill under `wpf-dev-pack/skills/` runs entirely in the foreign WPF session: auto-draft from conversation → interview augmentation via AskUserQuestion → write a timestamped md to the current working directory. No repo-path discovery, no git. One existing spec is `git mv`-ed to repo-root `FeedbackDocs/`. Metadata files (plugin.json, READMEs, Category Index, project Directory Layout) are updated to register the skill and folder.

**Tech Stack:** Markdown (SKILL.md + generated docs), JSON (plugin.json), git for file moves. No code, no automated test framework — verification is structural (git state, JSON parse, grep).

---

## File Structure

| File | Responsibility |
|------|----------------|
| `wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md` (create) | The skill itself — frontmatter + English workflow checklist + embedded Korean output template |
| `FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md` (move) | Migrated existing feedback corpus entry; first occupant so git tracks the folder |
| `wpf-dev-pack/skills/.claude/CLAUDE.md` (modify) | Register skill in Category Index (new "Meta / Maintenance" row); no Keyword Mapping (no auto-trigger) |
| `wpf-dev-pack/.claude-plugin/plugin.json` (modify) | Version bump 1.6.3 → 1.6.4 |
| `wpf-dev-pack/README.md` (modify) | Version badge → 1.6.4 |
| `wpf-dev-pack/README.ko.md` (modify) | Version badge → 1.6.4 |
| `.claude/CLAUDE.md` (modify) | Add `FeedbackDocs/` to Directory Layout tree |

> Note on commits: plan includes per-task commit steps. Do NOT `git push` (push only on explicit user request). If on `main`, the executor should create a feature branch before the first commit.

---

## Task 1: Create the skill

**Files:**
- Create: `wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md`

- [ ] **Step 1: Write SKILL.md with exact content below**

Create `wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md` containing exactly:

````markdown
---
description: "Analyzes the current WPF working session and produces an accumulable wpf-dev-pack improvement-feedback markdown file in the current directory. Use when, after doing WPF work with wpf-dev-pack skills/scaffolders in a project OUTSIDE the dotnet-with-claudecode repo, you want to capture anti-patterns you hand-fixed, missing or outdated skill guidance, mistaken or missed triggers, and scaffolder gaps — without editing wpf-dev-pack directly. The output md is meant to be contributed to dotnet-with-claudecode/FeedbackDocs via a pull request. User-invocable only."
disable-model-invocation: true
argument-hint: "[topic]"
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

## Workflow

Copy this checklist and track progress:

```
Feedback Collection Progress:
- [ ] Step 1: Auto-draft feedback items from this session
- [ ] Step 2: Augment via interview (AskUserQuestion)
- [ ] Step 3: Write the timestamped md to the current directory
- [ ] Step 4: Report path and contribution instructions
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

### Step 3 — Write the document

Resolve a short kebab-case `<topic>` from the `$0` argument if provided, else
infer it from the session's main subject. Resolve `<YYYY-MM-DD>` from the current
session date.

Write the file to `<cwd>/<YYYY-MM-DD>-<topic>-wpf-devpack-feedback.md` (current
working directory root — do NOT search for or write into any other repo, and do
NOT run git). Use this exact structure. The document BODY is written in Korean to
stay consistent with the existing FeedbackDocs corpus (the body is an artifact,
not skill content, so the English-only skill-content policy does not apply to it):

```markdown
# wpf-dev-pack 피드백 — <한 줄 제목>

- **작성일**: <YYYY-MM-DD>
- **출처**: <어느 프로젝트/세션에서, 무엇을 하다가>
- **목적**: <왜 이 피드백이 필요한가>
- **범위**: <신규 스킬/보강/스캐폴더 현대화 개수, 버전 범프·README 동기화 필요 안내>

---

## 0. 요약 (우선순위)

| ID | 종류 | 대상 | 우선순위 | 한 줄 |
|----|------|------|----------|-------|
| 1  | 신규 스킬/보강/현대화/규칙 | skills/<...>/SKILL.md | High/Medium/Low | ... |

---

## 1. <항목 제목>

### 근거 (세션 증거)
<무엇을 하다 어떤 문제에 부딪혔는지, 손으로 어떻게 고쳤는지 구체 증거>

### 제안 (구체 변경)
<어느 파일에 무엇을 추가/수정할지>

### 인접 스킬과의 차이/링크
<중복 스킬과의 경계, cross-link 대상>
```

Add one numbered `## N.` section per feedback item.

### Step 4 — Report

Report the absolute path of the written file, then instruct the user:

> 이 md를 `dotnet-with-claudecode` 저장소의 `FeedbackDocs/`에 넣어 Pull Request로
> 기여해주세요. 자세한 절차는 저장소의 CONTRIBUTING 문서를 참조하세요.

Do not commit, push, or attempt to locate the dotnet-with-claudecode repo.
````

- [ ] **Step 2: Verify frontmatter parses and length is within the listing budget**

Run:
```
pwsh -NoProfile -Command "$f='wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md'; $t=Get-Content $f -Raw; if($t -notmatch '(?s)^---\r?\n(.*?)\r?\n---'){Write-Error 'no frontmatter'}; $fm=$matches[1]; $d=([regex]'description:\s*\"(.*?)\"').Match($fm).Groups[1].Value; Write-Output ('desc-len=' + $d.Length); if($t -match 'disable-model-invocation:\s*true'){'has disable-model-invocation: true'}else{Write-Error 'missing disable-model-invocation'}"
```
Expected: `desc-len=` value well under 1536, and the line `has disable-model-invocation: true`. No errors.

- [ ] **Step 3: Verify no `model:` frontmatter field exists** (banned per project memory)

Run:
```
pwsh -NoProfile -Command "$t=Get-Content 'wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md' -Raw; if($t -match '(?m)^model:\s'){Write-Error 'model field present — must be removed'}else{'no model field — ok'}"
```
Expected: `no model field — ok`. No error.

- [ ] **Step 4: Commit**

```
git add wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md
git commit -m "feat(wpf-dev-pack): add collecting-wpf-dev-pack-feedback skill"
```

---

## Task 2: Migrate the existing feedback spec into FeedbackDocs/

**Files:**
- Move: `wpf-dev-pack/docs/superpowers/specs/2026-05-18-customcontrol-authoring-upgrade-design.md` → `FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md`

- [ ] **Step 1: Confirm no other file references the path being moved**

Run:
```
pwsh -NoProfile -Command "Select-String -Path (Get-ChildItem -Recurse -Include *.md,*.json -File | Where-Object FullName -notmatch 'docs.superpowers.(specs|plans).2026-05-18') -Pattern 'docs/superpowers/specs/2026-05-18-customcontrol-authoring-upgrade-design' -List | Select-Object Path"
```
Expected: no output (no external references). If any path is listed, note it — Step 4 will update it.

- [ ] **Step 2: Create FeedbackDocs/ and git-move the file**

```
git mv wpf-dev-pack/docs/superpowers/specs/2026-05-18-customcontrol-authoring-upgrade-design.md FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md
```
(`git mv` creates the `FeedbackDocs/` directory automatically.)

- [ ] **Step 3: Verify the move**

Run:
```
pwsh -NoProfile -Command "Test-Path 'FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md'; Test-Path 'wpf-dev-pack/docs/superpowers/specs/2026-05-18-customcontrol-authoring-upgrade-design.md'; Test-Path 'wpf-dev-pack/docs/superpowers/specs/2026-04-03-cmds-pattern-refactoring-design.md'; Test-Path 'wpf-dev-pack/docs/superpowers/plans/2026-04-03-cmds-pattern-refactoring.md'"
```
Expected output (in order): `True`, `False`, `True`, `True` — moved file exists at new path, gone from old path, and the two retained superpowers docs are untouched.

- [ ] **Step 4: Update any external references found in Step 1**

If Step 1 listed any file, edit each to replace
`wpf-dev-pack/docs/superpowers/specs/2026-05-18-customcontrol-authoring-upgrade-design.md`
with `FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md`.
If Step 1 produced no output, skip this step (nothing to change).

- [ ] **Step 5: Commit**

```
git add -A
git commit -m "chore(wpf-dev-pack): migrate customcontrol-authoring feedback spec to FeedbackDocs/"
```

---

## Task 3: Register skill in skills/.claude/CLAUDE.md Category Index

**Files:**
- Modify: `wpf-dev-pack/skills/.claude/CLAUDE.md` (final table `## Skill Category Index`)

- [ ] **Step 1: Add a "Meta / Maintenance" row to the Category Index table**

In `wpf-dev-pack/skills/.claude/CLAUDE.md`, find the last row of the Category Index table:

```
| **Scaffolding** | `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`, `make-wpf-converter`, `make-wpf-behavior`, `make-wpf-viewmodel`, `make-wpf-service` |
```

Insert a new row immediately after it:

```
| **Meta / Maintenance** | `collecting-wpf-dev-pack-feedback` |
```

Do NOT add anything to the Keyword-Skill Mapping section — this skill is
`disable-model-invocation: true` and has no keyword auto-trigger.

- [ ] **Step 2: Verify the row exists and Keyword Mapping is untouched**

Run:
```
pwsh -NoProfile -Command "$t=Get-Content 'wpf-dev-pack/skills/.claude/CLAUDE.md' -Raw; if($t -match '\*\*Meta / Maintenance\*\* \| `collecting-wpf-dev-pack-feedback`'){'category row ok'}else{Write-Error 'category row missing'}; $km=($t -split '## Skill Category Index')[0]; if($km -match 'collecting-wpf-dev-pack-feedback'){Write-Error 'must NOT appear in Keyword Mapping'}else{'keyword mapping clean'}"
```
Expected: `category row ok` and `keyword mapping clean`. No errors.

- [ ] **Step 3: Commit**

```
git add wpf-dev-pack/skills/.claude/CLAUDE.md
git commit -m "docs(wpf-dev-pack): index collecting-wpf-dev-pack-feedback skill"
```

---

## Task 4: Version bump and README badges

**Files:**
- Modify: `wpf-dev-pack/.claude-plugin/plugin.json:4`
- Modify: `wpf-dev-pack/README.md:9`
- Modify: `wpf-dev-pack/README.ko.md:9`

- [ ] **Step 1: Bump plugin.json version**

In `wpf-dev-pack/.claude-plugin/plugin.json`, change line 4 from:
```
  "version": "1.6.3",
```
to:
```
  "version": "1.6.4",
```

- [ ] **Step 2: Update README.md version badge**

In `wpf-dev-pack/README.md`, change line 9 from:
```
[![Version](https://img.shields.io/badge/version-1.6.3-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
```
to:
```
[![Version](https://img.shields.io/badge/version-1.6.4-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
```

- [ ] **Step 3: Update README.ko.md version badge**

In `wpf-dev-pack/README.ko.md`, change line 9 from:
```
[![Version](https://img.shields.io/badge/version-1.6.3-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
```
to:
```
[![Version](https://img.shields.io/badge/version-1.6.4-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
```

- [ ] **Step 4: Verify JSON validity and version consistency**

Run:
```
pwsh -NoProfile -Command "$j=Get-Content 'wpf-dev-pack/.claude-plugin/plugin.json' -Raw | ConvertFrom-Json; Write-Output ('plugin=' + $j.version); $r=Get-Content 'wpf-dev-pack/README.md' -Raw; $k=Get-Content 'wpf-dev-pack/README.ko.md' -Raw; if($j.version -eq '1.6.4' -and $r -match 'version-1.6.4-blue' -and $k -match 'version-1.6.4-blue' -and $r -notmatch 'version-1.6.3-blue' -and $k -notmatch 'version-1.6.3-blue'){'version sync ok'}else{Write-Error 'version mismatch'}"
```
Expected: `plugin=1.6.4` and `version sync ok`. No error (JSON parses cleanly).

- [ ] **Step 5: Commit**

```
git add wpf-dev-pack/.claude-plugin/plugin.json wpf-dev-pack/README.md wpf-dev-pack/README.ko.md
git commit -m "chore(wpf-dev-pack): v1.6.4 - register feedback collection skill"
```

---

## Task 5: Add FeedbackDocs/ to project Directory Layout

**Files:**
- Modify: `.claude/CLAUDE.md` (Directory Layout code block)

- [ ] **Step 1: Insert FeedbackDocs/ into the Directory Layout tree**

In `.claude/CLAUDE.md`, find:
```
├── wpf-dev-pack/                 # WPF 전용 플러그인 (현재 유일한 호스팅 대상)
├── archive-skills/               # microsoft-docs MCP로 대체되어 보관된 구 skill
└── docs/                         # 프로젝트 문서
```

Replace with:
```
├── wpf-dev-pack/                 # WPF 전용 플러그인 (현재 유일한 호스팅 대상)
├── FeedbackDocs/                 # 외부 세션 wpf-dev-pack 피드백 md 누적 폴더
├── archive-skills/               # microsoft-docs MCP로 대체되어 보관된 구 skill
└── docs/                         # 프로젝트 문서
```

- [ ] **Step 2: Verify the entry exists**

Run:
```
pwsh -NoProfile -Command "$t=Get-Content '.claude/CLAUDE.md' -Raw; if($t -match 'FeedbackDocs/\s+# 외부 세션 wpf-dev-pack 피드백 md 누적 폴더'){'layout entry ok'}else{Write-Error 'layout entry missing'}"
```
Expected: `layout entry ok`. No error.

- [ ] **Step 3: Commit**

```
git add .claude/CLAUDE.md
git commit -m "docs: add FeedbackDocs/ to project directory layout"
```

---

## Task 6: Final cross-cutting verification

**Files:** none (verification only)

- [ ] **Step 1: Confirm full deliverable state**

Run:
```
pwsh -NoProfile -Command "Test-Path 'wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md'; Test-Path 'FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md'; (Get-Content 'wpf-dev-pack/.claude-plugin/plugin.json' -Raw | ConvertFrom-Json).version; git status --porcelain"
```
Expected: `True`, `True`, `1.6.4`, and an empty `git status --porcelain` (all changes committed).

- [ ] **Step 2: Confirm retained superpowers docs were NOT moved**

Run:
```
pwsh -NoProfile -Command "Test-Path 'wpf-dev-pack/docs/superpowers/specs/2026-04-03-cmds-pattern-refactoring-design.md'; Test-Path 'wpf-dev-pack/docs/superpowers/plans/2026-04-03-cmds-pattern-refactoring.md'; Test-Path 'wpf-dev-pack/docs/superpowers/specs/2026-05-18-collecting-wpf-devpack-feedback-design.md'"
```
Expected: `True`, `True`, `True` — the two retained docs and this feature's design spec all remain in place.

- [ ] **Step 3: Done**

No commit (verification only). Report completion. Do NOT `git push` — pushing is a separate explicit user request.

---

## Self-Review

**1. Spec coverage:**
- F1 (new skill) → Task 1 ✓
- F1-T (output template) → embedded in Task 1 SKILL.md content ✓
- F2 (migrate single file, retain others) → Task 2 (+ Task 6 Step 2 guards retention) ✓
- F3 (Category Index, no Keyword Mapping) → Task 3 ✓
- F4 (version + READMEs) → Task 4 ✓
- F5 (Directory Layout) → Task 5 ✓
- F6 (CONTRIBUTING — user-owned, out of scope) → intentionally no task; documented as non-goal ✓
- Non-goals (no repo discovery, no git in skill, no PR automation, no evals) → encoded in Task 1 SKILL.md text; no eval task created ✓

**2. Placeholder scan:** SKILL.md uses `<topic>`, `<YYYY-MM-DD>`, `<한 줄 제목>` etc. — these are runtime template tokens the skill fills at execution, not plan placeholders; they are explicitly defined in Step 3 of the skill workflow. No "TBD/TODO/implement later" present.

**3. Type/name consistency:** Skill directory and name `collecting-wpf-dev-pack-feedback` consistent across Tasks 1/3/6 and Category Index row. Generated file suffix `-wpf-devpack-feedback.md` is intentionally distinct from the design-spec filename `collecting-wpf-devpack-feedback-design.md` (different artifacts) — no collision. Version string `1.6.4` consistent across Task 4 and verification in Task 6.
