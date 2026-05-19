# collecting-wpf-dev-pack-feedback 스킬 — 설계

- **작성일**: 2026-05-18
- **출처**: wpf-dev-pack을 외부 WPF 프로젝트 세션에서 사용하면서 발견한 개선점을, 그 세션에서 직접 고치지 않고 피드백 문서로 누적·기여하기 위한 워크플로 요구
- **목적**: 사용자가 직접 트리거하는(human-only) 스킬을 추가하여, WPF 작업 세션 대화를 분석해 wpf-dev-pack 개선 피드백 md 문서를 생성하고, 이를 `dotnet-with-claudecode/FeedbackDocs/`에 기여할 수 있게 한다
- **범위**: 신규 스킬 1개 + 기존 spec 1개 마이그레이션 + 부수 동기화(버전 범프·README·Category Index·Directory Layout). 별도 레포 기여 모델이므로 git 자동 처리는 하지 않음

---

## 0. 요약 (우선순위)

| ID | 종류 | 대상 | 우선순위 | 한 줄 |
|----|------|------|----------|-------|
| F1 | 신규 스킬 | `wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md` | High | 세션 분석 → 피드백 초안 → 인터뷰 보강 → 현재 cwd에 타임스탬프 md 생성 |
| F2 | 마이그레이션 | `2026-05-18-customcontrol-authoring-upgrade-design.md` → `FeedbackDocs/` | High | 기존 피드백성 spec 1개만 신설 폴더로 이동, 나머지 superpowers 문서는 유지 |
| F3 | 동기화 | `skills/.claude/CLAUDE.md` Category Index | Medium | 신규 스킬을 인덱스에 등재 (auto-trigger 없으므로 Keyword Mapping 제외) |
| F4 | 동기화 | `plugin.json` + `README.md` + `README.ko.md` | Medium | v1.6.3 → v1.6.4 버전 범프 및 뱃지 갱신 |
| F5 | 동기화 | 프로젝트 `.claude/CLAUDE.md` Directory Layout | Low | `FeedbackDocs/` 디렉토리 등재 |
| F6 | 문서(사용자 담당) | `CONTRIBUTING` | Low | 스킬 산출물을 `FeedbackDocs/`에 PR로 기여하라는 안내 — 사용자가 직접 추가 |

---

## F1. 신규 스킬 — `collecting-wpf-dev-pack-feedback`

### 근거
wpf-dev-pack은 오픈소스이며 사용자/PC마다 로컬 클론 경로가 다르다. WPF 실제 작업은 `dotnet-with-claudecode`가 아닌 외부 폴더 세션에서 일어나므로, 그 세션에서 wpf-dev-pack을 직접 수정하는 것은 비효율적이고 경로 가정도 불가능하다. 따라서 "개선점을 구조화된 md로 추출"하고, 그 md를 별도 기여 흐름(PR)으로 레포에 누적하는 분리 모델이 필요하다. 기존 `2026-05-18-customcontrol-authoring-upgrade-design.md`가 이미 이 형태의 산출물이다.

### 위치 및 frontmatter

경로: `wpf-dev-pack/skills/collecting-wpf-dev-pack-feedback/SKILL.md`

```yaml
---
description: "<영문 — 무엇을/언제. 3인칭. wpf-dev-pack 사용 세션을 분석해 개선 피드백 md를 생성하는 사용자 트리거 스킬. front-load 핵심 use case>"
disable-model-invocation: true
argument-hint: "[topic]"
---
```

- `disable-model-invocation: true` — 부작용(파일 생성) 있는 사용자 전용 워크플로. Claude 자동 호출 차단, `/wpf-dev-pack:collecting-wpf-dev-pack-feedback`로만 실행. 커맨드가 아니라 스킬.
- `user-invocable` 생략(기본 true) — `/` 메뉴 노출.
- `model` frontmatter **금지** (1M context entitlement 비호환, 세션 모델 상속).
- `argument-hint: [topic]` — 선택적 주제 인자. 비면 스킬이 세션에서 주제를 추론하거나 인터뷰에서 확정.
- SKILL.md 본문·description·코드 예시 주석: **영문 단일** (스킬 콘텐츠 언어 정책).
- evals 생략 — model 트리거가 없는 human-only 스킬이라 트리거 eval이 부적합.

### 동작 워크플로 (SKILL.md 본문에 영문 체크리스트로 명시)

1. **자동 초안 (Auto-draft)**: 현재 세션 대화를 분석하여 피드백 후보를 추출한다. 분석 대상:
   - 사용된 wpf-dev-pack 스킬과, 그 안내가 부정확/불충분했던 지점
   - 손으로 직접 고친 안티패턴 (스킬에 미반영된 교훈)
   - 누락되었거나 구식인 스킬/규칙 내용
   - 트리거되어야 했는데 안 된(또는 잘못 트리거된) 키워드
   - 스캐폴더(make-wpf-*)가 생성하지 못한/잘못 생성한 산출물
2. **인터뷰 보강 (Interview)**: `AskUserQuestion`으로 자동 분석이 놓친 항목을 추가 수집. 최소 1회, 최대 한두 질문(예: "트리거됐지만 잘못 안내한 스킬?", "직접 손으로 작성해 스캐폴딩되어야 할 패턴?").
3. **문서 작성 (Write)**: 현재 작업 디렉토리 루트에 `YYYY-MM-DD-<topic>-wpf-devpack-feedback.md` 생성. 날짜는 세션 컨텍스트의 현재 날짜, `<topic>`은 주제 kebab-case. **repo 경로 탐색·git add/commit/push 일절 없음.**
4. **보고 (Report)**: 생성된 절대 경로를 보고하고, "이 md를 `dotnet-with-claudecode`의 `FeedbackDocs/`에 넣어 PR로 기여해주세요. 자세한 절차는 레포 CONTRIBUTING 참조"라고 안내.

### 인접 스킬과의 차이
- `hookify:conversation-analyzer`는 대화에서 "차단할 행동"을 찾아 hook을 만든다. 본 스킬은 wpf-dev-pack **콘텐츠 개선 제안**을 md로 추출한다. 목적·산출물 모두 다름.
- `skill-creator`/`plugin-dev:skill-development`는 스킬을 직접 생성/수정한다. 본 스킬은 외부 세션에서 **수정하지 않고 제안만** 누적한다.

---

## F1-T. 생성 문서 템플릿

기존 `2026-05-18-customcontrol-authoring-upgrade-design.md` 구조를 그대로 계승한다. **생성 문서 본문 언어는 한글** (FeedbackDocs 누적 코퍼스가 한글이므로 일관성 유지 — 이는 산출물이며 SKILL.md 자체가 아니므로 스킬 콘텐츠 영문 정책과 무관).

```markdown
# wpf-dev-pack 피드백 — <한 줄 제목>

- **작성일**: YYYY-MM-DD
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

---

## F2. 마이그레이션 (1회성, 단 1개 파일)

- 이동: `wpf-dev-pack/docs/superpowers/specs/2026-05-18-customcontrol-authoring-upgrade-design.md`
  → `FeedbackDocs/2026-05-18-customcontrol-authoring-upgrade-design.md`
- `FeedbackDocs/` 폴더 신설 (레포 루트 `dotnet-with-claudecode/FeedbackDocs/`).
- **유지(이동 안 함)**: `docs/superpowers/specs/2026-04-03-cmds-pattern-refactoring-design.md`, `docs/superpowers/plans/2026-04-03-cmds-pattern-refactoring.md`, 루트 `docs/` 전체, 본 설계 문서 자체.
- 이동 파일을 참조하는 경로가 있는지 grep 후, 있으면 갱신.

---

## F3. skills/.claude/CLAUDE.md 동기화

- `## Skill Category Index`에 신규 스킬 등재. 신규 카테고리 **"Meta / Maintenance"** 추가하여 `collecting-wpf-dev-pack-feedback` 배치.
- `## Keyword-Skill Mapping`에는 **추가하지 않음** — `disable-model-invocation: true`라 키워드 auto-trigger가 없음.
- `## Adding a New Skill — Required Co-updates` 규칙을 만족: 토픽 겹치는 기존 스킬이 없으므로 cross-link 의무 없음. PRISM.md/Foundation-Application 쌍 비해당.

---

## F4. 버전 범프 및 README

Plugin Version Update Checklist 준수:
- `wpf-dev-pack/.claude-plugin/plugin.json` — `version` `1.6.3` → `1.6.4`
- `wpf-dev-pack/README.md` — 버전 뱃지
- `wpf-dev-pack/README.ko.md` — 버전 뱃지

---

## F5. 프로젝트 .claude/CLAUDE.md Directory Layout

`Directory Layout` 트리에 `FeedbackDocs/` 항목 추가 — "외부 세션 피드백 md 누적 폴더"로 한 줄 설명.

---

## F6. CONTRIBUTING (사용자 담당)

사용자가 직접 추가 예정: `collecting-wpf-dev-pack-feedback` 스킬로 생성한 md를 `FeedbackDocs/`에 넣어 PR로 기여하라는 안내. 본 작업 범위에서는 구현하지 않음. 요청 시 초안만 별도 제공.

---

## 비목표 (Non-goals)

- 외부 세션에서 `dotnet-with-claudecode` 로컬 경로를 탐색/추정하지 않는다.
- 스킬이 git add/commit/push를 수행하지 않는다.
- 기여 자동화(PR 생성 등)는 범위 밖. CONTRIBUTING 안내로 사용자가 수동 기여.
- evals.json을 만들지 않는다 (human-only 스킬).
