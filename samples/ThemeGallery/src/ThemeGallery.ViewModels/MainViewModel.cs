namespace ThemeGallery.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IThemeSwitcher _themeSwitcher;

    public IReadOnlyList<string> Themes { get; } =
    [
        "SolarSystem",
        "Diablo",
        "Metal",
        "Korea",
        "Flame",
        "Spring",
        "Summer",
        "Autumn",
        "Winter",
        "Neon",
    ];

    [ObservableProperty] private string _selectedTheme = string.Empty;

    public MainViewModel(IThemeSwitcher themeSwitcher)
    {
        _themeSwitcher = themeSwitcher;
        SelectedTheme = Themes[0];
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _themeSwitcher.Apply(value);
        }
    }
}
