# 필수 Converter 패턴

HTML/CSS → WPF XAML 변환 시 자주 사용되는 IValueConverter, IMultiValueConverter 구현.

---

## SizeToRectConverter

ActualWidth/Height를 Rect로 변환. Border.Clip의 RectangleGeometry에 사용.

```csharp
namespace MyApp.Converters;

/// <summary>
/// ActualWidth, ActualHeight를 Rect(0, 0, width, height)로 변환합니다.
/// Converts ActualWidth, ActualHeight to Rect(0, 0, width, height).
/// </summary>
public sealed class SizeToRectConverter : IMultiValueConverter
{
    public static readonly SizeToRectConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not double width ||
            values[1] is not double height)
        {
            return Rect.Empty;
        }

        return new Rect(0, 0, width, height);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

### XAML 사용

```xml
<RectangleGeometry.Rect>
    <MultiBinding Converter="{x:Static local:SizeToRectConverter.Instance}">
        <Binding Path="ActualWidth" RelativeSource="{RelativeSource AncestorType=Border}" />
        <Binding Path="ActualHeight" RelativeSource="{RelativeSource AncestorType=Border}" />
    </MultiBinding>
</RectangleGeometry.Rect>
```

---

## HeightMultiplierConverter

ActualHeight에 배율을 곱하여 회전 요소의 높이 계산.

```csharp
namespace MyApp.Converters;

/// <summary>
/// ActualHeight × 배율을 계산합니다.
/// Calculates ActualHeight × multiplier.
/// </summary>
public sealed class HeightMultiplierConverter : IValueConverter
{
    public static readonly HeightMultiplierConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double height)
        {
            return 0d;
        }

        var multiplier = parameter switch
        {
            string s when double.TryParse(s, out var m) => m,
            double d => d,
            _ => 2.0  // 기본 배율 (Default multiplier)
        };

        return height * multiplier;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

### XAML 사용

```xml
<Rectangle Height="{Binding ActualHeight,
                    RelativeSource={RelativeSource AncestorType=Border},
                    Converter={x:Static local:HeightMultiplierConverter.Instance},
                    ConverterParameter=2.0}" />
```

---

## CenterOffsetConverter

회전 요소를 중앙에 배치하기 위한 Canvas.Top 오프셋 계산.

```csharp
namespace MyApp.Converters;

/// <summary>
/// 회전 요소의 중앙 오프셋을 계산합니다: (height - height × multiplier) / 2
/// Calculates center offset for rotating element: (height - height × multiplier) / 2
/// </summary>
public sealed class CenterOffsetConverter : IValueConverter
{
    public static readonly CenterOffsetConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double height)
        {
            return 0d;
        }

        var multiplier = parameter switch
        {
            string s when double.TryParse(s, out var m) => m,
            double d => d,
            _ => 2.0
        };

        // 중앙 오프셋 = (원래 높이 - 확대된 높이) / 2
        // Center offset = (original height - scaled height) / 2
        return (height - height * multiplier) / 2;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

### XAML 사용

```xml
<Rectangle Canvas.Top="{Binding ActualHeight,
                        RelativeSource={RelativeSource AncestorType=Border},
                        Converter={x:Static local:CenterOffsetConverter.Instance},
                        ConverterParameter=2.0}" />
```

---

## Converter 등록 패턴

### 방법 1: x:Static (권장)

Converter를 static 싱글톤으로 만들어 직접 참조:

```csharp
public sealed class MyConverter : IValueConverter
{
    public static readonly MyConverter Instance = new();
    // ...
}
```

```xml
<Rectangle Height="{Binding ..., Converter={x:Static local:MyConverter.Instance}}" />
```

### 방법 2: ResourceDictionary

```xml
<ResourceDictionary>
    <local:HeightMultiplierConverter x:Key="HeightMultiplierConverter" />
</ResourceDictionary>

<Rectangle Height="{Binding ..., Converter={StaticResource HeightMultiplierConverter}}" />
```

### 방법 3: CustomControl 내 static 속성

```csharp
public class MyControl : Control
{
    public static readonly IMultiValueConverter RectConverter = new SizeToRectConverter();
    public static readonly IValueConverter HeightMultiplierConverter = new HeightMultiplierConverter();
    public static readonly IValueConverter CenterOffsetConverter = new CenterOffsetConverter();
}
```

```xml
<MultiBinding Converter="{x:Static local:MyControl.RectConverter}">
```

---

## GlobalUsings.cs

Converter 프로젝트용 전역 using:

```csharp
global using System;
global using System.Globalization;
global using System.Windows;
global using System.Windows.Data;
```
