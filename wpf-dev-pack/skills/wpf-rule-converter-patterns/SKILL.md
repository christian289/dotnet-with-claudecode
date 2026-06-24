---
name: wpf-rule-converter-patterns
description: "WPF IValueConverter rules: MarkupExtension singleton, pure functions, null/UnsetValue handling, TemplateBinding."
user-invocable: false
---

# Converter Patterns

Standards for implementing `IValueConverter` in wpf-dev-pack projects.

---

## MarkupExtension Singleton Pattern

Every converter is a `MarkupExtension` so it can be used directly in XAML without
declaring a resource, and `ProvideValue` returns a single shared instance
(converters are stateless, so a shared instance is safe and avoids
ResourceDictionary clutter). This is the **canonical** converter pattern,
matching the `/wpf-dev-pack:make-wpf-converter` scaffolder and the
`using-converter-markup-extension` knowledge topic. Pick this one pattern — do
NOT also expose a separate static `Instance` property.

Derive from a small base class (the scaffolder generates it once per project):

```csharp
namespace MyApp.Converters;

public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
    where T : class, new()
{
    private static readonly Lazy<T> _converter = new(() => new T());

    public override object ProvideValue(IServiceProvider serviceProvider) => _converter.Value;

    public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("ConvertBack is not supported.");
}

public sealed class BoolToVisibilityConverter : ConverterMarkupExtension<BoolToVisibilityConverter>
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || value == DependencyProperty.UnsetValue)
            return Visibility.Collapsed;

        var invert = parameter is "Invert" or "invert";
        return (value is bool b && (b ^ invert)) ? Visibility.Visible : Visibility.Collapsed;
    }
}
```

Usage in XAML (no ResourceDictionary entry required):

```xml
<TextBlock Visibility="{Binding IsActive,
               Converter={converters:BoolToVisibilityConverter}}" />
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
