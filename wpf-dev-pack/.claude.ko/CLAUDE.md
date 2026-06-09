# WPF Dev Pack - Configuration

WPF 지식은 WpfDevPackMcp MCP 서버가 온디맨드로 제공합니다(답변 전 검색).
커맨드 스킬은 슬래시로 호출합니다. 키워드 감지 훅은 없습니다.

> 본 파일은 `wpf-dev-pack/.claude/CLAUDE.md`의 한국어 미러입니다. AI는 영문
> 원본을 읽으며, 본 파일은 사람을 위한 참고용입니다. 갱신은 영문 원본을
> 먼저 수정한 뒤 본 파일에 반영하세요.

---

## 필수 플러그인 의존성

모든 agent는 다음 Claude Code 플러그인이 설치되어 있어야 합니다:

| Plugin | MCP Server | 용도 |
|--------|-----------|------|
| **context7** | context7 | 라이브러리/프레임워크 문서 |
| **microsoft-docs** | microsoft-learn | Microsoft 공식 문서 |
| **csharp-lsp** | csharp | C# LSP 코드 지능 |

## 필수 MCP (Claude Code 플러그인으로 설치하지 않음)

다음 MCP 서버는 agent들이 필요로 하지만 **Claude Code 플러그인 경로로
등록해서는 안 됩니다** — Claude Code의 내장 도구 설명이 모델로 하여금
그 도구들을 사용하지 않도록 강하게 편향시킵니다. `uv`를 통해 MCP 서버
형태로 직접 설치하세요.

| MCP Server | 용도 | 설치 방법 |
|---|---|---|
| **serena** | Semantic 코드 분석, 심볼 네비게이션 | [Quick Start](https://github.com/oraios/serena#quick-start)에 따라 `uv`로 직접 설치. 이유는 [Serena Claude Code 문서의 Attention note](https://oraios.github.io/serena/02-usage/030_clients.html#claude-code) 참조. |

---

## MVVM Composition Style

wpf-dev-pack은 **MVVM 프레임워크별 단일 매칭 경로**를 강제하며, 양쪽
모두 **Stateful ViewModel**에 기반합니다. Composition 방향은 프레임워크
별로 다릅니다. 용어 및 Microsoft 참조는
[`docs/TERMINOLOGY.md`](../docs/TERMINOLOGY.md) 참조.

| MVVM 프레임워크 | Composition 방향 | 메커니즘 | 와이어링 규칙 |
|---|---|---|---|
| CommunityToolkit.Mvvm (기본값) | **ViewModel First** | `Mappings.xaml` + 암시적 DataTemplate | `rules/view-viewmodel-wiring-communitytoolkit.md` |
| Prism 9 (대안) | **View First** | `RegisterForNavigation` + `IRegionManager` | `rules/view-viewmodel-wiring-prism.md` |

> v1.6.4 이전 문서는 모두 이를 "View First MVVM"으로 라벨링했습니다.
> 이 단일 라벨은 Microsoft 공식 정의 기준으로 부정확했습니다
> (`Mappings.xaml`의 lookup key는 ViewModel 타입 → ViewModel First).
> v1.6.4에서 경로별로 라벨을 바로잡았습니다. 강제되는 코드 패턴은
> 변경 없음.

금지된 대안(`ViewModelLocator`, 코드 비하인드 `DataContext` 할당,
Stateless VM 패턴, 두 경로 혼용 등)은 `rules/prohibitions.md`
(P-001…P-004) 참조.

---

## Essential (Post-Compact)

다음 규칙은 컨텍스트 압축 후에도 반드시 살아남아야 합니다. 이전 컨텍스트가
유실된 경우 이 섹션을 다시 읽으세요:

1. **No ViewModelLocator** — DI + DataTemplate 매핑만 사용 (`rules/prohibitions.md`)
2. **No System.Windows in ViewModel** — BCL 타입만 (`rules/mvvm-constraints.md`)
3. **모든 Freezable 객체 Freeze** — Brush, Pen, Geometry (`rules/freezable-performance.md`)
4. **Generic.xaml = MergedDictionaries 허브 전용** (`rules/resourcedictionary-patterns.md`)
5. **코드 작성 전 HandMirror로 API 시그니처 검증**
6. **프레임워크별 단일 매칭 경로** — ViewModel First (CommunityToolkit, `Mappings.xaml`) 또는 View First (Prism, `RegisterForNavigation`). 프레임워크별 와이어링은 `docs/TERMINOLOGY.md` 및 `rules/` 참조.
7. **WPF 지식 토픽은 `WpfDevPackMcp get_wpf_topic(id)`로 조회** — `skills/`에서 로드하지 않음.

---

## Per-Project Language Preference

플러그인은 프로젝트별 응답 언어 환경설정을 지원하며,
`LanguagePreferenceLoader` SessionStart 훅이 매 새 대화 시작 시 이를
읽어 시스템 컨텍스트에 주입합니다.

- **설정**: `/wpf-dev-pack:configuring-wpf-dev-pack-language`를 실행해
  `.claude/wpf-dev-pack.local.md`에 `language:` 필드(BCP-47 코드,
  예: `ko`, `en`, `ja`, `zh`)를 작성.
- **효과**: 다음 세션부터 훅이 시스템 컨텍스트에 해당 언어로 응답하도록
  지시문을 주입. SessionStart 훅은 세션 시작 시에만 발화하므로 현재
  세션에는 in-session 변경이 반영되지 않음.
- **범위**: wpf-dev-pack 컨텍스트 내 사용자 응답에 적용. Skill 콘텐츠
  언어 정책(SKILL.md 본문, 코드 주석)은 영향 없음 — 영문 단일 유지.
- **오버라이드**: 사용자가 in-conversation으로 항상 오버라이드 가능
  ("respond in English" / "한글로 답해줘"). 훅은 세션 기본값만 설정.
- **되돌리기**: `.claude/wpf-dev-pack.local.md` 삭제 또는 `language:`
  필드 제거. 훅은 아무것도 emit하지 않게 되고 플러그인 기본 언어
  동작이 적용됨.

이 파일은 개인용이며 저장소 `.gitignore`(`.claude/*.local.md`)가
처리합니다.

## .NET 버전 설정

### 버전 선택 규칙

1. **최소 지원 버전**: **.NET 8** (C# 12)
2. **사용자가 버전 명시** → 해당 버전 + 매핑 표의 C# 버전 사용
3. **명시 없음** → **최신 안정 .NET 사용** (현재 .NET 10)

### .NET ↔ C# 버전 매핑

| .NET 버전 | C# 버전 | TargetFramework | 주요 기능 |
|--------------|------------|-----------------|-----------|
| .NET 10 | C# 14 | `net10.0-windows` | Extensions, field keyword |
| .NET 9 | C# 13 | `net9.0-windows` | params collections, lock object |
| .NET 8 | C# 12 | `net8.0-windows` | Primary constructors, collection expressions |
| .NET 7 | C# 11 | `net7.0-windows` | Raw string literals, list patterns |
| .NET 6 | C# 10 | `net6.0-windows` | Global using, file-scoped namespace |
| .NET 5 | C# 9 | `net5.0-windows` | Records, init-only, top-level statements |
| .NET Core 3.1 | C# 8 | `netcoreapp3.1` | Nullable reference types, async streams |
| .NET Framework 4.8 | C# 7.3 | `net48` | Tuples, pattern matching, local functions |

> **갱신 정책**: 새 .NET 버전이 릴리스되면 이 표에 행 추가.
> 마지막 갱신: 2026-01 (최신 안정: .NET 10)

### 코드 생성 규칙

WPF 프로젝트나 코드를 생성할 때:

```
IF 사용자가 ".NET X" 명시:
    netX.0-windows + 매핑 표의 C# 버전 사용
ELSE:
    매핑 표 최상단의 최신 안정 .NET 사용
```

- 항상 대상 .NET 버전에서 사용 가능한 **최대 C# 기능** 활용
- `Microsoft.Extensions.Hosting`은 .NET major 버전과 맞추기
- 예: .NET 10 → `Microsoft.Extensions.Hosting` 10.x

---

## 핵심 규칙

```
RULE 1: WPF/C#/.NET 질문 → 답하기 전 WpfDevPackMcp 토픽 검색 (search_wpf_topics → get_wpf_topic)
RULE 2: 복잡한 작업은 전문 agent에 위임
RULE 3: 커맨드 스킬 활성화 announce
RULE 4: 복수 매치 시 가장 구체적인 토픽/스킬 선택
RULE 5: wpf-architect는 분석 전 Requirements Interview 필수 수행
```

---

## Requirements Interview System

`wpf-architect` 호출 시 AskUserQuestion을 사용해 **적응형 경로 기반
인터뷰**를 수행:

| 경로 | 작업 유형 | 단계 | 초점 |
|------|-----------|------|------|
| **A** | 신규 프로젝트 생성 | 7 | concept → architecture → scale → complexity → libraries → feature areas |
| **B** | 분석/개선 | 5 | analysis goal → analysis mode → scope → output format |
| **C** | 기능 구현 | 5 | feature description → implementation approach → libraries → feature areas |
| **D** | 디버그/수정 | 4 | symptom → problem type → problem area |

**키워드 분석**: 자유 입력 단계(A-2, B-2, C-2, D-2)에서 키워드를 감지해
후속 단계의 기본값을 자동 설정.

상세 인터뷰 사양은 `agents/wpf-architect.md` 참조.

## 트리거 우선순위

1. **명시적 slash command** (`/wpf-dev-pack:skill-name`) → 커맨드 스킬
2. **WPF 지식** → WpfDevPackMcp로 검색/조회 (`search_wpf_topics` → `get_wpf_topic`); `skills/.claude/CLAUDE.md` 참조
3. **컨텍스트 기반 추론** → 전문 agent에 위임

## 트리거 동작

**트리거 시:**
1. Announce: "WPF Dev Pack: Activating `skill-name` skill."
2. `.claude/rules/dotnet/wpf/mvvm-framework.md`에서 활성 MVVM 프레임워크 확인
3. 콘텐츠 로드:
   - **지식 토픽** → `WpfDevPackMcp get_wpf_topic(id[, variant])`를 호출해 MCP에서 가져옴
   - **커맨드 스킬** → 슬래시 명령으로 호출 (`/wpf-dev-pack:<skill-name>`)
   - **CommunityToolkit.Mvvm 커맨드 스킬** → SKILL.md
   - **Prism 9 커맨드 스킬** → PRISM.md 있으면 그 파일, 없으면 SKILL.md
4. 가이드라인 및 활성 프레임워크 규칙에 따라 코드 생성/수정

**Silent 적용** (announce 없음):
- `formatting-wpf-csharp-code` — `.cs` / `.xaml` 편집 시 `CodeFormatter` PostToolUse 훅이 자동 적용.

**복수 키워드:**
1. 가장 구체적인 것 우선 (예: "drawingcontext" > "performance")
2. 관련 skill은 병렬로 참조 가능
3. 충돌 시 사용자에게 확인

---

## 새 Skill 추가 시 — 필수 동반 갱신

**지식 토픽 추가** (WPF 지식, MCP를 통해 제공 — 플러그인 스킬이 아님):
1. `knowledge/<id>/TOPIC.md`(레포 루트, 플러그인 밖)에 토픽 콘텐츠를 작성합니다. **YAML frontmatter 없음.** 첫 번째 `# H1`이 제목이며, H1 바로 아래에 한 줄짜리 `> 요약` 블록인용을 작성합니다 — MCP 카탈로그(`TopicDocReader`)는 첫 번째 H1에서 제목을, 첫 번째 `>` 블록인용에서 요약을 읽습니다.
2. 라우터 수정 불필요, 플러그인 스킬 등록 불필요, 버전 범프 불필요, MCP 재빌드 불필요 — MCP 카탈로그가 새 디렉터리를 자동 발견하고 다음 `git pull` 시 `search_wpf_topics`로 노출됩니다.

**커맨드 스킬 추가** (슬래시 호출 가능한 플러그인 스킬, `skills/` 하위):

`skills/<skill-name>/SKILL.md`로 새 skill을 추가할 때 함께 갱신해야 하는 파일:

1. **`skills/.claude/CLAUDE.md`** — 잔류 커맨드 표에 행 추가.
2. **인접 기존 SKILL.md** — 주제가 겹치는 경우 새 skill로의 cross-link 추가 (`See [...](../skill-name/SKILL.md)`).
3. **Prism 9 분기가 필요한 skill** — `PRISM.md` 컴패니언 파일 작성 (`mvvm-framework.md` 참조).
4. **Foundation + Application 쌍 skill** — 두 skill을 별도로 만들고 상호 참조. Foundation skill은 메커니즘·일반 원칙을 다루고, Application skill은 구체 시나리오에 적용 (예: `preventing-dispatcher-deadlock` + `shutting-down-wpf-gracefully`).
