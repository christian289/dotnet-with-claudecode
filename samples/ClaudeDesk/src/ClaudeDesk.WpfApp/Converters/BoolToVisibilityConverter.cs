using System.Globalization;

namespace ClaudeDesk.Converters;

public sealed class BoolToVisibilityConverter : ConverterMarkupExtension<BoolToVisibilityConverter>
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || value == DependencyProperty.UnsetValue)
        {
            return Visibility.Collapsed;
        }

        bool invert = parameter is "Invert" or "invert";
        return (value is bool b && (b ^ invert)) ? Visibility.Visible : Visibility.Collapsed;
    }
}
