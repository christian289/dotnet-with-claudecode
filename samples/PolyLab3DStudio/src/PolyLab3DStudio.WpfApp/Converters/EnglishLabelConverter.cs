namespace PolyLab3DStudio.Converters;

/// <summary>Returns the parameter string when English labels are on, otherwise empty (for inline Runs).</summary>
public sealed class EnglishLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? parameter as string ?? "" : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
