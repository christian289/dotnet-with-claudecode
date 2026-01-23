---
name: make-wpf-converter
description: "Generates WPF IValueConverter or IMultiValueConverter classes with MarkupExtension pattern. Usage: /wpf-dev-pack:make-wpf-converter <ConverterName> [multi]"
---

# WPF Converter Generator

Generates IValueConverter or IMultiValueConverter with MarkupExtension pattern for direct XAML usage.

## Usage

```bash
# IValueConverter
/wpf-dev-pack:make-wpf-converter BoolToVisibility

# IMultiValueConverter
/wpf-dev-pack:make-wpf-converter AllTrue multi
```

---

## Generated Code

### Base Class (ConverterMarkupExtension.cs)

Create this base class first in your Converters folder:

```csharp
namespace {Namespace}.Converters;

/// <summary>
/// Base class for converters that can be used directly in XAML without resource declaration.
/// </summary>
public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
    where T : class, new()
{
    private static readonly Lazy<T> _converter = new(() => new T());

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter.Value;
    }

    public abstract object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture);

    public virtual object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported.");
    }
}
```

### Base Class (MultiConverterMarkupExtension.cs)

```csharp
namespace {Namespace}.Converters;

/// <summary>
/// Base class for multi-value converters with MarkupExtension support.
/// </summary>
public abstract class MultiConverterMarkupExtension<T> : MarkupExtension, IMultiValueConverter
    where T : class, new()
{
    private static readonly Lazy<T> _converter = new(() => new T());

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _converter.Value;
    }

    public abstract object? Convert(
        object?[] values,
        Type targetType,
        object? parameter,
        CultureInfo culture);

    public virtual object?[] ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported.");
    }
}
```

---

### IValueConverter

```csharp
namespace {Namespace}.Converters;

/// <summary>
/// Converts {SourceType} to {TargetType}.
/// </summary>
public sealed class {Name}Converter : ConverterMarkupExtension<{Name}Converter>
{
    public override object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        // TODO: Implement conversion logic
        if (value is not {SourceType} source)
        {
            return DependencyProperty.UnsetValue;
        }

        return source; // Replace with actual conversion
    }
}
```

### IMultiValueConverter

```csharp
namespace {Namespace}.Converters;

/// <summary>
/// Combines multiple values into a single result.
/// </summary>
public sealed class {Name}Converter : MultiConverterMarkupExtension<{Name}Converter>
{
    public override object? Convert(
        object?[] values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        // Validate input values
        if (values is null || values.Length < 2)
        {
            return DependencyProperty.UnsetValue;
        }

        // Check for unset values
        if (values.Any(v => v == DependencyProperty.UnsetValue))
        {
            return DependencyProperty.UnsetValue;
        }

        // TODO: Implement multi-value conversion logic
        return values;
    }
}
```

---

## XAML Usage

### MarkupExtension Pattern (Recommended)

```xml
<Window xmlns:conv="clr-namespace:MyApp.Converters">
    <!-- No resource declaration needed! -->
    <TextBlock Visibility="{Binding IsVisible, Converter={conv:BoolToVisibilityConverter}}"/>

    <!-- With parameter -->
    <TextBlock Visibility="{Binding IsHidden, Converter={conv:BoolToVisibilityConverter}, ConverterParameter=Invert}"/>
</Window>
```

### MultiBinding

```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{conv:FullNameConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

---

## Common Converters

### BoolToVisibilityConverter

```csharp
public sealed class BoolToVisibilityConverter : ConverterMarkupExtension<BoolToVisibilityConverter>
{
    public override object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not bool boolValue)
            return Visibility.Collapsed;

        var invert = parameter is "Invert" or "invert";
        return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }
}
```

### NullToVisibilityConverter

```csharp
public sealed class NullToVisibilityConverter : ConverterMarkupExtension<NullToVisibilityConverter>
{
    public override object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var isNull = value is null;
        var invert = parameter is "Invert";
        return (isNull ^ invert) ? Visibility.Collapsed : Visibility.Visible;
    }
}
```

### InverseBoolConverter

```csharp
public sealed class InverseBoolConverter : ConverterMarkupExtension<InverseBoolConverter>
{
    public override object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not bool boolValue)
            return false;

        return !boolValue;
    }

    public override object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not bool boolValue)
            return false;

        return !boolValue;
    }
}
```

---

## File Structure

```
{Project}/
└── Converters/
    ├── ConverterMarkupExtension.cs       # Base class
    ├── MultiConverterMarkupExtension.cs  # Multi base class
    ├── BoolToVisibilityConverter.cs
    ├── NullToVisibilityConverter.cs
    └── {Name}Converter.cs
```

---

## GlobalUsings.cs

```csharp
global using System;
global using System.Globalization;
global using System.Linq;
global using System.Windows;
global using System.Windows.Data;
global using System.Windows.Markup;
```

---

## Comparison: StaticResource vs MarkupExtension

| Aspect | StaticResource | MarkupExtension |
|--------|---------------|-----------------|
| Resource declaration | Required | Not required |
| XAML usage | `{StaticResource Key}` | `{local:Converter}` |
| Singleton | Manual | Built-in (Lazy) |
| Boilerplate | More | Less |

---

## Related Skills

- `using-converter-markup-extension` - Detailed MarkupExtension pattern
- `advanced-data-binding` - MultiBinding patterns
