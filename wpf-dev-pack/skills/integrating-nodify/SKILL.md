---
description: "Integrates Nodify library for building node-based editors in WPF with MVVM. Use when creating visual scripting, workflow editors, state machines, or any node graph UI with NodifyEditor, Node, Connector, and Connection controls."
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

## 6. Common Mistakes

| 실수 | 올바른 방법 |
|------|------------|
| `Anchor` 바인딩 누락 | `Anchor="{Binding Anchor, Mode=OneWayToSource}"` 필수 |
| `Location` 바인딩 누락 | `ItemContainerStyle`에서 `Location` 바인딩 |
| ViewModel에 `Point` 사용 | `System.Windows.Point`는 WPF 타입 — ViewModel 분리 시 주의 |
| `PendingConnection` 미설정 | 드래그로 연결 생성이 동작하지 않음 |
| `IsConnected` 미갱신 | 연결 추가/제거 시 양쪽 커넥터의 `IsConnected` 반드시 갱신 |

> ⚠️ `System.Windows.Point`는 `WindowsBase.dll` 소속. ViewModel 프로젝트를 순수 BCL로 유지하려면 `double X, Y` 프로퍼티로 분리하고 Converter에서 변환.

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
