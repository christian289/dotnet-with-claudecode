# dotnet-with-claudecode Project Configuration

## Repository Overview

이 저장소는 .NET 관련 Claude Code 플러그인을 호스팅하는 **마켓플레이스 저장소**입니다.
- 저장소 루트의 `.claude/rules`는 모든 플러그인에 공통 적용되는 범용 규칙만 포함
- 특정 플러그인에 편향된 설정(hooks, permissions 등)은 각 플러그인 내부에서 관리
- 현재 호스팅 중인 플러그인: `wpf-dev-pack` (단일)

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
├── wpf-dev-pack/                 # WPF 전용 플러그인 (현재 유일한 호스팅 대상)
├── archive-skills/               # microsoft-docs MCP로 대체되어 보관된 구 skill
└── docs/                         # 프로젝트 문서
```

## Plugin Version Update Checklist

플러그인 버전 업데이트 시 반드시 수정해야 하는 파일:
- `<plugin>/.claude-plugin/plugin.json` — `version` 필드
- `<plugin>/README.md` — 버전 뱃지
- `<plugin>/README.ko.md` — 버전 뱃지

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

