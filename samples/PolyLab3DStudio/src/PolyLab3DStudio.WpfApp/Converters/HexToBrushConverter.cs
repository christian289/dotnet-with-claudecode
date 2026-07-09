namespace PolyLab3DStudio.Converters;

/// <summary>Converts a "#RRGGBB" string to a frozen SolidColorBrush (cached).</summary>
public sealed class HexToBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, SolidColorBrush> Cache = [];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string hex = value as string ?? "#FFFFFF";
        if (!Cache.TryGetValue(hex, out SolidColorBrush? brush))
        {
            brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            Cache[hex] = brush;
        }

        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
