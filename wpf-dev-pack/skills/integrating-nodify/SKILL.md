---
description: "Integrates Nodify library for building node-based editors in WPF with MVVM. Use when creating visual scripting, workflow editors, state machines, or any node graph UI with NodifyEditor, Node, Connector, and Connection controls. Covers EditorGestures configuration, multi-selection with CanSelectMultipleItems, SelectedItems null pitfall, keyboard event handling via Window PreviewKeyDown Behavior pattern, and ItemContainerGenerator-based selection state reading."
user-invocable: false
model: sonnet
---

# Nodify Node-Based Editor Guide

Nodify 7.x 기반 노드 에디터 구현 가이드.

## NuGet Package

```xml
<PackageReference Include="Nodify" Version="7.2.*" />
```

- .NET 8+ / .NET Framework 4.7.2+ 호환
- WPF 외 추가 의존성 없음
- MVVM 전용 설계 (데이터 바인딩 기반)

## 1. XAML Namespace

```xml
xmlns:nodify="https://miroiu.github.io/nodify"
```

## 2. Core Controls

| Control | 용도 |
|---------|------|
| `NodifyEditor` | 메인 캔버스 (줌, 팬, 선택) |
| `Node` | 표준 노드 (Header, Input, Output) |
| `GroupingNode` | 노드 그룹화 |
| `KnotNode` | 연결선 경유점 |
| `StateNode` | 상태 머신용 노드 |
| `NodeInput` / `NodeOutput` | 입출력 커넥터 |
| `Connector` | 연결 포인트 |
| `Connection` | 베지어 커브 연결선 |
| `LineConnection` | 직선 연결 |
| `CircuitConnection` | 직각 연결 (회로 스타일) |
| `StepConnection` | 계단형 연결 |
| `PendingConnection` | 드래그 중 연결 미리보기 |
| `Minimap` | 전체 뷰 미니맵 |

## 3. ViewModel 구조

### ConnectorViewModel

```csharp
namespace MyApp.ViewModels;

public sealed partial class ConnectorViewModel : ObservableObject
{
    [ObservableProperty] private Point _anchor;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _title = string.Empty;
}
```

### NodeViewModel

```csharp
namespace MyApp.ViewModels;

public sealed partial class NodeViewModel : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private Point _location;

    public ObservableCollection<ConnectorViewModel> Input { get; } = [];
    public ObservableCollection<ConnectorViewModel> Output { get; } = [];
}
```

### ConnectionViewModel

```csharp
namespace MyApp.ViewModels;

public sealed class ConnectionViewModel(ConnectorViewModel source, ConnectorViewModel target)
{
    public ConnectorViewModel Source { get; } = source;
    public ConnectorViewModel Target { get; } = target;

    public void SetConnected(bool value)
    {
        Source.IsConnected = value;
        Target.IsConnected = value;
    }
}
```

### PendingConnectionViewModel

```csharp
namespace MyApp.ViewModels;

public sealed class PendingConnectionViewModel
{
    private readonly EditorViewModel _editor;
    private ConnectorViewModel? _source;

    public PendingConnectionViewModel(EditorViewModel editor)
    {
        _editor = editor;
        StartCommand = new RelayCommand<ConnectorViewModel>(source => _source = source);
        FinishCommand = new RelayCommand<ConnectorViewModel>(target =>
        {
            if (target is not null && _source is not null)
                _editor.Connect(_source, target);
        });
    }

    public IRelayCommand<ConnectorViewModel> StartCommand { get; }
    public IRelayCommand<ConnectorViewModel> FinishCommand { get; }
}
```

### EditorViewModel

```csharp
namespace MyApp.ViewModels;

public sealed partial class EditorViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = [];
    public ObservableCollection<ConnectionViewModel> Connections { get; } = [];
    public PendingConnectionViewModel PendingConnection { get; }

    public EditorViewModel()
    {
        PendingConnection = new PendingConnectionViewModel(this);
        DisconnectConnectorCommand = new RelayCommand<ConnectorViewModel>(Disconnect);
    }

    public IRelayCommand<ConnectorViewModel> DisconnectConnectorCommand { get; }

    public void Connect(ConnectorViewModel source, ConnectorViewModel target)
    {
        var connection = new ConnectionViewModel(source, target);
        connection.SetConnected(true);
        Connections.Add(connection);
    }

    private void Disconnect(ConnectorViewModel? connector)
    {
        if (connector is null) return;

        var connection = Connections.FirstOrDefault(
            c => c.Source == connector || c.Target == connector);

        if (connection is null) return;

        connection.SetConnected(false);
        Connections.Remove(connection);
    }
}
```

## 4. XAML Setup

### NodifyEditor (Complete)

```xml
<nodify:NodifyEditor ItemsSource="{Binding Nodes}"
                     Connections="{Binding Connections}"
                     PendingConnection="{Binding PendingConnection}"
                     DisconnectConnectorCommand="{Binding DisconnectConnectorCommand}">

    <!-- 노드 위치 바인딩 -->
    <!-- Node position binding -->
    <nodify:NodifyEditor.ItemContainerStyle>
        <Style TargetType="{x:Type nodify:ItemContainer}">
            <Setter Property="Location" Value="{Binding Location}" />
        </Style>
    </nodify:NodifyEditor.ItemContainerStyle>

    <!-- 노드 템플릿 -->
    <!-- Node template -->
    <nodify:NodifyEditor.ItemTemplate>
        <DataTemplate DataType="{x:Type local:NodeViewModel}">
            <nodify:Node Header="{Binding Title}"
                         Input="{Binding Input}"
                         Output="{Binding Output}">
                <nodify:Node.InputConnectorTemplate>
                    <DataTemplate DataType="{x:Type local:ConnectorViewModel}">
                        <nodify:NodeInput Header="{Binding Title}"
                                          IsConnected="{Binding IsConnected}"
                                          Anchor="{Binding Anchor, Mode=OneWayToSource}" />
                    </DataTemplate>
                </nodify:Node.InputConnectorTemplate>
                <nodify:Node.OutputConnectorTemplate>
                    <DataTemplate DataType="{x:Type local:ConnectorViewModel}">
                        <nodify:NodeOutput Header="{Binding Title}"
                                           IsConnected="{Binding IsConnected}"
                                           Anchor="{Binding Anchor, Mode=OneWayToSource}" />
                    </DataTemplate>
                </nodify:Node.OutputConnectorTemplate>
            </nodify:Node>
        </DataTemplate>
    </nodify:NodifyEditor.ItemTemplate>

    <!-- 연결선 템플릿 -->
    <!-- Connection template -->
    <nodify:NodifyEditor.ConnectionTemplate>
        <DataTemplate DataType="{x:Type local:ConnectionViewModel}">
            <nodify:LineConnection Source="{Binding Source.Anchor}"
                                   Target="{Binding Target.Anchor}" />
        </DataTemplate>
    </nodify:NodifyEditor.ConnectionTemplate>

</nodify:NodifyEditor>
```

## 5. Connection Styles

| Type | XAML | 설명 |
|------|------|------|
| Bezier | `<nodify:Connection>` | 기본 베지어 커브 |
| Line | `<nodify:LineConnection>` | 직선 |
| Circuit | `<nodify:CircuitConnection>` | 직각 (회로 스타일) |
| Step | `<nodify:StepConnection>` | 계단형 |

```xml
<!-- 연결 스타일만 교체하면 됨 -->
<!-- Just swap the connection style -->
<nodify:CircuitConnection Source="{Binding Source.Anchor}"
                           Target="{Binding Target.Anchor}" />
```

## 6. EditorGestures Configuration

`EditorGestures.Mappings`으로 마우스/키보드 제스처를 커스터마이징:

```csharp
// App 초기화 시 (App.xaml.cs 등)
using Nodify.Interactivity;

// 줌: Ctrl + 마우스 휠
EditorGestures.Mappings.Editor.ZoomModifierKey = ModifierKeys.Control;

// 패닝: Ctrl + 좌클릭 (기본값: 우클릭)
EditorGestures.Mappings.Editor.Pan.Value = new MouseGesture(MouseAction.LeftClick, ModifierKeys.Control);

// 선택 비활성화 (드래그 사각형 선택 끄기)
EditorGestures.Mappings.Editor.Selection.Apply(EditorGestures.SelectionGestures.None);

// 아이템 컨테이너 선택 제스처 (기본값)
// Replace: LeftClick           → 클릭한 노드만 선택
// Append:  Shift+LeftClick     → 기존 선택에 추가
// Invert:  Ctrl+LeftClick      → 선택 반전
// Remove:  Alt+LeftClick       → 선택에서 제거

// 개별 해제 가능:
EditorGestures.Mappings.ItemContainer.Selection.Append.Unbind();
EditorGestures.Mappings.ItemContainer.CancelAction.Unbind();
```

**주의:** Pan 제스처를 `Ctrl+LeftClick`으로 설정하면 기본 `Invert` 선택 제스처와 충돌. 필요 시 `Invert`를 Unbind하거나 다른 조합으로 변경.

## 7. Multi-Selection

### XAML 설정

```xml
<nodify:NodifyEditor CanSelectMultipleItems="True"
                     SelectedItem="{Binding SelectedNode}"
                     ... />
```

### SelectedItems는 null — 사용 금지

`NodifyEditor.SelectedItems`는 별도 초기화 없이 **null을 반환**. 직접 접근하면 `NullReferenceException` 또는 `ArgumentNullException` 발생:

```csharp
// BAD — NullReferenceException
var count = editor.SelectedItems.Count;

// BAD — ArgumentNullException (LINQ)
var nodes = editor.SelectedItems.OfType<INodeItem>().ToList();
```

### 선택 상태 읽기 — ItemContainerGenerator 사용

```csharp
private List<INodeItem> GetSelectedNodes(NodifyEditor editor)
{
    var selectedNodes = new List<INodeItem>();

    for (int i = 0; i < editor.Items.Count; i++)
    {
        if (editor.Items[i] is not INodeItem node)
        {
            continue;
        }

        var container = editor.ItemContainerGenerator.ContainerFromIndex(i) as ItemContainer;

        if (container is { IsSelected: true })
        {
            selectedNodes.Add(node);
        }
    }

    // 다중 선택이 없으면 SelectedItem 폴백
    if (selectedNodes.Count <= 0 && editor.SelectedItem is INodeItem selectedNode)
    {
        selectedNodes.Add(selectedNode);
    }

    return selectedNodes;
}
```

## 8. Keyboard Event Handling (Delete Key Pattern)

NodifyEditor는 `OnKeyDown`을 `InputProcessor`에 위임하여 키보드 네비게이션 등에 사용. Delete 키를 커스텀 처리하려면 NodifyEditor보다 먼저 이벤트를 가로채야 함.

**문제:** NodifyEditor는 기본적으로 키보드 포커스를 받지 않으므로, Editor에 직접 `PreviewKeyDown`을 걸어도 동작하지 않음.

**해결:** Behavior에서 부모 Window의 `PreviewKeyDown`을 구독:

```csharp
public class EditorDeleteBehavior : Behavior<NodifyEditor>
{
    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand),
            typeof(EditorDeleteBehavior));

    public ICommand? DeleteCommand { get; set; }

    private Window? _parentWindow;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _parentWindow = Window.GetWindow(AssociatedObject);
        if (_parentWindow is not null)
        {
            _parentWindow.PreviewKeyDown += OnWindowPreviewKeyDown;
        }
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;

        // 텍스트 입력 중이면 무시
        if (Keyboard.FocusedElement is TextBox or TextBoxBase or PasswordBox) return;

        // Editor 영역에서만 처리
        if (!AssociatedObject.IsMouseOver && !AssociatedObject.IsKeyboardFocusWithin) return;

        var selectedNodes = GetSelectedNodes(AssociatedObject);
        if (selectedNodes.Count > 0 && DeleteCommand?.CanExecute(selectedNodes) == true)
        {
            DeleteCommand.Execute(selectedNodes);
            e.Handled = true;
        }
    }
    // ... OnUnloaded에서 구독 해제
}
```

**스코프 가드 조건:**
- `IsMouseOver` — 마우스가 Editor 위에 있으면 처리 (키보드 포커스 없어도)
- `IsKeyboardFocusWithin` — Editor 내부에 포커스가 있으면 처리
- `TextBox` 체크 — 검색창 등에서 Delete 키가 텍스트 삭제로 동작하도록

## 9. Common Mistakes

| 실수 | 올바른 방법 |
|------|------------|
| `Anchor` 바인딩 누락 | `Anchor="{Binding Anchor, Mode=OneWayToSource}"` 필수 |
| `Location` 바인딩 누락 | `ItemContainerStyle`에서 `Location` 바인딩 |
| ViewModel에 `Point` 사용 | `System.Windows.Point`는 WPF 타입 — ViewModel 분리 시 주의 |
| `PendingConnection` 미설정 | 드래그로 연결 생성이 동작하지 않음 |
| `IsConnected` 미갱신 | 연결 추가/제거 시 양쪽 커넥터의 `IsConnected` 반드시 갱신 |
| Showing Disconnect on a disconnected socket | In the right-click handler, check `connector.IsConnected`. For disconnected sockets, fall back to the node-level menu (Delete etc.). Avoids the illogical UX of offering Disconnect on a socket that has nothing to disconnect. |
| Guessing gesture defaults without consulting `Nodify.xml` | Grep `{NuGet}\nodify\{ver}\lib\net472\Nodify.xml` directly — all `SelectionGestures`/`EditorGestures` defaults and `ModifierKeys` combinations are specified in the official XML doc comments. |

> ⚠️ `System.Windows.Point`는 `WindowsBase.dll` 소속. ViewModel 프로젝트를 순수 BCL로 유지하려면 `double X, Y` 프로퍼티로 분리하고 Converter에서 변환.

### Disconnect Menu Branching Example

```csharp
private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
{
    var (node, connector) = FindNodeAndConnector(e.OriginalSource as DependencyObject, AssociatedObject);
    if (node == null) return;

    var menu = new ContextMenu();

    // Only show Disconnect when the socket is actually connected.
    // For disconnected sockets, fall back to the node-level menu (Delete / SimpleView / AddInput etc.).
    if (connector is { IsConnected: true } && DisconnectConnectorCommand != null)
    {
        menu.Items.Add(new MenuItem
        {
            Header = "Disconnect",
            Command = DisconnectConnectorCommand,
            CommandParameter = connector
        });
    }
    else
    {
        // Node-level menu items: Delete, ToggleSimpleView, AddInputSocket, etc.
    }

    if (menu.Items.Count > 0)
    {
        menu.IsOpen = true;
        e.Handled = true;
    }
}
```

## Key Rules

- `NodifyEditor`에 `ItemsSource`, `Connections`, `PendingConnection` 3개 필수 바인딩
- `ItemContainerStyle`로 노드 `Location` 바인딩
- `Anchor`는 `Mode=OneWayToSource` (Nodify가 좌표를 계산하여 ViewModel에 전달)
- 연결 추가/제거 시 `IsConnected` 양쪽 갱신
- Connection 스타일은 `ConnectionTemplate`에서 컨트롤만 교체
- MVVM 순수성이 필요하면 `Point` 대신 `double X, Y` 사용

## 참고

- [Nodify GitHub](https://github.com/miroiu/nodify)
- [Nodify Wiki](https://github.com/miroiu/nodify/wiki)
- [NuGet - Nodify](https://www.nuget.org/packages/Nodify)
