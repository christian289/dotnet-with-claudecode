namespace PolyLab3DStudio.Converters;

/// <summary>Multi-binding equality check (active tool, selected swatch/row, active tab).</summary>
public sealed class ValuesEqualConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Length == 2 && Equals(values[0], values[1]);

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
