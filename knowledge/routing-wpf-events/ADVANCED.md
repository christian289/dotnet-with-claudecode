# WPF Routed Events — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

---

## 1. Creating Custom Routed Events

### 1.1 Bubbling Event

```csharp
namespace MyApp.Controls;

using System.Windows;

public class CustomSlider : Control
{
    // Register routed event
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        name: "ValueChanged",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedPropertyChangedEventHandler<double>),
        ownerType: typeof(CustomSlider));

    // CLR event wrapper
    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // Raise the event
    protected virtual void OnValueChanged(double oldValue, double newValue)
    {
        var args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent);
        RaiseEvent(args);
    }
}
```

### 1.2 Tunneling Event (with Preview)

```csharp
namespace MyApp.Controls;

using System.Windows;

public class CustomButton : Button
{
    // Tunneling (Preview) event
    public static readonly RoutedEvent PreviewClickedEvent = EventManager.RegisterRoutedEvent(
        name: "PreviewClicked",
        routingStrategy: RoutingStrategy.Tunnel,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(CustomButton));

    // Bubbling event
    public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent(
        name: "Clicked",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(CustomButton));

    public event RoutedEventHandler PreviewClicked
    {
        add => AddHandler(PreviewClickedEvent, value);
        remove => RemoveHandler(PreviewClickedEvent, value);
    }

    public event RoutedEventHandler Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }

    protected override void OnClick()
    {
        // Raise Preview (Tunneling) first
        var previewArgs = new RoutedEventArgs(PreviewClickedEvent, this);
        RaiseEvent(previewArgs);

        // If not handled, raise Bubbling event
        if (!previewArgs.Handled)
        {
            var args = new RoutedEventArgs(ClickedEvent, this);
            RaiseEvent(args);
        }

        base.OnClick();
    }
}
```

### 1.3 Custom EventArgs

```csharp
namespace MyApp.Events;

using System.Windows;

public class ItemSelectedEventArgs : RoutedEventArgs
{
    public object SelectedItem { get; }
    public int SelectedIndex { get; }

    public ItemSelectedEventArgs(RoutedEvent routedEvent, object source, object selectedItem, int selectedIndex)
        : base(routedEvent, source)
    {
        SelectedItem = selectedItem;
        SelectedIndex = selectedIndex;
    }
}

public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);
```

---

## 2. Class Event Handlers

### 2.1 Registering Class Handler

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class EnhancedTextBox : TextBox
{
    static EnhancedTextBox()
    {
        // Class handler - called before instance handlers
        EventManager.RegisterClassHandler(
            typeof(EnhancedTextBox),
            PreviewKeyDownEvent,
            new KeyEventHandler(OnPreviewKeyDownClass));

        EventManager.RegisterClassHandler(
            typeof(EnhancedTextBox),
            GotFocusEvent,
            new RoutedEventHandler(OnGotFocusClass));
    }

    private static void OnPreviewKeyDownClass(object sender, KeyEventArgs e)
    {
        // Called for all EnhancedTextBox instances
        if (sender is EnhancedTextBox textBox)
        {
            // Common keyboard handling logic
        }
    }

    private static void OnGotFocusClass(object sender, RoutedEventArgs e)
    {
        // Auto-select all text on focus
        if (sender is EnhancedTextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
```

---

## 3. Defining Attached Events

```csharp
namespace MyApp.Behaviors;

using System.Windows;

public static class ValidationBehavior
{
    // Attached routed event
    public static readonly RoutedEvent ValidationErrorEvent = EventManager.RegisterRoutedEvent(
        name: "ValidationError",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(ValidationBehavior));

    public static void AddValidationErrorHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement element)
        {
            element.AddHandler(ValidationErrorEvent, handler);
        }
    }

    public static void RemoveValidationErrorHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement element)
        {
            element.RemoveHandler(ValidationErrorEvent, handler);
        }
    }

    // Raise the attached event
    public static void RaiseValidationError(UIElement element)
    {
        element.RaiseEvent(new RoutedEventArgs(ValidationErrorEvent, element));
    }
}
```
