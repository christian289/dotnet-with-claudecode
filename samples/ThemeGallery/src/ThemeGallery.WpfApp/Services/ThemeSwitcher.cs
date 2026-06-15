namespace ThemeGallery.Services;

public sealed class ThemeSwitcher : IThemeSwitcher
{
    private ResourceDictionary? _current;

    public void Apply(string themeName)
    {
        var dict = new ResourceDictionary
        {
            Source = new Uri(
                $"pack://application:,,,/ThemeGallery.Themes;component/{themeName}.xaml",
                UriKind.Absolute),
        };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_current is not null)
        {
            merged.Remove(_current);
        }

        merged.Add(dict);
        _current = dict;
    }
}
