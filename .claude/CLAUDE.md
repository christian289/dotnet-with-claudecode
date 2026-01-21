# dotnet-with-claudecode Project Configuration

## Skills with Templates

실행 가능한 코드 예시가 포함된 skill 목록입니다.

> **📌 마이그레이션 계획**: `templates` 폴더 방식에서 `scripts` 폴더 방식으로 전환 예정입니다.
> - 기존: 정적 템플릿 파일 제공
> - 변경: PowerShell 스크립트로 .NET CLI 명령어를 통해 프로젝트 구조 생성
> - 완료: `configuring-dependency-injection` (2026-01-22)

| Skill | Template 프로젝트 | 설명 | 상태 |
|-------|------------------|------|------|
| `configuring-dependency-injection` | WpfDISample | WPF DI 설정 | ✅ scripts 전환 완료 |
| `configuring-avalonia-dependency-injection` | AvaloniaDISample | AvaloniaUI DI 설정 | 📋 전환 예정 |
| `designing-avalonia-customcontrol-architecture` | AvaloniaCustomControlSample | AvaloniaUI CustomControl 구조 | 📋 전환 예정 |
| `designing-wpf-customcontrol-architecture` | WpfCustomControlSample | WPF CustomControl 구조 | 📋 전환 예정 |
| `implementing-communitytoolkit-mvvm` | WpfMvvmSample | CommunityToolkit.Mvvm 패턴 | 📋 전환 예정 |
| `managing-literal-strings` | LiteralStringSample | const string 관리 | 📋 전환 예정 |
| `managing-wpf-collectionview-mvvm` | WpfCollectionViewSample | CollectionView MVVM 캡슐화 | 📋 전환 예정 |
| `managing-wpf-popup-focus` | WpfPopupSample | Popup 포커스 관리 | 📋 전환 예정 |
| `mapping-viewmodel-view-datatemplate` | WpfDataTemplateSample | ViewModel-View DataTemplate 매핑 | 📋 전환 예정 |
| `rendering-with-drawingcontext` | DrawingContextSample | DrawingContext 고성능 렌더링 | 📋 전환 예정 |

---

## WPF Skills 관리

### 검토 대기 항목 (WPF-Samples 기반)

아래 항목들은 WPF-Samples와 비교하여 Skill 추가 여부를 검토해야 합니다.

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
