# 설계 스펙: wpf-dev-pack 지식 스킬 → 로컬 클론 기반 GitHub 지식 MCP 서버

- 날짜: 2026-06-04
- 상태: 설계 승인됨(대화), 사용자 스펙 검토 대기
- 대상 저장소: `christian289/dotnet-with-claudecode`
- 대상 플러그인: `wpf-dev-pack`

---

## 1. 배경 & 문제

`wpf-dev-pack` 플러그인의 `skills/` 디렉터리에는 **60개의 `SKILL.md`** 가 있다.
Claude Code 플러그인 로더는 세션 시작 시 모든 스킬의 `description` frontmatter를
시스템 컨텍스트에 주입한다. 60개 description이 항상 로드되면서 컨텍스트를 과도하게
점유하고, 다른 작업 공간을 압박한다.

이 중 대부분은 "지식형"(코드 패턴/원리 설명, `user-invocable: false`)이고, 소수만
"커맨드형"(`make-wpf-*` 스캐폴딩, 메타 설정 등 `argument-hint` 보유) 이다.

## 2. 목표 / 성공 기준 / 비목표

### 목표
- 지식형 스킬을 MCP 서버 뒤로 옮겨 세션 시작 시 항상 로드되는 description 수를 줄인다.
- 이 하니스에서 MCP 도구는 **지연 로드(deferred)** 되므로 항상-로드 비용 ≈ 0.
- 콘텐츠(자주 변경)와 서버 바이너리(거의 불변)를 분리하여, **md만 수정·push 하면**
  서버 재빌드·NuGet 재배포·플러그인 업데이트 없이 반영되게 한다.

### 성공 기준
1. 세션 시작 시 노출되는 `wpf-dev-pack` 스킬 description: **60 → 11**(잔류 10 + 신규 1).
2. WPF 키워드 입력 시 기존과 동일하게 적절한 지식이 자동으로 Claude에 전달된다
   (`UserPromptSubmit` 훅 라우터 경유).
3. `make-wpf-*` 등 커맨드 스킬은 슬래시 커맨드로 동작 그대로 유지된다.
4. 지식 md 수정이 서버 재빌드 없이 다음 `git pull` 시점에 반영된다.

### 비목표 (YAGNI)
- 기존 `hooks/`·`agents/`·`.claude/rules/*.md`(항상 로드 규칙) 이전.
- 지식 md 본문 번역/영문화(현행 혼용 상태 그대로 이전).
- MCP Prompts/Resources 노출, 풀 RAG 인덱스, 커밋된 manifest, GitHub Trees API.

## 3. 결정 로그 (브레인스토밍 결과 + 근거)

| # | 결정 | 근거 |
|---|------|------|
| D1 | 접근 = **조회 도구 + 얇은 라우터** | 도구는 지연 로드라 항상-로드 0. 라우터로 자동 발동성 보존. |
| D2 | 스코프 = 지식형 → MCP, 커맨드형 → 플러그인 잔류 | 사용자 지정 분리 기준. |
| D3 | 배포 = **dnx + NuGet** | 기존 HandMirrorMcp 패턴과 일치, 마켓플레이스 레포에 바이너리 미포함. |
| D4 | 콘텐츠 = 임베디드 리소스 ❌ → **로컬 git 클론 + 필요 시 `git pull`** | 임베디드는 콘텐츠 수정 시 재빌드+재배포 강제(불합리). 로컬 클론은 레이트리밋 없음·빠름·오프라인 가능. |
| D5 | 오프라인/장애 = 로컬 클론으로 계속 서비스, pull은 best-effort | 네트워크 끊겨도 마지막 로컬 상태로 동작. |
| D6 | 경로 = **유저 명시 지정 필수**(자동 클론 디폴트 폐기) | 사용자 지정. 미설정 시 MCP 사용 불가 + 경고 훅. |
| D7 | 라우터 = 기존 `WpfKeywordDetector.cs`(UserPromptSubmit) 재활용 | 이미 키워드→id 맵 보유, 컨텍스트 비용 0. |
| D8 | MCP 등록 = `wpf-dev-pack/.mcp.json`에 HandMirror와 병기, 소스는 `mcp/` | 사용자 지정("handmirrormcp처럼 포함"). |
| D9 | 네이밍 = `WpfDevPackMcp`(점 없는 파스칼 케이스) | HandMirrorMcp와 동일 컨벤션. |

## 4. 아키텍처

```
사용자 프롬프트
  └─ UserPromptSubmit 훅 (WpfKeywordDetector.cs) — 항상-로드 컨텍스트 0
       ├─ 지식 키워드 감지 → "WpfDevPackMcp get_wpf_topic(<id>) 호출" 지시 주입
       └─ 커맨드 키워드 감지 → "/wpf-dev-pack:<skill>" 제안 (기존 그대로)
       │
Claude ─(MCP 도구 호출)─► PreToolUse 훅 (RepoPathGuard.cs)
       │                     └─ 경로 미설정/무효 → 차단 + 안내(set-repo-path)
       ▼
   WpfDevPackMcp (stdio, dnx, .NET 10)
     ├─ 경로 해석: env WPFDEVPACK_REPO_PATH > ~/.wpf-dev-pack-mcp/config.json
     ├─ 콘텐츠: <repo>/wpf-dev-pack/knowledge/<id>/*.md  (로컬 파일시스템 스캔)
     ├─ 갱신: TTL 경과 시 git pull (best-effort, 비치명적)
     └─ 도구: list_wpf_topics / get_wpf_topic / search_wpf_topics / refresh_wpf_knowledge
       │
   로컬 git 클론  ◄── git pull ──  github.com/christian289/dotnet-with-claudecode (main)
```

### 컴포넌트 목록

| # | 컴포넌트 | 위치 | 신규/변경 |
|---|---------|------|-----------|
| A | `WpfDevPackMcp` MCP 서버 (.NET 10) | 소스 `mcp/` (레포 루트, 플러그인 밖), 배포 dnx/NuGet | 신규 |
| B | 지식 콘텐츠 50개 | `wpf-dev-pack/knowledge/<id>/` | 신규(이동) |
| C | `WpfKeywordDetector.cs` 라우터 | `wpf-dev-pack/hooks/` · UserPromptSubmit | 변경 |
| D | `set-repo-path` 스킬 + `SetWpfDevPackRepoPath.cs` | `wpf-dev-pack/skills/`, `wpf-dev-pack/scripts/` | 신규 |
| E | `RepoPathGuard.cs` 가드 훅 | `wpf-dev-pack/hooks/` · PreToolUse | 신규 |
| F | `config.json` (경로 단일 원천) | `~/.wpf-dev-pack-mcp/` | 신규 |

## 5. 컴포넌트 상세

### A. MCP 서버 `WpfDevPackMcp`

> **Amendment (2026-06-04 follow-up):** 지식 토픽 메인 파일이 `SKILL.md` → `TOPIC.md`로 변경되었고, YAML frontmatter가 제거되었습니다. 요약은 frontmatter `description` 대신 본문 `>` 블록인용(첫 번째 blockquote)에 위치합니다. 서버의 카탈로그 리더가 `FrontmatterReader` → `TopicDocReader`로 교체되었으며, 제목은 첫 번째 `# H1`에서, 요약은 첫 번째 `>` 블록인용에서 읽습니다.

- **프로젝트**: `mcp/WpfDevPackMcp.csproj` — **레포 루트의 `mcp/` 에 위치(플러그인 `wpf-dev-pack/` 밖).** HandMirrorMcp처럼 서버 소스는 플러그인에 번들하지 않고, 플러그인에는 `.mcp.json` 등록만 둔다. (플러그인 안에 두는 것은 `wpf-dev-pack/knowledge/` 지식 콘텐츠뿐.)
  - `net10.0`, `OutputType=Exe`, `AssemblyName=WpfDevPackMcp`, `PackAsTool=true`(dnx 실행).
  - 패키지 의존성: `ModelContextProtocol`(공식 C# SDK), `Microsoft.Extensions.Hosting`.
  - 레퍼런스(`SMVT.Mcp.FunctionBlocks.StdioHost`)와 달리 **단일 프로젝트**(정적 콘텐츠라 메타데이터 Exporter 타겟 불필요). **EmbeddedResource 없음.**
- **호스트 구성** (레퍼런스 패턴):
  - `Host.CreateApplicationBuilder` → 로깅 stderr 전용(`LogToStandardErrorThreshold=Trace`), stdout은 JSON-RPC 전용.
  - `AddMcpServer().WithStdioServerTransport().WithTools<WpfTopicTools>()`.
- **경로 해석 우선순위**:
  1. env `WPFDEVPACK_REPO_PATH`
  2. `~/.wpf-dev-pack-mcp/config.json` 의 `repoPath`
  3. 둘 다 없으면 도구가 "미설정" 오류 반환(가드 훅이 선제 차단).
- **로컬 클론 처리**:
  - 지정 경로가 존재하지 않거나 `.git`이 없으면 최초 사용 시 그 위치로
    `git clone --branch <branch> https://github.com/christian289/dotnet-with-claudecode <repoPath>`.
  - 유효 클론이면 갱신 휴리스틱 적용.
- **갱신 휴리스틱("필요 시 기계적 판단")**:
  - 서버 시작 시 + `(now - lastPullUtc) > TTL` 일 때만 pull. TTL 기본 60분(env `WPFDEVPACK_PULL_TTL_MINUTES` 로 조정).
  - 서버 관리형 클론(서버가 clone) → `git fetch origin && git reset --hard origin/<branch>`.
  - 사용자 지정 워킹 사본 → 워킹트리 clean이면 `git pull --ff-only`, dirty면 skip(로컬 편집 보호).
  - 모든 git 작업은 best-effort. 실패(오프라인/레이트리밋/`git` 부재)는 stderr 로깅 후 **비치명적** — 현재 로컬 상태로 계속 서비스.
  - `lastPullUtc` 등 pull 상태는 서버 관리 `~/.wpf-dev-pack-mcp/state.json` 에 저장(유저 작성 `config.json` 과 분리).
- **카탈로그 구성**:
  - `<repo>/wpf-dev-pack/knowledge/*/TOPIC.md` 스캔 → `TopicDocReader`로 본문 파싱.
  - `id` = 디렉터리명. `title` = 첫 `# H1`(없으면 id). `summary` = 첫 `>` 블록인용(없으면 빈 문자열). YAML frontmatter 없음.
  - 인메모리 캐시, pull 또는 TTL 시 무효화. **별도 manifest 불필요**(로컬 스캔이 인덱스).
- **도구 표면**:

  | 도구 | 인자 | 반환 |
  |------|------|------|
  | `list_wpf_topics` | — | `[{id, title, summary, companions[]}]` |
  | `get_wpf_topic` | `id`, `variant?`(`default`\|`prism`\|`advanced`) | 해당 md 전문 + `companions[]` |
  | `search_wpf_topics` | `query` | 랭킹된 `[{id, title, snippet, score}]` |
  | `refresh_wpf_knowledge` | — | 즉시 pull 강제 후 갱신 결과 요약 |

  - `variant` → 파일 매핑: `default`=`TOPIC.md`, `prism`=`PRISM.md`, `advanced`=`ADVANCED.md`. 없는 variant 요청 시 가용 목록 안내.
  - `companions` = 토픽 디렉터리 내 가용 동반 파일(`PRISM.md`, `ADVANCED.md`, `references/*.md` 등).
  - `search` 점수 = `{id,title,summary,body}` 키워드/부분일치 가중 합(title·summary 가중↑). 상위 N + 스니펫.

### B. 지식 콘텐츠 `wpf-dev-pack/knowledge/`

- 지식 스킬 50개의 **디렉터리 전체**(`TOPIC.md` + `PRISM.md` + `ADVANCED.md` + `references/` + `examples/` + 기타 `*.md`)를 `skills/<id>/` → `knowledge/<id>/` 로 이동(`git mv` 로 이력 보존).
- 토픽 1개 = `knowledge/<id>/`. 콘텐츠 원천 = GitHub의 해당 폴더.
- 각 토픽의 메인 파일은 `TOPIC.md`(YAML frontmatter 없음). 제목은 첫 `# H1`, 요약은 그 아래 첫 `>` 블록인용.
- `knowledge/`는 플러그인 스킬 오토로더 경로가 아니므로 description이 로드되지 않음 → 컨텍스트 절감 달성.

### C. 라우터 — `WpfKeywordDetector.cs` (변경)

- 기존 키워드→스킬id 맵을 **라우팅 테이블로 유지**(컨텍스트 비용 0, 훅은 프롬프트에 미포함).
- 출력 분기:
  - 감지 id가 **지식 토픽** → `"WpfDevPackMcp get_wpf_topic(\"<id>\") 를 호출해 가이드를 로드한 뒤 적용하라"` 형태로 출력.
  - 감지 id가 **잔류 커맨드** → 기존 `-> /wpf-dev-pack:<skill>` 유지.
- 훅 내부에 "잔류 커맨드 id 집합"(아래 §7 목록)을 상수로 두고 그 외는 MCP로 분기.

### D. 설정 스킬 `set-repo-path` + file-based app

- **스킬** `wpf-dev-pack/skills/set-repo-path/SKILL.md`
  - frontmatter: `description`(영문), `argument-hint: [path]`, `disable-model-invocation: true`(휴먼 전용).
  - 본문: `$0`(경로)가 비면 AskUserQuestion으로 요청. 이후
    `dotnet "${CLAUDE_PLUGIN_ROOT}/scripts/SetWpfDevPackRepoPath.cs" "$0"` 실행.
  - 호출: `/wpf-dev-pack:set-repo-path <path>`.
- **file-based app** `wpf-dev-pack/scripts/SetWpfDevPackRepoPath.cs` (`#!/usr/bin/env dotnet`)
  - 검증: 경로 존재 여부, `.git` 존재(git 저장소), `wpf-dev-pack/knowledge` 또는 최소 `wpf-dev-pack/` 포함 여부.
  - 경로 미존재 시: 안내 후 클론 위치로 사용할지 그대로 기록(서버가 최초 사용 시 clone).
  - `~/.wpf-dev-pack-mcp/config.json` 에 `{repoPath, branch}` 기록(머신-전역). 확인 메시지 출력.

### E. 가드 훅 `RepoPathGuard.cs` (신규)

- `wpf-dev-pack/hooks/RepoPathGuard.cs`, **PreToolUse**, matcher = `WpfDevPackMcp` 도구명 패턴(`mcp__WpfDevPackMcp__*`).
- 동작: `config.json` + env 확인 → 유효 `repoPath` 없으면 **도구 호출 차단(deny) + 사유**:
  "경로가 설정되지 않았습니다. `/wpf-dev-pack:set-repo-path <path>` 를 먼저 실행하세요."
- 정확히 "경로 미설정 상태로 MCP 서버를 이용하려 할 때 불가능 경고" 요구를 충족(서버의 모호한 오류 대신 친절한 가드).
- 차단 메커니즘(PreToolUse deny JSON vs exit code 2)은 구현 시 현행 훅 API로 최종 확정(§11 R5).

### F. 설정 파일 `~/.wpf-dev-pack-mcp/config.json`

```json
{ "repoPath": "C:/Users/chris/personal/dotnet-with-claudecode", "branch": "main" }
```

- 세 소비자의 단일 원천: 스킬 D가 **쓰고**, 서버 A·가드 E가 **읽음**.
- 로컬 클론 경로는 프로젝트와 무관한 머신 단위 값이라 전역 위치 채택(언어 설정의 per-project `.claude/*.local.md` 와 구분).

## 6. 데이터 흐름

- **경로 설정됨**: 가드 통과 → 서버가 로컬 클론 스캔 → TTL 경과 시 pull → 토픽 반환.
- **경로 미설정**: `RepoPathGuard` 차단 → 사용자에게 `set-repo-path` 안내.

## 7. 마이그레이션 (skills → knowledge)

### 잔류(커맨드/훅결합) 10개 + 신규 1개 = `skills/` 에 11개
- `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`,
  `make-wpf-converter`, `make-wpf-behavior`, `make-wpf-viewmodel`, `make-wpf-service`
- `collecting-wpf-dev-pack-feedback`, `configuring-wpf-dev-pack-language`
- `formatting-wpf-csharp-code` (CodeFormatter 훅 + `templates/` 결합으로 잔류)
- (신규) `set-repo-path`
- 잔류 커맨드의 동반 파일도 잔류: `make-wpf-project/PRISM.md`, `make-wpf-viewmodel/PRISM.md`, `formatting-wpf-csharp-code/templates/*`.

### MCP로 이전(지식) 50개 → `knowledge/`
`advanced-data-binding`, `authoring-wpf-controls`, `binding-enum-command-parameters`,
`checking-image-bounds-transform`, `configuring-console-app-di`,
`configuring-dependency-injection`, `configuring-wpf-themeinfo`,
`containing-control-decorative-overflow`, `designing-wpf-customcontrol-architecture`,
`displaying-slider-index`, `embedding-pdb-in-exe`, `flaui-cross-process-input`,
`flaui-prism-dialog-discovery`, `flaui-wpf-element-discovery`,
`handling-errors-with-erroror`, `highlighting-nodify-connections`,
`implementing-communitytoolkit-mvvm`, `implementing-hit-testing`,
`implementing-repository-pattern`, `implementing-wpf-splash-screen`,
`implementing-wpf-validation`, `integrating-livecharts2`, `integrating-nodify`,
`integrating-wpfui-fluent`, `managing-literal-strings`,
`managing-styles-resourcedictionary`, `managing-unit-tests`,
`managing-wpf-application-lifecycle`, `managing-wpf-collectionview-mvvm`,
`managing-wpf-popup-focus`, `navigating-visual-logical-tree`,
`optimizing-wpf-memory`, `preventing-dispatcher-deadlock`, `publishing-wpf-apps`,
`rendering-with-drawingcontext`, `rendering-with-drawingvisual`,
`rendering-wpf-architecture`, `rendering-wpf-high-performance`,
`resolving-icon-font-inheritance`, `routing-wpf-events`,
`scottplot-axes-margins-destructive`, `scottplot-syncing-modifier-keys-for-mousewheel`,
`shutting-down-wpf-gracefully`, `structuring-wpf-projects`, `testing-wpf-viewmodels`,
`threading-wpf-dispatcher`, `using-converter-markup-extension`,
`using-xaml-property-element-syntax`, `validating-with-fluentvalidation`,
`virtualizing-wpf-ui`
- 동반 PRISM.md 11개 동반 이전: `implementing-communitytoolkit-mvvm`,
  `configuring-dependency-injection`, `structuring-wpf-projects`,
  `managing-wpf-application-lifecycle`, `binding-enum-command-parameters`,
  `implementing-wpf-validation`, `managing-wpf-collectionview-mvvm`,
  `validating-with-fluentvalidation`, `implementing-repository-pattern`,
  `displaying-slider-index`, `testing-wpf-viewmodels`.
- ADVANCED.md / references / examples / SOURCE-LINK.md 등 동반 파일도 각 디렉터리째 이전.

> 수치 검증: `skills/**/SKILL.md` = 60개. 잔류 10 + 이전 50 = 60. ✅

## 8. `.mcp.json` / 매니페스트 / 문서 변경

### `wpf-dev-pack/.mcp.json` (HandMirror와 병기)
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "description": "MCP servers for WPF Dev Pack plugin",
  "mcpServers": {
    "HandMirrorMcp": { "type": "stdio", "command": "dnx", "args": ["HandMirrorMcp@0.1.1", "--yes"] },
    "WpfDevPackMcp": { "type": "stdio", "command": "dnx", "args": ["WpfDevPackMcp@<ver>", "--yes"] }
  }
}
```

### 기타 문서
- `wpf-dev-pack/skills/.claude/CLAUDE.md`: 키워드 테이블을 잔류 커맨드 행 + 라우팅 설명 포인터로 슬림(훅과 중복 제거 → 컨텍스트 절감).
- `wpf-dev-pack/.claude/CLAUDE.md`: Trigger Behavior 절을 "지식은 MCP `get_wpf_topic`, 커맨드는 슬래시" 로 갱신. "Adding a New Skill" 절에 지식 토픽 추가 절차(= `knowledge/`에 md 추가) 반영.
- 루트 `.claude/CLAUDE.md`: Directory Layout(`mcp/`, `wpf-dev-pack/knowledge/`), Maintainer Workflow 표(`set-repo-path`), Version Update Checklist(MCP 패키지 버전) 갱신.
- `wpf-dev-pack/hooks/hooks.json`: PreToolUse에 `RepoPathGuard` 등록.
- `wpf-dev-pack/hooks/README*.md`: 신규 훅 2종 설명 반영.

### 피드백 적용 워크플로우 변경 (`.claude/skills/applying-wpf-dev-pack-feedback`)

피드백 적용도 **더 이상 스킬이 아니라 지식 베이스(md 문서)** 를 대상으로 한다.
현재 Step 3의 "변경 종류 → 적용 위치" 표(현 `SKILL.md` 라인 93–99)가 전부
`wpf-dev-pack/skills/` 를 가리키므로 다음으로 교체한다:

| 변경 종류 | 적용 위치 (변경 후) |
|---|---|
| **신규 지식 토픽** | `wpf-dev-pack/knowledge/<id>/TOPIC.md` 생성 (frontmatter 없음, 첫 `# H1` = 제목, 첫 `>` 블록인용 = 요약) + `WpfKeywordDetector.cs` 키워드→id 맵에 추가(= 라우팅 원천) + 인접 토픽 교차링크 |
| **지식 보강** | 기존 `wpf-dev-pack/knowledge/<id>/TOPIC.md` 편집 |
| **Prism 9 컴패니언(지식)** | 해당 `knowledge/<id>/` 옆에 `PRISM.md` 추가 |
| 신규 커맨드 스킬 | (기존대로) `wpf-dev-pack/skills/<name>/SKILL.md` + 슬림화된 `skills/.claude/CLAUDE.md` |
| 스캐폴더 현대화 | (기존대로) `make-wpf-*` 스킬 템플릿 |
| 규칙 추가 | (기존대로) `wpf-dev-pack/.claude/rules/<rule>.md` |

**버전/배포 함의:**
- **지식만 변경된 피드백은 플러그인 version bump도, MCP 재배포도 불필요** — 콘텐츠가
  플러그인이 아니라 저장소에서 MCP로 fetch되기 때문. md 수정·push → 다음 `git pull`
  시 반영. apply 스킬 Step 4(version bump)는 지식-only일 때 건너뛰고, APPLIED-LOG의
  `Plugin version` 열은 `(knowledge only, no plugin/MCP bump)` 로 표기.
- 실제 스킬/훅/규칙(플러그인에 실리는 것) 변경 시에만 기존 version bump 적용.

**부수 변경:**
- `MicrosoftSkillCreatorReminder.cs`(이 스킬의 PreToolUse 훅)는 `wpf-dev-pack/skills/` 경로의 `SKILL.md` 작성만 감지합니다. 지식 토픽은 `TOPIC.md`이므로(`/skill.md` suffix에 매칭되지 않음) 훅 대상에서 자연히 제외됩니다. `knowledge/` 경로 절은 추가하지 않습니다.
- 이 스킬은 repo-level `.claude/skills/` 소속이고 `SKILL.md` 영문 정책 대상이라
  `.claude.ko/` 미러 불필요.

## 9. 배포 & 버전

서버는 두 가지 산출물 형태를 지원한다:

1. **dnx + NuGet tool — framework-dependent·platform-agnostic** (`PackAsTool=true`, RID 없음): 단일 `dotnet pack` 이 모든 OS에서 .NET 10 런타임으로 도는 작은 크로스플랫폼 패키지 1개(~1.3MB)를 생성. `.mcp.json` 은 핀 버전(`@<ver>`)을 dnx로 참조. **self-contained/RID별·Native AOT는 의도적으로 미사용** — 플러그인이 이미 .NET 10 SDK를 강제하므로(훅=`dotnet` file-based app, `.mcp.json`=`dnx`) 런타임이 항상 존재. 런타임 내장의 이점이 없고 비용(패키지 크기·멀티 publish·AOT는 소스생성 JSON·OS별 툴체인)만 큼. 필요 시 `<RuntimeIdentifiers>…;any</RuntimeIdentifiers>`+조건부 `<SelfContained>`로 self-contained 전환 가능(dnx 자동선택).
2. **Single-file self-contained exe** (선택 산출물; 게시 프로필 `mcp/Properties/PublishProfiles/win-x64.pubxml`): `dotnet publish ... -p:PublishProfile=win-x64` → `WpfDevPackMcp.exe`(단일 파일, 런타임 내장). NuGet/dnx 없이 순수 exe가 필요할 때. `bin/` 은 gitignore라 산출물 미커밋.

- 빌드/게시/점검 상세는 **`mcp/README.md`** 참조.
- `/wpf-dev-pack-release` 체크리스트에 "MCP 패키지 버전 / `.mcp.json` 핀" 항목 추가.
- `docs/changelogs/wpf-dev-pack.md` 에 본 변경 엔트리(릴리스 스킬이 수행).

## 10. 바이링궐(`.claude.ko/`) 동기화 대상

레포 규약상 `.claude/` 및 CLAUDE.md 변경은 한국어 미러 동기화 필요:
- `.claude.ko/CLAUDE.md`(루트), `wpf-dev-pack/.claude.ko/CLAUDE.md`,
  `wpf-dev-pack/skills/.claude.ko/CLAUDE.md`.
- 훅 README는 in-place `*.ko.md` 규약.
- SKILL.md 본문은 영문 정책 유지(이전 시 내용 보존).

## 11. 리스크 & 완화

| # | 리스크 | 완화 |
|---|--------|------|
| R1 | 모델이 훅 라우터 지시를 무시 → 지식 미로드 | 현 스킬 자동발동도 동일 리스크. 명시적 fetch라 오히려 신뢰도↑. |
| R2 | `git`/네트워크/clone 전제 | 개발자 대상이라 합리적. pull 실패는 비치명적. |
| R3 | `ModelContextProtocol` SDK 도구 API 세부 | 레퍼런스로 패턴 검증됨. 구현 시 패키지로 최종 확인. |
| R4 | dnx는 NuGet publish 선행 | 최초 1회 publish 파이프라인 필요(릴리스 절차에 포함). |
| R5 | PreToolUse 차단 출력 포맷 | 구현 시 현행 훅 API(deny JSON / exit 2)로 확정. |
| R6 | MCP 서버 소스 위치 | 레포 루트 `mcp/`(플러그인 밖). 플러그인엔 `.mcp.json` 등록만 — HandMirrorMcp와 동일. 설치본에 서버 소스 미포함. |

## 12. 구현 단계 개요 (상세 계획은 writing-plans에서)

1. **MCP 서버**: `mcp/WpfDevPackMcp` — 경로 해석·git·카탈로그·도구 4종.
2. **콘텐츠 이전**: `git mv` 로 지식 50개 디렉터리 → `wpf-dev-pack/knowledge/`.
3. **설정 스킬 + app**: `set-repo-path` + `SetWpfDevPackRepoPath.cs`.
4. **가드 훅**: `RepoPathGuard.cs` + `hooks.json` 등록.
5. **라우터 갱신**: `WpfKeywordDetector.cs` 분기 + `skills/.claude/CLAUDE.md` 슬림.
6. **매니페스트/문서**: `.mcp.json`, 플러그인·루트 CLAUDE.md, `.claude.ko/` 미러, 훅 README, changelog.
7. **피드백 워크플로우**: `applying-wpf-dev-pack-feedback` Step 3 표·version 함의 갱신 + `MicrosoftSkillCreatorReminder.cs` 경로(`knowledge/` 포함).
8. **배포**: NuGet publish + `.mcp.json` 버전 핀(`/wpf-dev-pack-release`).
