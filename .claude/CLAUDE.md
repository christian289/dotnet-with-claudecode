# dotnet-with-claudecode Project Configuration

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
