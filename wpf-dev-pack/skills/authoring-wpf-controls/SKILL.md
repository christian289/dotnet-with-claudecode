---
description: "Guides decision-making for WPF control authoring including UserControl vs Control vs FrameworkElement selection. Use when creating new controls or evaluating Style/Template/Trigger alternatives."
---

# WPF Control Authoring Guide

A guide for decision-making when authoring WPF controls.

## 1. Do You Need a New Control?

**Review alternatives first.** Thanks to WPF's extensibility, most requirements can be solved without creating a new control.

| Requirement | Alternative | Example |
|-------------|-------------|---------|
| Change appearance only | Style | Unify TextBlock to red Arial 14pt |
| Change control structure | ControlTemplate | Make RadioButton look like traffic light |
| Change data display method | DataTemplate | Add checkbox to ListBox items |
| Change state-based behavior | Trigger | Make selected item bold red |
| Display composite content | Rich Content | Show image+text together in Button |

**When a new control is needed:**

- New functionality/behavior not available in existing controls
- Reusable composite components
- Special input/interaction patterns

---

## 2. Base Class Selection

```
┌─────────────────────────────────────────────────────────────┐
│                    Control Type Decision                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────┐  │
│  │ UserControl │    │   Control   │    │ FrameworkElement│  │
│  └──────┬──────┘    └──────┬──────┘    └────────┬────────┘  │
│         │                  │                    │           │
│  Combine existing    ControlTemplate      Direct rendering  │
│  Quick development   Customization        Full control      │
│  No template         Theme support        Performance       │
│                                           optimization      │
└─────────────────────────────────────────────────────────────┘
```

### UserControl Selection Criteria

- ✅ Combining existing controls is sufficient
- ✅ Prefer application-like development approach
- ✅ ControlTemplate customization not needed
- ❌ Theme support not needed

### Control Selection Criteria (Recommended)

- ✅ Need appearance customization via ControlTemplate
- ✅ Need various theme support
- ✅ Need WPF built-in control level extensibility
- ✅ Complete separation of UI and logic

### FrameworkElement Selection Criteria

- ✅ Appearance not achievable by simple element composition
- ✅ Need direct rendering via OnRender
- ✅ Custom composition based on DrawingVisual
- ✅ Extreme performance optimization needed

---

## 3. Principles for Designing Stylable Controls

### 3.1 Don't Strictly Enforce Template Contract

```csharp
// ❌ Wrong: Throws exception if Part is missing
public override void OnApplyTemplate()
{
    var button = GetTemplateChild("PART_Button") as Button;
    if (button == null)
        throw new InvalidOperationException("PART_Button required!");
}

// ✅ Correct: Works even if Part is missing
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    ButtonElement = GetTemplateChild("PART_Button") as Button;
    // If null, only that feature is disabled, control continues to work
}
```

**Core Principles:**

- ControlTemplate may be incomplete at design time
- Panel doesn't throw exceptions for too many or too few children
- If required elements are missing, only disable that feature

### 3.2 Helper Element Patterns

| Type | Description | Example |
|------|-------------|---------|
| **Standalone** | Independent, reusable | Popup, ScrollViewer, TabPanel |
| **Type-based** | Recognizes TemplatedParent, auto-binding | ContentPresenter, ItemsPresenter |
| **Named** | Referenced in code via x:Name | PART_TextBox, PART_Button |

```csharp
// Type-based: ContentPresenter automatically binds to TemplatedParent.Content
<ContentPresenter />

// Named: Direct reference needed in code
<TextBox x:Name="PART_EditableTextBox" />
```

### 3.3 State/Behavior Expression Priority

Prefer higher items:

1. **Property Binding** - `ComboBox.IsDropDownOpen` ↔ `ToggleButton.IsChecked`
2. **Trigger/Animation** - Background color change on Hover state
3. **Command** - `ScrollBar.LineUpCommand`
4. **Standalone Helper** - `TabPanel` in `TabControl`
5. **Type-based Helper** - `ContentPresenter` in `Button`
6. **Named Helper** - `TextBox` in `ComboBox`
7. **Bubbled Event** - Event bubbling from Named element
8. **Custom OnRender** - `ButtonChrome` in `Button`

---

## 4. DependencyProperty Implementation

DependencyProperty is required to support styles, bindings, animations, and dynamic resources.

```csharp
public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(NumericUpDown),
        new FrameworkPropertyMetadata(
            defaultValue: 0,
            propertyChangedCallback: OnValueChanged,
            coerceValueCallback: CoerceValue));

public int Value
{
    get => (int)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
}

// ⚠️ Don't add logic to CLR wrapper! It's bypassed during binding
// Use callbacks instead:
private static void OnValueChanged(DependencyObject d,
    DependencyPropertyChangedEventArgs e) { }

private static object CoerceValue(DependencyObject d, object value)
    => Math.Clamp((int)value, 0, 100);
```

---

## 5. RoutedEvent Implementation

Use RoutedEvent to support bubbling, EventSetter, and EventTrigger.

```csharp
public static readonly RoutedEvent ValueChangedEvent =
    EventManager.RegisterRoutedEvent(
        nameof(ValueChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<int>),
        typeof(NumericUpDown));

public event RoutedPropertyChangedEventHandler<int> ValueChanged
{
    add => AddHandler(ValueChangedEvent, value);
    remove => RemoveHandler(ValueChangedEvent, value);
}

protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> e)
    => RaiseEvent(e);
```

---

## 6. Customization Support Strategy

```
┌────────────────────────────────────────────────────────────┐
│           Exposure Strategy by Customization Frequency     │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Very Frequent  →  Expose as DependencyProperty            │
│                    (Background, Foreground, etc.)          │
│                                                            │
│  Sometimes      →  Expose as Attached Property             │
│                    (Grid.Row, Canvas.Left, etc.)           │
│                                                            │
│  Rarely         →  Guide to redefine ControlTemplate       │
│                    (Documentation required)                │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

---

## 7. Theme Resource Organization

```
📁 Themes/
├── Generic.xaml          ← Default (required)
├── Aero.NormalColor.xaml ← Windows Vista/7
├── Luna.NormalColor.xaml ← Windows XP Blue
├── Luna.Homestead.xaml   ← Windows XP Olive
└── Luna.Metallic.xaml    ← Windows XP Silver
```

**Add ThemeInfo to AssemblyInfo.cs:**

```csharp
[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly,  // Theme-specific resources
    ResourceDictionaryLocation.SourceAssembly)] // Generic resources
```

**Set DefaultStyleKey in static constructor:**

```csharp
static NumericUpDown()
{
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(NumericUpDown),
        new FrameworkPropertyMetadata(typeof(NumericUpDown)));
}
```

---

## Decision Checklist

### Before Creating a New Control

- [ ] Can it be solved with Style?
- [ ] Can it be solved with ControlTemplate?
- [ ] Can it be solved with DataTemplate?
- [ ] Can it be solved with Trigger?
- [ ] Can it be solved with Rich Content?

### Base Class Selection

- [ ] Need ControlTemplate customization? → Control
- [ ] Need theme support? → Control
- [ ] Combining existing controls is sufficient? → UserControl
- [ ] Need direct rendering? → FrameworkElement

### Control Design

- [ ] Did you minimize Template Contract?
- [ ] Does it work even if Part is missing?
- [ ] Handling with feature disable instead of exception?
- [ ] Did you follow state expression priority?

### Properties/Events

- [ ] Are style/binding supporting properties DependencyProperty?
- [ ] Is there no logic in CLR wrapper?
- [ ] Are events implemented as RoutedEvent?

### Theme/Resources

- [ ] Is there a default style in Generic.xaml?
- [ ] Did you set ThemeInfo attribute?
- [ ] Did you set DefaultStyleKey?
