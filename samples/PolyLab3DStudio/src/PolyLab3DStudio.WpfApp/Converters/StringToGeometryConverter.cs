namespace PolyLab3DStudio.Converters;

/// <summary>Parses geometry mini-language path data coming from data bindings.</summary>
public sealed class StringToGeometryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string data && data.Length > 0 ? Geometry.Parse(data) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
