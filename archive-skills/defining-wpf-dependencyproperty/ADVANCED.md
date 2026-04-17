# Advanced DependencyProperty Patterns

## Property Value Inheritance

### Creating Inheritable Property

```csharp
namespace MyApp.Controls;

using System.Windows;

public class ThemeControl : Control
{
    public static readonly DependencyProperty AccentColorProperty = DependencyProperty.Register(
        nameof(AccentColor),
        typeof(System.Windows.Media.Color),
        typeof(ThemeControl),
        new FrameworkPropertyMetadata(
            System.Windows.Media.Colors.Blue,
            FrameworkPropertyMetadataOptions.Inherits));  // Inherits flag

    public System.Windows.Media.Color AccentColor
    {
        get => (System.Windows.Media.Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }
}
```

### Attached Inherited Property

```csharp
namespace MyApp.Attached;

using System.Windows;

public static class ThemeHelper
{
    public static readonly DependencyProperty ThemeNameProperty =
        DependencyProperty.RegisterAttached(
            "ThemeName",
            typeof(string),
            typeof(ThemeHelper),
            new FrameworkPropertyMetadata(
                "Default",
                FrameworkPropertyMetadataOptions.Inherits));

    public static string GetThemeName(DependencyObject obj)
        => (string)obj.GetValue(ThemeNameProperty);

    public static void SetThemeName(DependencyObject obj, string value)
        => obj.SetValue(ThemeNameProperty, value);
}
```

```xml
<!-- Set once at Window level, inherited by all children -->
<Window local:ThemeHelper.ThemeName="Dark">
    <Grid>
        <!-- All children can access ThemeName -->
        <Button Content="{Binding (local:ThemeHelper.ThemeName), RelativeSource={RelativeSource Self}}"/>
    </Grid>
</Window>
```

---

## Overriding Metadata

### Override Default Value

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;

public class MyButton : Button
{
    static MyButton()
    {
        // Override default value for existing property
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MyButton),
            new FrameworkPropertyMetadata(typeof(MyButton)));

        // Override with new default and callback
        BackgroundProperty.OverrideMetadata(
            typeof(MyButton),
            new FrameworkPropertyMetadata(
                System.Windows.Media.Brushes.Blue,
                OnBackgroundChanged));
    }

    private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle in derived class
    }
}
```

---

## Common Patterns

### Dependency Property with Event

```csharp
public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
    nameof(SelectedItem),
    typeof(object),
    typeof(ItemSelector),
    new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

// Routed event for selection changed
public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
    nameof(SelectedItemChanged),
    RoutingStrategy.Bubble,
    typeof(RoutedPropertyChangedEventHandler<object>),
    typeof(ItemSelector));

public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
{
    add => AddHandler(SelectedItemChangedEvent, value);
    remove => RemoveHandler(SelectedItemChangedEvent, value);
}

public object SelectedItem
{
    get => GetValue(SelectedItemProperty);
    set => SetValue(SelectedItemProperty, value);
}

private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var control = (ItemSelector)d;
    var args = new RoutedPropertyChangedEventArgs<object>(e.OldValue, e.NewValue, SelectedItemChangedEvent);
    control.RaiseEvent(args);
}
```

### Dependency Property Triggering Visual Update

```csharp
public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
    nameof(Radius),
    typeof(double),
    typeof(CircleControl),
    new FrameworkPropertyMetadata(
        50.0,
        FrameworkPropertyMetadataOptions.AffectsRender,
        OnRadiusChanged,
        CoerceRadius));

private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var control = (CircleControl)d;
    control.InvalidateVisual();  // Request re-render
}

private static object CoerceRadius(DependencyObject d, object baseValue)
{
    var value = (double)baseValue;
    return Math.Max(0, value);  // Ensure non-negative
}
```
