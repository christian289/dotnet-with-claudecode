---
description: "Analyzes the current WPF working session and produces an accumulable wpf-dev-pack improvement-feedback markdown file in the current directory. Use when, after doing WPF work with wpf-dev-pack skills/scaffolders in a project OUTSIDE the dotnet-with-claudecode repo, you want to capture anti-patterns you hand-fixed, missing or outdated skill guidance, mistaken or missed triggers, and scaffolder gaps — without editing wpf-dev-pack directly. The output md is meant to be contributed to dotnet-with-claudecode/FeedbackDocs via a pull request."
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
