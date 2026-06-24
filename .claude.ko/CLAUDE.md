# dotnet-with-claudecode Project Configuration

## Repository Overview

이 저장소는 .NET 관련 Claude Code 플러그인을 호스팅하는 **마켓플레이스 저장소**입니다.
- 저장소 루트의 `.claude/rules`는 모든 플러그인에 공통 적용되는 범용 규칙만 포함
- 특정 플러그인에 편향된 설정(hooks, permissions 등)은 각 플러그인 내부에서 관리
- 현재 호스팅 중인 플러그인: `wpf-dev-pack` (단일)

## 이중 언어 문서 컨벤션 (`.claude.ko/`)

본 저장소는 `.claude/` 하위 규칙의 한국어 번역본을 `.claude/`와 동일한
폴더 구조로 미러링한 `.claude.ko/` 디렉토리에 보관합니다. 컨벤션은
다음과 같습니다:

- **`.claude/` 하위 파일** — **AI가 실제로 읽는 source of truth**.
  영어. 모든 규칙 로딩, 시스템 프롬프트 주입, 자동화 동작은 이
  파일을 기준으로 합니다.
- **`.claude.ko/` 하위 파일** — **사람을 위한 참고용. AI는 읽지 않습니다.**
  메인테이너 및 한국어 기여자를 위해 유지하는 한국어 번역본입니다.
  디렉토리가 `.claude/` 바깥에 있어 Claude Code의
  `.claude/rules/**/*.md` 자동 로더가 잡지 않습니다.

`.claude/<path>.md` 각각에 대응되는 한글 미러는
`.claude.ko/<same-path>.md`입니다. 개별 파일에 `.ko` 접미사는 더 이상
사용하지 않습니다 — 디렉토리 이름이 언어 표시를 담당합니다.

규칙·지침을 갱신할 때는 `.claude/` 하위 파일을 먼저 수정한 뒤 그 변경을
대응되는 `.claude.ko/` 파일에 반영합니다. 반대 방향으로는 갱신하지
않습니다 — 두 파일이 어긋날 경우 `.claude/` 쪽이 우선합니다.

이 컨벤션은 다음에 적용됩니다:

- 루트 레벨 `CLAUDE.md` (`.claude/CLAUDE.md` ↔ `.claude.ko/CLAUDE.md`).
- `.claude/rules/` 하위 전 파일 (`.claude.ko/rules/`에 미러링).
- `.claude/` 바깥에 위치하는 저장소 노출 문서 — 자동 로드 이슈가 없으므로
  in-place `<name>.ko.md` 접미사를 그대로 사용합니다: `README.md` /
  `README.ko.md`, `.github/` 아래 CONTRIBUTING 쌍,
  `wpf-dev-pack/hooks/` 아래 hooks README 등.

플러그인 번들 AI 대상 콘텐츠에는 적용되지 않습니다 — skill 본문 등은 별도
정책에 따라 영문 단일입니다 (`.claude/rules/claude-skills/best-practices.md`
참조): `SKILL.md` 파일과 `wpf-dev-pack/context/` 아래 세션 주입 정책 문서
(`CorePolicyLoader` SessionStart 훅이 전달). 플러그인은 `CLAUDE.md`를 갖지
않습니다 — 플러그인 `CLAUDE.md`는 설치 사용자에게 자동 로드되지 않으므로,
플러그인 컨텍스트는 skill·agent·hook으로 전달합니다.

## Directory Layout

```
dotnet-with-claudecode/
├── .claude/
│   ├── CLAUDE.md                 # 본 파일 (저장소 공통 설정)
│   └── rules/                    # 전 플러그인 공통 규칙
│       ├── claude-rules/         # 메모리/규칙 파일 작성 지침
│       ├── claude-skills/        # Skill 작성 지침
│       ├── dotnet/               # C#, WPF, AvaloniaUI, spreadsheet
│       ├── secure-coding/        # 시큐어 코딩 지침
│       └── preferences.md        # 답변 언어 등 기본 지침
├── mcp/                          # WpfDevPackMcp MCP 서버 소스 (플러그인 외부)
├── knowledge/                    # MCP 지식 토픽 (WpfDevPackMcp가 제공; 플러그인 밖이라 번들되지 않음)
├── wpf-dev-pack/                 # WPF 전용 플러그인 (현재 유일한 호스팅 대상)
├── FeedbackDocs/                 # 외부 세션 wpf-dev-pack 피드백 md 누적 폴더
├── archive-skills/               # microsoft-docs MCP로 대체되어 보관된 구 skill
└── docs/                         # 프로젝트 문서
```

## Plugin Version Update Checklist

**버전 변경은 `/wpf-dev-pack-release` 스킬을 통해서만 수행합니다.**
이 스킬이 아래 파일들을 lockstep으로 갱신합니다. 어떤 다른 워크플로우도
(피드백 반영, 새 skill 추가, hook 추가, 문서 수정 등 무엇이든) `version`
필드를 직접 손대지 않습니다. 버전 범프가 필요한 변경이면 작업을
완료한 뒤 `/wpf-dev-pack-release`를 별도로 호출하세요.

플러그인 릴리스 스킬이 갱신하는 파일:
- `<plugin>/.claude-plugin/plugin.json` — `version` 필드
- `<plugin>/README.md` — 버전 뱃지
- `<plugin>/README.ko.md` — 버전 뱃지
- `docs/changelogs/<plugin>.md` — 새 버전 항목 추가

**WpfDevPackMcp NuGet 패키지 버전 관리:** `WpfDevPackMcp` MCP 서버는 별도
NuGet 패키지로 배포되며, `wpf-dev-pack/.mcp.json`의 `dnx` 버전 지정자로
고정됩니다. 이 버전은 플러그인 버전과 독립적입니다. 지식 전용 편집
(`knowledge/` 하위 파일 수정)은 **플러그인 버전 범프 없이,
MCP 재배포 없이** 반영됩니다 — 서버는 호출마다 저장소에서 콘텐츠를 직접
읽어옵니다. 플러그인 패키지 자체에 포함된 변경(커맨드 스킬, 훅, 규칙)만이
`/wpf-dev-pack-release`를 통한 플러그인 버전 범프를 필요로 합니다.

**`WpfDevPackMcp` NuGet 배포(릴리스마다):** NuGet 버전은 불변이므로 버전을 먼저
올리고 핀은 마지막에 갱신합니다.

1. 검증: `dotnet test mcp/WpfDevPackMcp.Tests` 후 MCP Inspector로 도구 실행
   (`npx @modelcontextprotocol/inspector "<빌드된 exe>"`).
2. `mcp/WpfDevPackMcp.csproj`의 `<Version>` 올리기.
3. `dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg`
4. `dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg --source https://api.nuget.org/v3/index.json --api-key <KEY>`
5. nuget.org 인덱싱 후(`dnx WpfDevPackMcp@<ver> --yes`로 확인) `wpf-dev-pack/.mcp.json`의
   `dnx` 핀을 `@<old>` → `@<ver>` 로 갱신.

API 키는 비밀(절대 커밋 금지). 최초 1회 설정(API 키, 패키지 id 소유권)과 빌드·MCP
Inspector 사용법 등 상세는 [`mcp/README.md`](../mcp/README.md)에 정리되어 있습니다.

## 메인테이너 워크플로우

본 저장소의 메인테이너가 자주 사용하는 슬래시 명령:

| 명령 | 범위 | 용도 |
|------|------|------|
| `/applying-wpf-dev-pack-feedback <file.md>` | repo | FeedbackDocs 항목 하나를 `wpf-dev-pack`에 반영하고, 원본 md를 `FeedbackDocs/`로 이동시키며, `FeedbackDocs/APPLIED-LOG.md`에 한 줄을 추가. 커밋은 수행하지 않음. |
| `/wpf-dev-pack:configuring-wpf-dev-pack-language [code]` | plugin | `.claude/wpf-dev-pack.local.md`에 BCP-47 `language:` 필드를 작성. 다음 세션부터 발효. |
| `/wpf-dev-pack:set-repo-path <path>` | plugin | WpfDevPackMcp가 지식을 읽어올 로컬 클론 경로를 설정합니다. MCP 도구를 사용하기 전에 반드시 실행해야 합니다. |
| `/wpf-dev-pack-release` | repo | 플러그인 버전 범프의 유일한 경로. `plugin.json`, 양쪽 README 뱃지, `docs/changelogs/wpf-dev-pack.md`를 lockstep으로 갱신. 위 "Plugin Version Update Checklist" 참조. |

### 로컬 플러그인 테스트

로컬 체크아웃에 대해 작업 중인 플러그인 변경을 테스트하려면:

```
/plugin marketplace remove dotnet-claude-plugins
/plugin marketplace add <이 저장소의 절대 경로>
/plugin install wpf-dev-pack@dotnet-claude-plugins
/reload-plugins      # 이후 변경마다 실행
```

격리된 단발성 테스트 세션을 원하면:
`claude --plugin-dir <절대경로>/wpf-dev-pack` 로 실행.

## AvaloniaUI Skills

이 프로젝트에서 관리하는 AvaloniaUI 전용 스킬 목록입니다.

> **📌 참고**: WPF 관련 스킬들은 [wpf-dev-pack](./wpf-dev-pack)으로 이전되었습니다.

| Skill | 설명 |
|-------|------|
| `configuring-avalonia-dependency-injection` | AvaloniaUI DI 설정 (GenericHost) |
| `designing-avalonia-customcontrol-architecture` | AvaloniaUI CustomControl 구조 |
| `structuring-avalonia-projects` | AvaloniaUI 프로젝트 구조 설계 |
| `using-avalonia-collectionview` | DataGridCollectionView, ReactiveUI 패턴 |
| `fixing-avaloniaui-radialgradientbrush` | RadialGradientBrush 호환성 이슈 해결 |
| `converting-html-css-to-wpf-xaml` | HTML/CSS → WPF XAML 변환 |

