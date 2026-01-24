---
name: formatting-culture-aware-data
description: "Formats dates, numbers, and currency with culture awareness in WPF. Use when displaying localized data formats or building international applications."
---

# Culture-Aware Data Formatting in WPF

Format dates, numbers, and currency based on user's culture preferences.

## 1. Culture Formatting Overview

| Data Type | en-US | ko-KR | de-DE |
|-----------|-------|-------|-------|
| Date (d) | 1/21/2026 | 2026-01-21 | 21.01.2026 |
| Currency (C) | $1,234.56 | ₩1,235 | 1.234,56 € |
| Number (N2) | 1,234.56 | 1,234.56 | 1.234,56 |
| Percent (P) | 75.00% | 75.00% | 75,00 % |

---

## 2. XAML Formatting

### 2.1 Date Formatting

```xml
<!-- Short date (culture-aware) -->
<TextBlock Text="{Binding Date, StringFormat={}{0:d}}"/>

<!-- Long date -->
<TextBlock Text="{Binding Date, StringFormat={}{0:D}}"/>

<!-- Custom format (not culture-aware) -->
<TextBlock Text="{Binding Date, StringFormat={}{0:yyyy-MM-dd}}"/>

<!-- Date and time -->
<TextBlock Text="{Binding Date, StringFormat={}{0:g}}"/>
```

### 2.2 Number Formatting

```xml
<!-- Number with 2 decimal places -->
<TextBlock Text="{Binding Amount, StringFormat={}{0:N2}}"/>

<!-- Number with no decimals -->
<TextBlock Text="{Binding Count, StringFormat={}{0:N0}}"/>

<!-- Fixed decimal places -->
<TextBlock Text="{Binding Value, StringFormat={}{0:F2}}"/>
```

### 2.3 Currency Formatting

```xml
<!-- Currency (culture-aware symbol and format) -->
<TextBlock Text="{Binding Price, StringFormat={}{0:C}}"/>

<!-- Currency with no decimals -->
<TextBlock Text="{Binding Price, StringFormat={}{0:C0}}"/>
```

### 2.4 Percent Formatting

```xml
<!-- Percent (multiplies by 100) -->
<TextBlock Text="{Binding Rate, StringFormat={}{0:P}}"/>

<!-- Percent with 1 decimal -->
<TextBlock Text="{Binding Rate, StringFormat={}{0:P1}}"/>
```

---

## 3. Code Formatting

### 3.1 Using Current Culture

```csharp
// Uses Thread.CurrentThread.CurrentCulture
var dateStr = DateTime.Now.ToString("d");
var currencyStr = price.ToString("C");
var numberStr = amount.ToString("N2");
```

### 3.2 Specific Culture

```csharp
var koKr = new CultureInfo("ko-KR");
var deDE = new CultureInfo("de-DE");

// Korean formatting
var dateKr = DateTime.Now.ToString("d", koKr);      // 2026-01-21
var currencyKr = price.ToString("C", koKr);         // ₩1,234

// German formatting
var dateDE = DateTime.Now.ToString("d", deDE);      // 21.01.2026
var currencyDE = price.ToString("C", deDE);         // 1.234,56 €
```

### 3.3 Invariant Culture (for data storage)

```csharp
// Always use InvariantCulture for serialization
var dataStr = value.ToString(CultureInfo.InvariantCulture);
var parsed = double.Parse(dataStr, CultureInfo.InvariantCulture);
```

---

## 4. Localized Enum Converter

```csharp
namespace MyApp.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using MyApp.Resources;

public sealed class LocalizedEnumConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            var key = $"{enumValue.GetType().Name}_{enumValue}";
            return Strings.ResourceManager.GetString(key, culture)
                   ?? enumValue.ToString();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

**Resource file entries:**
```xml
<!-- Strings.resx -->
<data name="Status_Active"><value>Active</value></data>
<data name="Status_Inactive"><value>Inactive</value></data>

<!-- Strings.ko-KR.resx -->
<data name="Status_Active"><value>활성</value></data>
<data name="Status_Inactive"><value>비활성</value></data>
```

**Usage:**
```xml
<TextBlock Text="{Binding Status, Converter={local:LocalizedEnumConverter}}"/>
```

---

## 5. Localized String Resources

### 5.1 LocalizedStrings Class

```csharp
namespace MyApp.Resources;

public sealed class LocalizedStrings
{
    public Strings Strings { get; } = new();
}
```

### 5.2 App.xaml Registration

```xml
<Application.Resources>
    <local:LocalizedStrings x:Key="Loc"/>
</Application.Resources>
```

### 5.3 XAML Usage

```xml
<TextBlock Text="{Binding Source={StaticResource Loc}, Path=Strings.WelcomeMessage}"/>
<Button Content="{Binding Source={StaticResource Loc}, Path=Strings.SaveButton}"/>
```

---

## 6. Localized Images

### 6.1 File Structure

```
Resources/
├── Images/
│   ├── flag.png            (default/en-US)
│   ├── flag.ko-KR.png      (Korean)
│   └── flag.ja-JP.png      (Japanese)
```

### 6.2 Helper Class

```csharp
public static class LocalizedImageHelper
{
    public static string GetLocalizedPath(string basePath)
    {
        var culture = Thread.CurrentThread.CurrentUICulture.Name;
        var dir = Path.GetDirectoryName(basePath) ?? "";
        var name = Path.GetFileNameWithoutExtension(basePath);
        var ext = Path.GetExtension(basePath);

        var localizedPath = Path.Combine(dir, $"{name}.{culture}{ext}");

        return File.Exists(localizedPath) ? localizedPath : basePath;
    }
}
```

---

## 7. Format Specifiers Reference

| Specifier | Description | Example (en-US) |
|-----------|-------------|-----------------|
| d | Short date | 1/21/2026 |
| D | Long date | Tuesday, January 21, 2026 |
| t | Short time | 2:30 PM |
| T | Long time | 2:30:00 PM |
| g | General (short) | 1/21/2026 2:30 PM |
| G | General (long) | 1/21/2026 2:30:00 PM |
| C | Currency | $1,234.56 |
| N | Number | 1,234.56 |
| P | Percent | 75.00% |
| F | Fixed-point | 1234.56 |

---

## 8. References

- [Standard Numeric Format Strings - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
- [Standard Date and Time Format Strings - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings)
- [CultureInfo Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)
