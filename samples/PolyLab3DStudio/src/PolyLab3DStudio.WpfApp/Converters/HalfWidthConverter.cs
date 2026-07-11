namespace PolyLab3DStudio.Converters;

/// <summary>
/// Turns a container's ActualWidth into a two-column item width:
/// (width - gap) / 2, with the gap taken from the converter parameter.
/// Used by the 3D 사전 term-card grid (CSS grid 1fr 1fr equivalent).
/// </summary>
public sealed class HalfWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double width = value is double d ? d : 0;
        double gap = parameter is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double g) ? g : 0;
        return Math.Max(0, (width - gap) / 2);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
