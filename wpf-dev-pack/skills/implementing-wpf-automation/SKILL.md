---
description: Implements WPF UI Automation for accessibility using AutomationPeer and AutomationProperties. Use when building accessible applications or enabling screen reader support.
---

# WPF UI Automation Patterns

Implementing accessibility features using UI Automation framework.

## Prerequisites

When implementing AutomationPeer for a CustomControl, ensure the control project is properly set up:

- **ThemeInfo attribute** in AssemblyInfo.cs → See `/configuring-wpf-themeinfo`
- **CustomControl project structure** → See `/authoring-wpf-controls`
- **DefaultStyleKeyProperty** in static constructor

---

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
<Button Content="Submit"
        AutomationProperties.Name="Submit form"/>

<!-- Name for image buttons (no text content) -->
<Button AutomationProperties.Name="Close window">
    <Image Source="/Icons/close.png"/>
</Button>

<!-- HelpText - additional description -->
<TextBox AutomationProperties.Name="Email address"
         AutomationProperties.HelpText="Enter your email in format user@domain.com"/>

<!-- LabeledBy - reference to label element -->
<Label x:Name="UsernameLabel" Content="Username:"/>
<TextBox AutomationProperties.LabeledBy="{Binding ElementName=UsernameLabel}"/>
```

### 2.2 Additional Properties

```xml
<!-- AcceleratorKey - keyboard shortcut hint -->
<Button Content="_Save"
        AutomationProperties.AcceleratorKey="Ctrl+S"/>

<!-- AccessKey - mnemonic key -->
<Button Content="_File"
        AutomationProperties.AccessKey="Alt+F"/>

<!-- ItemStatus - current state information -->
<ListBoxItem AutomationProperties.ItemStatus="Selected, 3 of 10"/>

<!-- ItemType - type description for list items -->
<ListBoxItem AutomationProperties.ItemType="Email message"/>

<!-- LiveSetting - for dynamic content updates -->
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
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Slider;
    }

    // Return class name
    protected override string GetClassNameCore()
    {
        return nameof(RatingControl);
    }

    // Return accessible name
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
    protected override string GetHelpTextCore()
    {
        return "Use arrow keys to change rating";
    }
}
```

> **Advanced**: See [ADVANCED.md](ADVANCED.md) for implementing automation patterns (IRangeValueProvider), raising automation events, keyboard navigation, and screen reader announcements.

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

## 5. Accessibility Checklist

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

## 6. Testing Accessibility

### 6.1 Using Inspect.exe

```
Tools location:
Windows SDK → bin → [arch] → inspect.exe

Usage:
1. Run inspect.exe
2. Hover over UI elements
3. View automation properties
4. Verify Name, ControlType, Patterns
```

### 6.2 Using Narrator

```
Windows + Ctrl + Enter: Toggle Narrator
Tab: Navigate forward
Shift + Tab: Navigate backward
Caps Lock + Up/Down: Read current item
```

---

## 7. References

- [UI Automation Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)
- [AutomationProperties Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.automationproperties)
- [Custom Control Accessibility - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/accessibility-best-practices)
