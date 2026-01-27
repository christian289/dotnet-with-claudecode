# dotnet-with-claudecode Project Configuration

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

---

## Skills 업데이트 이력

### 2026-01-27: wpf-dev-pack - Requirements Interview 시스템 추가

**목적:**
- 사용자의 진짜 요구사항을 파악하기 위한 AskUserQuestion 기반 인터뷰 시스템

**변경 사항:**
- `wpf-architect`, `wpf-architect-low` 에이전트에 4단계 인터뷰 프로세스 추가
- AskUserQuestion 도구 활성화

**인터뷰 스텝:**
| Step | 질문 | 라우팅 |
|------|------|--------|
| 1 | 작업 유형 | 프로젝트 생성/분석/기능구현/디버깅 |
| 2 | 아키텍처 패턴 | MVVM/Code-behind/Prism |
| 3 | 기술 수준 | 간단/균형/고급 |
| 4 | 기능 영역 | UI, 바인딩, 렌더링, 애니메이션 (다중선택) |

**Skill/Agent 매핑:**
- MVVM 선택 → `wpf-mvvm-expert`, `implementing-communitytoolkit-mvvm`
- 고급 렌더링 → `wpf-performance-optimizer`, `rendering-with-drawingcontext`
- UI/컨트롤 → `wpf-control-designer`, `authoring-wpf-controls`

---

### 2026-01-23: wpf-dev-pack v1.2.0 - 모델 티어 구조 개선

**목적:**
- Claude Pro 구독자와 Claude Max 구독자 모두 wpf-dev-pack 사용 가능하도록 개선

**에이전트 구조 (11개):**

| 에이전트 | 모델 | 대상 |
|----------|------|------|
| wpf-architect | Opus | Claude Max |
| wpf-architect-low | Sonnet | Claude Pro |
| wpf-code-reviewer | Opus | Claude Max |
| wpf-code-reviewer-low | Sonnet | Claude Pro |
| wpf-control-designer | Sonnet | 모두 |
| wpf-xaml-designer | Sonnet | 모두 |
| wpf-mvvm-expert | Sonnet | 모두 |
| wpf-data-binding-expert | Sonnet | 모두 |
| wpf-performance-optimizer | Sonnet | 모두 |
| code-formatter | Haiku | 모두 |
| serena-initializer | Haiku | 모두 |

**Claude Pro 사용자 안내:**
- 대부분의 에이전트는 Sonnet이므로 그대로 사용
- `wpf-architect`와 `wpf-code-reviewer`만 `-low` 버전 사용

---

### 2026-01-22: WPF 스킬 wpf-dev-pack으로 이전

**변경 사항:**
- WPF 관련 49개 스킬을 `wpf-dev-pack/skills`로 이전
- `.claude/skills`에는 AvaloniaUI 전용 스킬만 유지
- WPF Skills 검토 대기 항목은 유지 (wpf-dev-pack 추가 후보)

**남은 스킬 (AvaloniaUI 전용):**
- configuring-avalonia-dependency-injection
- converting-html-css-to-wpf-xaml
- designing-avalonia-customcontrol-architecture
- fixing-avaloniaui-radialgradientbrush
- structuring-avalonia-projects
- using-avalonia-collectionview

---

### 2026-01-22: configuring-dependency-injection 스킬 개선

**templates → scripts 마이그레이션:**
- `templates` 폴더를 `scripts` 폴더로 변경
- 정적 템플릿 파일 대신 `Create-WpfDISample.ps1` PowerShell 스크립트 생성
- .NET CLI 명령어로 프로젝트 구조 자동 생성 (dotnet new, dotnet add package 등)
- `dotnet sln migrate`로 .sln을 .slnx 형식으로 변환

**SKILL.md 개선:**
- 스크립트 없이도 프로젝트 구조 파악 가능하도록 모든 파일 내용 포함
- 생성되는 프로젝트 구조 다이어그램 추가
- 각 파일별 코드 예시 완비 (csproj, xaml, cs 파일 전체)

**요구사항:**
- .NET 10 SDK 필요

---

### 2026-01-21: WPF-Samples 기반 Skills 정비

**신규 생성:**
- `handling-wpf-input-commands` - RoutedCommand, ICommand, CommandBinding, InputBinding
- `routing-wpf-events` - Bubbling/Tunneling, PreviewXxx, RoutedEventArgs
- `implementing-wpf-dragdrop` - DragDrop.DoDragDrop, DataFormats, DragEventArgs
- `defining-wpf-dependencyproperty` - DependencyProperty.Register, PropertyMetadata, Callbacks
- `creating-wpf-flowdocument` - FlowDocument, Paragraph, FixedDocument
- `managing-wpf-application-lifecycle` - Startup, Shutdown, UnhandledException
- `creating-wpf-dialogs` - Window.ShowDialog, MessageBox, CommonDialog
- `implementing-wpf-automation` - UI Automation, AutomationPeer
- `localizing-wpf-applications` - x:Uid, BAML Localization, FlowDirection
- `using-wpf-clipboard` - Clipboard.SetText/GetText, DataFormats

**기존 Skill 보강:**
- `managing-wpf-collectionview-mvvm` - Grouping UI XAML 예제, Expander 스타일 그룹화, 복합 Sort+Group 패턴 추가
- `mapping-viewmodel-view-datatemplate` - HierarchicalDataTemplate for TreeView 패턴 추가
- `managing-styles-resourcedictionary` - 이미 DynamicResource 테마 전환 패턴 포함 (추가 불필요)
- `integrating-wpf-media` - 이미 MediaElement 상세 패턴 포함 (추가 불필요)

**참고 자료:**
- 원본: https://github.com/microsoft/WPF-Samples
