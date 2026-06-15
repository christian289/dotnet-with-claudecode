using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ClaudeDesk.Converters;

public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
    where T : class, new()
{
    private static readonly Lazy<T> _converter = new(() => new T());

    public override object ProvideValue(IServiceProvider serviceProvider) => _converter.Value;

    public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("ConvertBack is not supported.");
}
