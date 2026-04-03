# Converter Patterns

Standards for implementing `IValueConverter` in wpf-dev-pack projects.

---

## Singleton Pattern with Static Instance Property

Every converter must expose a static `Instance` property so it can be used directly in XAML without declaring a resource.
Converters are stateless — a single shared instance is safe and avoids ResourceDictionary clutter.

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyApp.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();

    // Private constructor enforces singleton usage
    private BoolToVisibilityConverter() { }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DependencyProperty.UnsetValue || value is null)
            return Visibility.Collapsed;

        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}
```

Usage in XAML (no ResourceDictionary entry required):

```xml
<TextBlock Visibility="{Binding IsActive,
               Converter={x:Static converters:BoolToVisibilityConverter.Instance}}" />
```

---

## Pure Function Requirement

Converters must be pure functions:

- No side effects (no writes to fields, services, or ViewModels)
- No dependency on external state (no static mutable state, no `DateTime.Now` inside Convert)
- Same inputs always produce the same output

If conversion logic requires external context, pass it via the `ConverterParameter` or via a `MultiBinding` with `IMultiValueConverter`.

---

## null and UnsetValue Handling

Always handle both `null` and `DependencyProperty.UnsetValue` as the first check in `Convert`:

```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is null || value == DependencyProperty.UnsetValue)
        return DependencyProperty.UnsetValue;   // or a safe default

    // normal conversion logic
}
```

Returning `DependencyProperty.UnsetValue` from `Convert` signals WPF to use the property's default value rather than throwing a binding error.

---

## TemplateBinding vs Binding in ControlTemplate

Inside a `ControlTemplate`, use `TemplateBinding` (not `Binding`) to reference the templated parent's properties.
`TemplateBinding` is a lightweight one-way binding that avoids the overhead of a full `Binding` object.

```xml
<!-- Correct: TemplateBinding for ControlTemplate property references -->
<ControlTemplate TargetType="{x:Type local:MyButton}">
    <Border Background="{TemplateBinding Background}"
            BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}"
            CornerRadius="4">
        <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                          VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
    </Border>
</ControlTemplate>
```

```xml
<!-- Incorrect: Binding with RelativeSource is verbose and slower -->
<Border Background="{Binding Background,
            RelativeSource={RelativeSource TemplatedParent}}">
```

Use `Binding` with `RelativeSource=TemplatedParent` only when you need two-way binding or a converter applied to a templated-parent property — `TemplateBinding` is one-way only and does not support converters directly.
