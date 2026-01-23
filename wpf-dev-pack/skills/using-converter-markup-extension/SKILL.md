---
name: using-converter-markup-extension
description: Implements IValueConverter as MarkupExtension for direct XAML usage without StaticResource. Use when creating converters to eliminate resource dictionary declarations.
---

# Using Converter Markup Extension

Combine `MarkupExtension` with `IValueConverter` for direct XAML usage without resource declarations.

## Why Markup Extension Converters?

| Aspect | StaticResource Converter | MarkupExtension Converter |
|--------|-------------------------|--------------------------|
| Declaration | Required in Resources | Not required |
| XAML Usage | `{StaticResource MyConverter}` | `{local:MyConverter}` |
| Singleton | Manual implementation | Built-in lazy singleton |
| Boilerplate | More | Less |

---

## Base Classes

### IValueConverter Base (.NET 7+)

```csharp
namespace MyApp.Converters;

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
        throw new NotSupportedException();
    }
}
```

### IMultiValueConverter Base

```csharp
namespace MyApp.Converters;

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
        throw new NotSupportedException();
    }
}
```

---

## Example Converters

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

**XAML Usage:**
```xml
<Button Visibility="{Binding IsEnabled, Converter={local:BoolToVisibilityConverter}}"/>

<!-- With parameter -->
<Button Visibility="{Binding IsDisabled, Converter={local:BoolToVisibilityConverter}, ConverterParameter=Invert}"/>
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

### StringFormatConverter

```csharp
public sealed class StringFormatConverter : ConverterMarkupExtension<StringFormatConverter>
{
    public override object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (parameter is not string format)
            return value?.ToString();

        return string.Format(culture, format, value);
    }
}
```

**XAML Usage:**
```xml
<TextBlock Text="{Binding Price, Converter={local:StringFormatConverter}, ConverterParameter='{}{0:C}'}"/>
```

### FullNameConverter (Multi)

```csharp
public sealed class FullNameConverter : MultiConverterMarkupExtension<FullNameConverter>
{
    public override object? Convert(
        object?[] values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Length < 2)
            return string.Empty;

        var firstName = values[0]?.ToString() ?? string.Empty;
        var lastName = values[1]?.ToString() ?? string.Empty;

        return $"{firstName} {lastName}".Trim();
    }
}
```

**XAML Usage:**
```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{local:FullNameConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

---

## GlobalUsings.cs

```csharp
global using System;
global using System.Globalization;
global using System.Windows;
global using System.Windows.Data;
global using System.Windows.Markup;
```

---

## Migration from StaticResource

### Before

```xml
<Window.Resources>
    <local:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
</Window.Resources>

<Button Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibility}}"/>
```

### After

```xml
<!-- No resource declaration needed -->
<Button Visibility="{Binding IsVisible, Converter={local:BoolToVisibilityConverter}}"/>
```

---

## File Structure

```
MyApp/
├── Converters/
│   ├── ConverterMarkupExtension.cs      # Base class
│   ├── MultiConverterMarkupExtension.cs # Multi base class
│   ├── BoolToVisibilityConverter.cs
│   ├── NullToVisibilityConverter.cs
│   └── StringFormatConverter.cs
```

---

## Checklist

- [ ] Inherit from `ConverterMarkupExtension<T>` or `MultiConverterMarkupExtension<T>`
- [ ] Class is `sealed` (no inheritance needed)
- [ ] Handle null input gracefully
- [ ] Return `DependencyProperty.UnsetValue` for invalid input if needed
- [ ] Use `ConverterParameter` for variations instead of multiple converters
