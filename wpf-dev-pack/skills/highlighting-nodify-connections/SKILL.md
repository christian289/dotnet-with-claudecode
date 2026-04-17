---
description: >
  Implements hover-driven visual highlight (neon glow) that propagates from a Nodify socket to its connected line and the peer socket on the other side of the connection. Covers the weak-event subscription pattern between `ConnectorItem` and `ConnectionItem`, peer-count propagation for 1:N fan-out, the `IsMouseOver` gotcha inside Nodify's `NodeInput`/`NodeOutput` `ControlTemplate` (where a named `Highlight` border is toggled by an internal trigger that external styles cannot reach), and the required local `ControlTemplate` override that replaces `IsMouseOver` with a data-driven `IsHighlighted` trigger so both endpoints render identically.

  Use When:
    - Implementing hover/highlight effect on Nodify `NodeInput` / `NodeOutput` sockets and their connected lines
    - The hovered socket and the peer socket (opposite end of the connection) must render IDENTICAL visuals
    - Neon / glow effects need to follow a connection from one socket to another
    - Working with Nodify 7.x `Connector` / `Connection` / `NodeInput` / `NodeOutput` controls
    - User reports asymmetry between hover-side and peer-side socket visuals
    - User reports connection line has no glow while sockets do (local-value vs Style.Trigger precedence bug)
    - Stacking saturation when 1 output is connected to N inputs (multiple `DropShadowEffect` instances overlap and saturate)
    - Need to push XAML mouse events back to DataContext without code-behind ("i:ChangePropertyAction TargetObject='{Binding}'" pattern)

  Do NOT Use When:
    - Only the directly hovered socket needs to change (no peer propagation) — use a simple `Style.Trigger` on `IsMouseOver` inside the connector's `DataTemplate`
    - Highlight state is computed from domain logic, not hover (execution state, error state, selection state — use a separate `DataTrigger` on the relevant ViewModel property)
    - Replacing the entire node visual (not just socket/connection highlight) — use the `designing-wpf-customcontrol-architecture` skill
    - Not using Nodify — use `customizing-controltemplate` skill instead
user-invocable: false
model: sonnet
---

# Highlighting Nodify Connections

Nodify 소켓에 마우스를 hover하면 그 소켓 + 연결된 line + 반대편 소켓이 **동일한** 네온 효과로 강조되도록 구현하는 패턴. 단일 connection 단위로 효과가 전파되며, peer 카운트로 1:N fan-out 상황에서도 정확하게 동작한다.

## Core Principles

1. **Model이 시각을 주도** — Hover 상태는 `ConnectorItem.IsHovered`(mouse 입력으로 설정), 강조 상태는 `ConnectorItem.IsHighlighted`(파생, peer 카운트 포함). View는 `IsHighlighted`만 바인딩.
2. **WeakEvent로 구독** — `ConnectionItem`이 양쪽 endpoint의 `IsHovered` 변경을 `PropertyChangedEventManager.AddHandler`로 구독. `ConnectorItem`이 `ConnectionItem`보다 오래 살 수 있어 강한 참조 누수 방지.
3. **Idempotent 라이프사이클** — Connect/Disconnect/Restore(Undo)에서 구독이 중복되지 않도록 `_isAttached` 플래그로 가드.
4. **Template 내부 Trigger는 외부 Style로 override 불가** — Nodify의 `NodeInput`/`NodeOutput`은 ControlTemplate 안에 `Highlight` named Border를 갖고 `IsMouseOver` Trigger로 토글한다. 외부 `Style.Trigger`의 Setter는 `TargetName`을 지정할 수 없으므로 peer 쪽에 같은 효과를 내려면 ControlTemplate 자체를 로컬에 복사해서 override한다.
5. **Local value precedence** — XAML element의 인라인 attribute는 `Style.Trigger`의 Setter보다 우선순위가 높다. Trigger로 override하려는 속성은 기본값도 반드시 `<Style>`의 Setter로 선언.
6. **Freezable 리소스 재사용** — 여러 곳에서 적용되는 `DropShadowEffect`는 리소스로 선언 + `PresentationOptions:Freeze="True"`로 GC 압력 감소.

## Architecture Overview

```
[mouse enter NodeInput]
  → i:ChangePropertyAction → ConnectorItem.IsHovered = true
  → ConnectorItem.IsHighlighted PropertyChanged (own glow)
  → ConnectionItem (weak-event subscriber) observes IsHovered change
     → IsHighlighted = true
     → peer.IncrementPeerHighlight() → peer.IsHighlighted = true (peer glow)
     → ConnectionItem.IsHighlighted PropertyChanged → MidpointConnection style changes (line glow)
```

## Workflow

### Step 1 — ConnectorItem: IsHovered + IsHighlighted + peer count

Prism `BindableBase` 기반 모델. `IsConnected` setter는 `IsHighlighted` 갱신도 함께 발행한다 (unconnected 소켓은 hover해도 강조하지 않는 규칙 적용 시 필수).

```csharp
public class ConnectorItem : BindableBase
{
    private bool _isHovered;
    public bool IsHovered
    {
        get => _isHovered;
        set
        {
            if (SetProperty(ref _isHovered, value))
            {
                RaisePropertyChanged(nameof(IsHighlighted));
            }
        }
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
            {
                RaisePropertyChanged(nameof(IsHighlighted));
            }
        }
    }

    private int _peerHighlightCount;
    // 연결된 소켓만 hover 시 강조. peer 카운트는 연결이 있어야만 증가하므로 IsConnected 체크 불필요
    public bool IsHighlighted => (IsHovered && IsConnected) || _peerHighlightCount > 0;

    internal void IncrementPeerHighlight()
    {
        _peerHighlightCount++;
        if (_peerHighlightCount == 1)
        {
            RaisePropertyChanged(nameof(IsHighlighted));
        }
    }

    internal void DecrementPeerHighlight()
    {
        // Underflow 가드 — OnDetached가 중복 호출되거나 순서가 꼬여도 카운트가 음수로 가지 않도록
        if (_peerHighlightCount <= 0)
        {
            return;
        }
        _peerHighlightCount--;
        if (_peerHighlightCount == 0)
        {
            RaisePropertyChanged(nameof(IsHighlighted));
        }
    }
}
```

### Step 2 — ConnectionItem: WeakEvent 구독 + idempotent 라이프사이클

```csharp
public class ConnectionItem : BindableBase
{
    public ConnectorItem Source { get; }
    public ConnectorItem Target { get; }

    private bool _isAttached;

    private bool _isHighlighted;
    public bool IsHighlighted
    {
        get => _isHighlighted;
        private set
        {
            if (_isHighlighted == value)
            {
                return;
            }
            _isHighlighted = value;
            if (value)
            {
                Source.IncrementPeerHighlight();
                Target.IncrementPeerHighlight();
            }
            else
            {
                Source.DecrementPeerHighlight();
                Target.DecrementPeerHighlight();
            }
            RaisePropertyChanged();
        }
    }

    public ConnectionItem(ConnectorItem source, ConnectorItem target)
    {
        Source = source;
        Source.IsConnected = true;
        Target = target;
        Target.IsConnected = true;

        // Weak event — ConnectorItem은 ConnectionItem보다 오래 살 수 있으므로 강한 참조 회피
        PropertyChangedEventManager.AddHandler(Source, OnEndpointHoverChanged, nameof(ConnectorItem.IsHovered));
        PropertyChangedEventManager.AddHandler(Target, OnEndpointHoverChanged, nameof(ConnectorItem.IsHovered));
        _isAttached = true;

        // 생성 시점에 이미 hover된 상태라면 즉시 강조
        SyncHighlight();
    }

    // DesignerCanvasService.Disconnect에서 호출
    public void OnDetached()
    {
        if (!_isAttached)
        {
            return;
        }
        IsHighlighted = false;
        PropertyChangedEventManager.RemoveHandler(Source, OnEndpointHoverChanged, nameof(ConnectorItem.IsHovered));
        PropertyChangedEventManager.RemoveHandler(Target, OnEndpointHoverChanged, nameof(ConnectorItem.IsHovered));
        _isAttached = false;
    }

    // Undo로 연결 복원 시 호출
    public void OnAttached()
    {
        if (_isAttached)
        {
            return;
        }
        PropertyChangedEventManager.AddHandler(Source, OnEndpointHoverChanged, nameof(ConnectorItem.IsHovered));
        PropertyChangedEventManager.AddHandler(Target, OnEndpointHoverChanged, nameof(ConnectorItem.IsHovered));
        _isAttached = true;
        SyncHighlight();
    }

    private void OnEndpointHoverChanged(object? sender, PropertyChangedEventArgs e) => SyncHighlight();

    private void SyncHighlight() => IsHighlighted = Source.IsHovered || Target.IsHovered;
}
```

### Step 3 — Canvas 서비스에서 라이프사이클 호출

모든 연결 추가/제거 경로가 한 funnel 메서드를 통과하게 만들고 그곳에서 호출. 예: `DesignerCanvasService.Disconnect(ConnectionItem)` / `RestoreConnection(ConnectionItem)`.

```csharp
public void Disconnect(ConnectionItem connection)
{
    // ... 기존 IsConnected 재계산 + Connections.Remove ...
    Connections.Remove(connection);
    connection.OnDetached();   // ← peer 카운트 정리 + 구독 해제
    NotifyConnectionChanged(connection.Source, connection.Target);
}

public void RestoreConnection(ConnectionItem connection)
{
    // ... 기존 IsConnected=true 설정 + Connections.Add ...
    Connections.Add(connection);
    connection.OnAttached();   // ← 구독 재개 + 현재 hover 상태 반영
    NotifyConnectionChanged(connection.Source, connection.Target);
}
```

### Step 4 — XAML: 마우스 이벤트를 ViewModel로 push

`Microsoft.Xaml.Behaviors`의 `i:ChangePropertyAction`으로 NodeInput/NodeOutput DataTemplate에서 `ConnectorItem.IsHovered`에 직접 쓴다. 코드비하인드 불필요.

```xml
<DataTemplate DataType="{x:Type viewModels:ConnectorItem}">
    <nodify:NodeInput Header="{Binding Header}"
                      IsConnected="{Binding IsConnected}"
                      Anchor="{Binding PixelPosition, Mode=OneWayToSource}">
        <i:Interaction.Triggers>
            <i:EventTrigger EventName="MouseEnter">
                <i:ChangePropertyAction TargetObject="{Binding}"
                                        PropertyName="IsHovered"
                                        Value="True"/>
            </i:EventTrigger>
            <i:EventTrigger EventName="MouseLeave">
                <i:ChangePropertyAction TargetObject="{Binding}"
                                        PropertyName="IsHovered"
                                        Value="False"/>
            </i:EventTrigger>
        </i:Interaction.Triggers>
        <!-- Style은 Step 5 참조 -->
    </nodify:NodeInput>
</DataTemplate>
```

### Step 5 — 글로우 색상/효과 리소스 (Freezable 공유)

테마에 종속된 액센트 컬러(`SystemAccentColorPrimaryBrush`)는 네온이 아닌 그냥 "강조"로 보여 효과가 약하다. **고정 색상**(예: 민트 `#00FFB7`) 리소스로 선언.

```xml
<coreUIViews:View.Resources>
    <!-- PresentationOptions: namespace 선언 필수 -->
    <!-- xmlns:PresentationOptions="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options" -->

    <Color x:Key="NeonGlowColor">#00FFB7</Color>

    <SolidColorBrush x:Key="NeonGlowBrush"
                     Color="{StaticResource NeonGlowColor}"
                     PresentationOptions:Freeze="True"/>

    <!-- outline용 반투명 (라인 halo) -->
    <SolidColorBrush x:Key="NeonGlowOutlineBrush"
                     Color="#5500FFB7"
                     PresentationOptions:Freeze="True"/>

    <!-- 소켓 글로우 (개별 소켓에 적용되므로 stacking 없음 — 강도 자유) -->
    <DropShadowEffect x:Key="SocketGlowEffect"
                      Color="{StaticResource NeonGlowColor}"
                      BlurRadius="12"
                      ShadowDepth="0"
                      Opacity="0.7"
                      PresentationOptions:Freeze="True"/>

    <!-- 라인 글로우 (1:N fan-out 시 6개 라인이 한 점에 수렴 → Opacity 낮게) -->
    <DropShadowEffect x:Key="ConnectionGlowEffect"
                      Color="{StaticResource NeonGlowColor}"
                      BlurRadius="10"
                      ShadowDepth="0"
                      Opacity="0.3"
                      PresentationOptions:Freeze="True"/>
</coreUIViews:View.Resources>
```

**Stacking rule:** 하나의 소켓에 여러 연결이 수렴할 수 있는 쪽(주로 output→N inputs)에서 `DropShadowEffect`의 `Opacity`를 낮게 잡는다. `Opacity=1.0`은 2개 이상 겹칠 때 바로 포화되어 텍스트가 읽히지 않는다.

### Step 6 — NodeInput/NodeOutput: ControlTemplate 로컬 override (핵심)

**문제:** Nodify 7.x의 기본 `ControlTemplate`은 내부에 `<Border x:Name="Highlight">` + `<Trigger Property="IsMouseOver" Value="True"> <Setter TargetName="Highlight" ... /> </Trigger>`를 가진다. 외부 `Style.Trigger`는 `TargetName`을 지정할 수 없어 template 내부 파트에 접근 불가. 결과적으로 mouse-over 쪽은 `Highlight`가 보이지만 peer 쪽은 보이지 않아 **시각이 비대칭**.

**해결:** Nodify의 `ControlTemplate`을 로컬에 복사해 `IsMouseOver` Trigger를 `IsHighlighted` DataTrigger로 교체. DataTrigger는 template 내부에서도 `TargetName`을 지정할 수 있다.

Nodify 원본 템플릿 참조: `https://github.com/miroiu/nodify/blob/v{version}/Nodify/Themes/Styles/NodeInput.xaml` (NodeOutput도 동일 구조, 좌우 방향만 반전).

```xml
<nodify:NodeInput.Style>
    <Style TargetType="{x:Type nodify:NodeInput}"
           BasedOn="{StaticResource {x:Type nodify:NodeInput}}">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type nodify:NodeInput}">
                    <!-- Nodify 원본에서 복사한 구조. 중요한 내부 파트는 그대로 유지 -->
                    <Grid ...>
                        <!-- PART_Connector: 바인딩은 Nodify 원본과 동일 -->
                        <Control x:Name="PART_Connector"
                                 Background="Transparent"
                                 BorderBrush="{TemplateBinding BorderBrush}"
                                 Template="{TemplateBinding ConnectorTemplate}"/>

                        <!-- Highlight: IsMouseOver 대신 IsHighlighted로 제어 -->
                        <Border Visibility="Collapsed"
                                x:Name="Highlight"
                                OpacityMask="{StaticResource FadeOpacityMask}"
                                Background="{TemplateBinding BorderBrush}"/>

                        <ContentPresenter ContentSource="Header" .../>
                    </Grid>

                    <ControlTemplate.Triggers>
                        <!-- IsConnected 트리거는 원본 유지 -->
                        <Trigger Property="IsConnected" Value="True">
                            <Setter TargetName="PART_Connector"
                                    Property="Background"
                                    Value="{Binding BorderBrush, RelativeSource={RelativeSource TemplatedParent}}"/>
                        </Trigger>

                        <!-- ★ 원본의 IsMouseOver 트리거 제거하고 IsHighlighted로 교체 ★ -->
                        <DataTrigger Binding="{Binding IsHighlighted}" Value="True">
                            <Setter TargetName="Highlight"
                                    Property="Visibility"
                                    Value="Visible"/>
                        </DataTrigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>

        <!-- 바깥 Style.Triggers: 컨트롤 레벨 속성 (template bind로 전달되는 속성들) -->
        <Style.Triggers>
            <DataTrigger Binding="{Binding IsHighlighted}" Value="True">
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="BorderBrush" Value="{StaticResource NeonGlowBrush}"/>
                <Setter Property="Effect" Value="{StaticResource SocketGlowEffect}"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</nodify:NodeInput.Style>
```

**로컬 재선언 필수 리소스** (Nodify 원본 ResourceDictionary에만 존재, BasedOn으로는 template 내부 `{StaticResource}` lookup이 안 닿음):
- `FadeOpacityMask` (`LinearGradientBrush`, 수평 fade)
- `FadeOpacityMaskVertical` (`LinearGradientBrush`, 수직 fade — NodeOutput용)
- `ConnectorThumbTemplate` (`ControlTemplate` — connector dot 모양)

Nodify 원본에서 복사해 View.Resources에 선언.

**주의:**
- `IsHighlighted` 바인딩은 `ConnectorItem`이 DataContext일 때 정상 동작. TemplatedParent가 아니라 DataContext를 타깃으로 하므로 `RelativeSource={RelativeSource TemplatedParent}` 금지.
- NodeInput/NodeOutput은 좌우 반전만 다르므로 동일 패턴을 한 쌍 작성.
- `Highlight` Border는 Nodify의 암묵적 내부 파트 (공개 `[TemplatePart]` 아님). Nodify 버전 업그레이드 시 재검증 필요 — `Directory.Packages.props`에 버전 고정 권장.

### Step 7 — MidpointConnection (line): Style로 기본값 이동

**문제:** `<MidpointConnection Stroke="..." StrokeThickness="2" OutlineBrush="..." OutlineThickness="4" ...>` 처럼 인라인으로 준 속성은 **local value**로 WPF value precedence에서 `Style.Trigger`의 Setter보다 **우선순위가 높다**. 그래서 `IsHighlighted=true` 트리거가 Stroke를 mint로 바꾸려 해도 인라인 Stroke가 이겨서 라인이 원래 색 그대로 유지된다.

**해결:** 인라인 속성을 모두 Style의 기본 Setter로 이동.

```xml
<DataTemplate DataType="{x:Type viewModels:ConnectionItem}">
    <!-- 인라인에는 binding만, 시각 속성은 Style로 -->
    <nodifyUIHelpers:MidpointConnection Source="{Binding Source.PixelPosition}"
                                        Target="{Binding Target.PixelPosition}"
                                        SourceNodePixelPosition="{Binding Source.Node.PixelPosition}"
                                        SourceNodePixelSize="{Binding Source.Node.PixelSize}"
                                        TargetNodePixelPosition="{Binding Target.Node.PixelPosition}"
                                        TargetNodePixelSize="{Binding Target.Node.PixelSize}">
        <nodifyUIHelpers:MidpointConnection.Style>
            <Style TargetType="{x:Type nodifyUIHelpers:MidpointConnection}">
                <!-- 기본값 — 인라인 대신 여기에 -->
                <Setter Property="Stroke" Value="{DynamicResource TextFillColorSecondaryBrush}"/>
                <Setter Property="StrokeThickness" Value="2"/>
                <Setter Property="OutlineBrush" Value="Transparent"/>
                <Setter Property="OutlineThickness" Value="4"/>

                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsHighlighted}" Value="True">
                        <Setter Property="Stroke" Value="{StaticResource NeonGlowBrush}"/>
                        <Setter Property="StrokeThickness" Value="2"/>
                        <Setter Property="OutlineBrush" Value="{StaticResource NeonGlowOutlineBrush}"/>
                        <Setter Property="OutlineThickness" Value="5"/>
                        <Setter Property="Effect" Value="{StaticResource ConnectionGlowEffect}"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </nodifyUIHelpers:MidpointConnection.Style>
    </nodifyUIHelpers:MidpointConnection>
</DataTemplate>
```

### Step 8 — 검증 체크리스트

- [ ] 연결되지 않은 소켓에 hover하면 효과 **없음** (unconnected gate)
- [ ] 연결된 1:1 소켓 hover 시: 소켓 + 라인 + peer 소켓 모두 동일 민트 네온
- [ ] 1:N (output 1개 + inputs N개) hover 시: N개 라인 + N개 peer 소켓 모두 강조, 라인 수렴 지점이 포화되지 않음
- [ ] hover된 쪽 소켓과 peer 소켓의 비주얼이 **완전히 동일** (Highlight Border 포함)
- [ ] 연결 삭제 후 hover/unhover에서 peer 카운트 누수 없음 (stale connection이 peer를 글로우시키지 않음)
- [ ] Undo로 연결 복원 시 현재 hover 상태가 즉시 반영
- [ ] 다크/라이트 테마 모두에서 민트색이 시인성 확보 (고정 색상이므로 테마 종속 없음)

## Common Pitfalls

| 증상 | 원인 | 해결 |
|------|------|------|
| 라인에 글로우 안 들어옴 | `MidpointConnection`에 인라인 `Stroke="..."`가 Style.Trigger Setter보다 우선 | 인라인 속성을 Style의 기본 Setter로 이동 |
| Peer 소켓만 글로우가 약함 (시각 비대칭) | Nodify의 template 내부 `IsMouseOver` Trigger가 `Highlight` Border를 hover 쪽에만 표시 | ControlTemplate 로컬 override + `IsHighlighted` DataTrigger로 교체 |
| 1:N fan-out 시 라인 수렴 지점이 너무 밝고 텍스트가 안 보임 | `DropShadowEffect.Opacity=1.0` 여러 개가 겹쳐 포화 | 라인용 Effect는 `Opacity=0.3` 수준, `Foreground` Setter는 빼서 텍스트는 테마 색 유지 |
| Unconnected 소켓도 hover 시 글로우 | `IsHighlighted = IsHovered \|\| peer` (OR) | `IsHighlighted = (IsHovered && IsConnected) \|\| peer > 0` + `IsConnected` setter에서 `IsHighlighted` PropertyChanged 발행 |
| 연결 삭제 → 다시 hover 시 stale peer 카운트로 잘못된 글로우 | `OnDetached`에서 구독을 해제하지 않음 | `PropertyChangedEventManager.RemoveHandler` 호출 + `_isAttached` 가드 |
| `OnAttached`를 중복 호출해서 이벤트가 중복 발행 | `AddHandler`는 중복 등록 허용 | `_isAttached` 플래그로 idempotent 가드 |
| `Peer count`가 음수로 내려감 | Decrement이 Increment보다 먼저 불림 (edge case) | `DecrementPeerHighlight`에 `<= 0` early return underflow 가드 |
| Template 복사 후 `Cannot find resource 'FadeOpacityMask'` | Nodify 원본 ResourceDictionary 리소스가 BasedOn으로 전파되지 않음 | `FadeOpacityMask`/`FadeOpacityMaskVertical`/`ConnectorThumbTemplate`을 View.Resources에 재선언 |
| `DropShadowEffect`가 매 트리거마다 새 인스턴스 생성 | `<Setter.Value>` 인라인 Freezable은 공유 불가 | 리소스로 선언 + `PresentationOptions:Freeze="True"` |

## When NOT to Use

- **단일 소켓만 강조** (peer 전파 불필요): `nodify:NodeInput` DataTemplate에 `Style.Triggers`로 `IsMouseOver=True` DataTrigger 하나만 추가하면 됨. 본 스킬의 peer 카운트/weak event 인프라는 과대.
- **도메인 상태(실행 중/에러/선택)로 강조**: hover 무관하므로 `ConnectionItem`에 `HasError` 같은 프로퍼티를 두고 단순 DataTrigger로 바인딩. `PropertyChangedEventManager` 필요 없음.
- **애니메이션 전이**: 본 스킬은 즉시 토글. fade-in/out 원하면 `Storyboard` + `BeginStoryboard`로 별도 레이어.

## 관련 스킬

- `integrating-nodify` — Nodify 기본 통합 (ViewModel 구조, PendingConnection, EditorGestures)
- `customizing-controltemplate` — ControlTemplate override 일반 원칙
- `creating-wpf-brushes` — DropShadowEffect + SolidColorBrush 패턴
- `using-wpf-behaviors-triggers` — `i:Interaction.Triggers` + `ChangePropertyAction` 심화

## 참고

- [Nodify NodeInput.xaml (v7.1.0)](https://github.com/miroiu/nodify/blob/v7.1.0/Nodify/Themes/Styles/NodeInput.xaml)
- [Nodify NodeOutput.xaml (v7.1.0)](https://github.com/miroiu/nodify/blob/v7.1.0/Nodify/Themes/Styles/NodeOutput.xaml)
- [WPF Dependency Property Value Precedence](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/dependency-property-value-precedence)
- [WeakEvent Patterns](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/weak-event-patterns)
