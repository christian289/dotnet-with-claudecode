---
name: advanced-data-binding
description: "Implements advanced WPF data binding patterns including MultiBinding, PriorityBinding, and complex converters. Use when combining multiple values, fallback values, or implementing complex binding scenarios."
---

# WPF Advanced Data Binding

## 1. MultiBinding

`MultiBinding` is used to combine multiple source values into a single binding target.

### 1.1 Basic Pattern

```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} {1}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

### 1.2 With IMultiValueConverter

```csharp
public sealed class FullNameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not string firstName ||
            values[1] is not string lastName)
        {
            return DependencyProperty.UnsetValue;
        }

        return $"{firstName} {lastName}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource FullNameConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

### 1.3 Boolean Logic with MultiBinding

```csharp
public sealed class AllTrueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.All(v => v is true);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

```xml
<!-- Enable button only when all conditions are true -->
<Button Content="Submit">
    <Button.IsEnabled>
        <MultiBinding Converter="{StaticResource AllTrueConverter}">
            <Binding Path="IsFormValid"/>
            <Binding Path="IsNotBusy"/>
            <Binding Path="HasPermission"/>
        </MultiBinding>
    </Button.IsEnabled>
</Button>
```

---

## 2. PriorityBinding

`PriorityBinding` uses the first successful value among multiple bindings. Useful for async data loading.

### 2.1 Basic Pattern

```xml
<TextBlock>
    <TextBlock.Text>
        <PriorityBinding>
            <!-- Priority 1: Data loaded from server (async) -->
            <Binding Path="ServerData" IsAsync="True"/>
            <!-- Priority 2: Cached data -->
            <Binding Path="CachedData"/>
            <!-- Priority 3: Default value -->
            <Binding Source="Loading..."/>
        </PriorityBinding>
    </TextBlock.Text>
</TextBlock>
```

### 2.2 With FallbackValue

```xml
<Image>
    <Image.Source>
        <PriorityBinding>
            <!-- Priority 1: Original image -->
            <Binding Path="HighResImage" IsAsync="True"/>
            <!-- Priority 2: Thumbnail -->
            <Binding Path="Thumbnail"/>
            <!-- Priority 3: Default image -->
            <Binding Source="/Assets/placeholder.png"/>
        </PriorityBinding>
    </Image.Source>
</Image>
```

---

## 3. Complex Converter Patterns

### 3.1 Chained Converters

```csharp
public sealed class ConverterChain : List<IValueConverter>, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return this.Aggregate(value, (current, converter) =>
            converter.Convert(current, targetType, parameter, culture));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

```xml
<local:ConverterChain x:Key="NullToVisibilityInverted">
    <local:NullToBoolConverter/>
    <local:InverseBoolConverter/>
    <BooleanToVisibilityConverter/>
</local:ConverterChain>
```

### 3.2 Parametrized Converter

```csharp
public sealed class ComparisonConverter : IValueConverter
{
    public ComparisonType Type { get; set; } = ComparisonType.Equal;
    public object? CompareTo { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var compareValue = parameter ?? CompareTo;

        return Type switch
        {
            ComparisonType.Equal => Equals(value, compareValue),
            ComparisonType.NotEqual => !Equals(value, compareValue),
            ComparisonType.GreaterThan when value is IComparable c => c.CompareTo(compareValue) > 0,
            ComparisonType.LessThan when value is IComparable c => c.CompareTo(compareValue) < 0,
            _ => false
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public enum ComparisonType { Equal, NotEqual, GreaterThan, LessThan }
```

---

## 4. Binding Debugging

### 4.1 Enable Trace

```xml
<Window xmlns:diag="clr-namespace:System.Diagnostics;assembly=WindowsBase">
    <TextBlock Text="{Binding Path=Name,
               diag:PresentationTraceSources.TraceLevel=High}"/>
</Window>
```

### 4.2 Debug Converter

```csharp
public sealed class DebugConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        System.Diagnostics.Debugger.Break();
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        System.Diagnostics.Debugger.Break();
        return value;
    }
}
```

---

## 5. Best Practices

| Scenario | Recommended Pattern |
|----------|---------------------|
| Combining multiple values | MultiBinding + IMultiValueConverter |
| Async + fallback | PriorityBinding |
| Simple string format | StringFormat |
| Complex transformation | Custom IValueConverter |
| Two-way binding needed | Implement ConvertBack |

---

## 6. Related Skills

- `implementing-communitytoolkit-mvvm` - MVVM pattern basics
- `managing-wpf-collectionview-mvvm` - CollectionView binding
- `mapping-viewmodel-view-datatemplate` - DataTemplate mapping
