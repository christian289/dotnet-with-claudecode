namespace AvaloniaCustomControlSample.Controls;

public sealed class CustomButton : Button
{
    public static readonly StyledProperty<IBrush?> CustomBackgroundProperty =
        AvaloniaProperty.Register<CustomButton, IBrush?>(nameof(CustomBackground));

    public static readonly StyledProperty<IBrush?> CustomForegroundProperty =
        AvaloniaProperty.Register<CustomButton, IBrush?>(nameof(CustomForeground));

    public static readonly StyledProperty<string?> CustomTextProperty =
        AvaloniaProperty.Register<CustomButton, string?>(nameof(CustomText), "Custom Button");

    public static readonly StyledProperty<double> CornerRadiusValueProperty =
        AvaloniaProperty.Register<CustomButton, double>(nameof(CornerRadiusValue), 8.0);

    public IBrush? CustomBackground
    {
        get => GetValue(CustomBackgroundProperty);
        set => SetValue(CustomBackgroundProperty, value);
    }

    public IBrush? CustomForeground
    {
        get => GetValue(CustomForegroundProperty);
        set => SetValue(CustomForegroundProperty, value);
    }

    public string? CustomText
    {
        get => GetValue(CustomTextProperty);
        set => SetValue(CustomTextProperty, value);
    }

    public double CornerRadiusValue
    {
        get => GetValue(CornerRadiusValueProperty);
        set => SetValue(CornerRadiusValueProperty, value);
    }
}