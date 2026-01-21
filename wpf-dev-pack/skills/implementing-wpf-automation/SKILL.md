---
name: implementing-wpf-automation
description: Implements WPF UI Automation for accessibility using AutomationPeer and AutomationProperties. Use when building accessible applications or enabling screen reader support.
---

# WPF UI Automation Patterns

Implementing accessibility features using UI Automation framework.

## 1. UI Automation Overview

```
UI Automation Framework
├── Providers (Server-side)
│   ├── AutomationPeer (base class)
│   ├── FrameworkElementAutomationPeer
│   └── Custom AutomationPeers
├── Clients (Consumer-side)
│   ├── Screen readers (Narrator, JAWS)
│   ├── Testing tools
│   └── Custom automation clients
└── Automation Properties
    ├── AutomationProperties.Name
    ├── AutomationProperties.HelpText
    └── AutomationProperties.LabeledBy
```

---

## 2. AutomationProperties

### 2.1 Basic Properties

```xml
<!-- Name - primary identifier for screen readers -->
<!-- Name - 스크린 리더용 주 식별자 -->
<Button Content="Submit"
        AutomationProperties.Name="Submit form"/>

<!-- Name for image buttons (no text content) -->
<!-- 이미지 버튼용 이름 (텍스트 콘텐츠 없음) -->
<Button AutomationProperties.Name="Close window">
    <Image Source="/Icons/close.png"/>
</Button>

<!-- HelpText - additional description -->
<!-- HelpText - 추가 설명 -->
<TextBox AutomationProperties.Name="Email address"
         AutomationProperties.HelpText="Enter your email in format user@domain.com"/>

<!-- LabeledBy - reference to label element -->
<!-- LabeledBy - 라벨 요소 참조 -->
<Label x:Name="UsernameLabel" Content="Username:"/>
<TextBox AutomationProperties.LabeledBy="{Binding ElementName=UsernameLabel}"/>
```

### 2.2 Additional Properties

```xml
<!-- AcceleratorKey - keyboard shortcut hint -->
<!-- AcceleratorKey - 키보드 단축키 힌트 -->
<Button Content="_Save"
        AutomationProperties.AcceleratorKey="Ctrl+S"/>

<!-- AccessKey - mnemonic key -->
<!-- AccessKey - 니모닉 키 -->
<Button Content="_File"
        AutomationProperties.AccessKey="Alt+F"/>

<!-- ItemStatus - current state information -->
<!-- ItemStatus - 현재 상태 정보 -->
<ListBoxItem AutomationProperties.ItemStatus="Selected, 3 of 10"/>

<!-- ItemType - type description for list items -->
<!-- ItemType - 목록 항목의 타입 설명 -->
<ListBoxItem AutomationProperties.ItemType="Email message"/>

<!-- LiveSetting - for dynamic content updates -->
<!-- LiveSetting - 동적 콘텐츠 업데이트용 -->
<TextBlock AutomationProperties.LiveSetting="Polite"
           Text="{Binding StatusMessage}"/>
```

### 2.3 LiveSetting Values

| Value | Description |
|-------|-------------|
| **Off** | No announcements |
| **Polite** | Announce when user is idle |
| **Assertive** | Announce immediately |

---

## 3. Custom AutomationPeer

### 3.1 Basic Custom Peer

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

public class RatingControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(int), typeof(RatingControl),
        new PropertyMetadata(0));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int MaxValue { get; set; } = 5;

    // Create custom AutomationPeer
    // 커스텀 AutomationPeer 생성
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new RatingControlAutomationPeer(this);
    }
}

public class RatingControlAutomationPeer : FrameworkElementAutomationPeer
{
    public RatingControlAutomationPeer(RatingControl owner)
        : base(owner)
    {
    }

    private RatingControl RatingControl => (RatingControl)Owner;

    // Return control type for screen readers
    // 스크린 리더용 컨트롤 타입 반환
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Slider;
    }

    // Return class name
    // 클래스 이름 반환
    protected override string GetClassNameCore()
    {
        return nameof(RatingControl);
    }

    // Return accessible name
    // 접근 가능한 이름 반환
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            return $"Rating: {RatingControl.Value} of {RatingControl.MaxValue} stars";
        }

        return name;
    }

    // Return help text
    // 도움말 텍스트 반환
    protected override string GetHelpTextCore()
    {
        return "Use arrow keys to change rating";
    }
}
```

### 3.2 Implementing Automation Patterns

```csharp
namespace MyApp.Controls;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

public class RatingControlAutomationPeer : FrameworkElementAutomationPeer,
    IRangeValueProvider
{
    public RatingControlAutomationPeer(RatingControl owner)
        : base(owner)
    {
    }

    private RatingControl RatingControl => (RatingControl)Owner;

    // Expose supported patterns
    // 지원하는 패턴 노출
    public override object? GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.RangeValue)
        {
            return this;
        }

        return base.GetPattern(patternInterface);
    }

    // IRangeValueProvider implementation
    // IRangeValueProvider 구현
    public bool IsReadOnly => false;

    public double LargeChange => 1;

    public double SmallChange => 1;

    public double Maximum => RatingControl.MaxValue;

    public double Minimum => 0;

    public double Value => RatingControl.Value;

    public void SetValue(double value)
    {
        if (value < Minimum || value > Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        RatingControl.Value = (int)value;
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Slider;
    }

    protected override string GetClassNameCore()
    {
        return nameof(RatingControl);
    }
}
```

---

## 4. Common Automation Patterns

| Pattern | Purpose | Example Controls |
|---------|---------|------------------|
| **IInvokeProvider** | Single action | Button, MenuItem |
| **IToggleProvider** | Toggle state | CheckBox, ToggleButton |
| **ISelectionProvider** | Contains selectable items | ListBox, ComboBox |
| **ISelectionItemProvider** | Selectable item | ListBoxItem |
| **IRangeValueProvider** | Numeric range | Slider, ProgressBar |
| **IValueProvider** | Text value | TextBox |
| **IExpandCollapseProvider** | Expand/collapse | TreeViewItem, Expander |
| **IScrollProvider** | Scrollable content | ScrollViewer |

---

## 5. Raising Automation Events

### 5.1 Property Changed Event

```csharp
public class CustomControlAutomationPeer : FrameworkElementAutomationPeer
{
    public void RaiseValueChanged(int oldValue, int newValue)
    {
        // Notify automation clients of value change
        // 값 변경을 자동화 클라이언트에 알림
        RaisePropertyChangedEvent(
            RangeValuePatternIdentifiers.ValueProperty,
            (double)oldValue,
            (double)newValue);
    }

    public void RaiseSelectionChanged()
    {
        RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
    }
}
```

### 5.2 From Control

```csharp
public class RatingControl : Control
{
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RatingControl)d;

        // Get automation peer and raise event
        // AutomationPeer를 가져와 이벤트 발생
        var peer = UIElementAutomationPeer.FromElement(control) as RatingControlAutomationPeer;
        peer?.RaiseValueChanged((int)e.OldValue, (int)e.NewValue);
    }
}
```

---

## 6. Focus and Keyboard Navigation

### 6.1 Keyboard Support

```csharp
public class RatingControl : Control
{
    public RatingControl()
    {
        // Enable keyboard focus
        // 키보드 포커스 활성화
        Focusable = true;
        FocusVisualStyle = (Style)FindResource(SystemParameters.FocusVisualStyleKey);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Left:
            case Key.Down:
                if (Value > 0)
                {
                    Value--;
                    e.Handled = true;
                }
                break;

            case Key.Right:
            case Key.Up:
                if (Value < MaxValue)
                {
                    Value++;
                    e.Handled = true;
                }
                break;

            case Key.Home:
                Value = 0;
                e.Handled = true;
                break;

            case Key.End:
                Value = MaxValue;
                e.Handled = true;
                break;
        }
    }
}
```

### 6.2 Focus Visual

```xml
<Style TargetType="{x:Type local:RatingControl}">
    <Setter Property="FocusVisualStyle">
        <Setter.Value>
            <Style>
                <Setter Property="Control.Template">
                    <Setter.Value>
                        <ControlTemplate>
                            <Border BorderBrush="{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}"
                                    BorderThickness="2"
                                    CornerRadius="2"
                                    Margin="-2"/>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>
        </Setter.Value>
    </Setter>
</Style>
```

---

## 7. Screen Reader Announcements

### 7.1 Live Regions

```xml
<!-- Status updates announced when changed -->
<!-- 변경 시 상태 업데이트 안내 -->
<TextBlock x:Name="StatusText"
           AutomationProperties.LiveSetting="Polite"
           AutomationProperties.Name="Status"/>
```

```csharp
// Update status - screen reader will announce
// 상태 업데이트 - 스크린 리더가 안내
StatusText.Text = "3 items selected";
```

### 7.2 Programmatic Announcements

```csharp
using System.Windows.Automation.Peers;

public static void Announce(string message)
{
    var peer = FrameworkElementAutomationPeer.FromElement(Application.Current.MainWindow);

    if (peer != null)
    {
        peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }
}
```

---

## 8. Accessibility Checklist

### Essential

- [ ] All interactive elements have AutomationProperties.Name
- [ ] Images have descriptive names or are marked decorative
- [ ] Form fields are labeled (LabeledBy or Name)
- [ ] Focus is visible and logical order is correct
- [ ] Keyboard navigation works for all functionality

### Enhanced

- [ ] HelpText provides additional context
- [ ] AcceleratorKey documents shortcuts
- [ ] LiveSetting for dynamic content
- [ ] Custom controls have AutomationPeers
- [ ] Color contrast meets WCAG guidelines

---

## 9. Testing Accessibility

### 9.1 Using Inspect.exe

```
Tools location:
Windows SDK → bin → [arch] → inspect.exe

Usage:
1. Run inspect.exe
2. Hover over UI elements
3. View automation properties
4. Verify Name, ControlType, Patterns
```

### 9.2 Using Narrator

```
Windows + Ctrl + Enter: Toggle Narrator
Tab: Navigate forward
Shift + Tab: Navigate backward
Caps Lock + Up/Down: Read current item
```

---

## 10. References

- [UI Automation Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)
- [AutomationProperties Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.automationproperties)
- [Custom Control Accessibility - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/accessibility-best-practices)
