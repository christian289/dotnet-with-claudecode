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

## WPF Skills 검토 대기 항목

아래 항목들은 WPF-Samples와 비교하여 wpf-dev-pack에 Skill 추가 여부를 검토해야 합니다.

| WPF-Samples 폴더/샘플 | 고려사항 | 결정 |
|----------------------|----------|------|
| **PerMonitorDPI** | DPI-Aware 설정은 프로젝트 설정 수준. Skill로 만들 가치 있을지? | 미정 |
| **Migration and Interoperability** (WindowsFormsHost, HwndHost) | Win32/WinForms 통합은 레거시 시나리오. 수요 있을지? | 미정 |
| **Compatibility** (.NET Framework → .NET 마이그레이션) | 버전별 차이 문서화 가치 있을지? | 미정 |
| **Data Binding** → ADODataSet, XmlDataSource | XML/ADO 바인딩은 현대 앱에서 드묾. 필요할지? | 미정 |
| **Data Binding** → PriorityBinding, MultiBinding | 고급 바인딩 시나리오. 별도 Skill vs 기존에 통합? | 미정 |
| **Elements** → FocusVisualStyle, VisibiltyChanges | 작은 주제. 독립 Skill vs 다른 Skill에 통합? | 미정 |

---

## Skills 업데이트 이력

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
