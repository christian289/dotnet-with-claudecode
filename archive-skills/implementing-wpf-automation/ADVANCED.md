# WPF UI Automation — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

---

## 1. Implementing Automation Patterns

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
    public override object? GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.RangeValue)
        {
            return this;
        }

        return base.GetPattern(patternInterface);
    }

    // IRangeValueProvider implementation
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

## 2. Raising Automation Events

### 2.1 Property Changed Event

```csharp
public class CustomControlAutomationPeer : FrameworkElementAutomationPeer
{
    public void RaiseValueChanged(int oldValue, int newValue)
    {
        // Notify automation clients of value change
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

### 2.2 From Control

```csharp
public class RatingControl : Control
{
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RatingControl)d;

        // Get automation peer and raise event
        var peer = UIElementAutomationPeer.FromElement(control) as RatingControlAutomationPeer;
        peer?.RaiseValueChanged((int)e.OldValue, (int)e.NewValue);
    }
}
```

---

## 3. Focus and Keyboard Navigation

### 3.1 Keyboard Support

```csharp
public class RatingControl : Control
{
    public RatingControl()
    {
        // Enable keyboard focus
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

### 3.2 Focus Visual

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

## 4. Screen Reader Announcements

### 4.1 Live Regions

```xml
<!-- Status updates announced when changed -->
<TextBlock x:Name="StatusText"
           AutomationProperties.LiveSetting="Polite"
           AutomationProperties.Name="Status"/>
```

```csharp
// Update status - screen reader will announce
StatusText.Text = "3 items selected";
```

### 4.2 Programmatic Announcements

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
