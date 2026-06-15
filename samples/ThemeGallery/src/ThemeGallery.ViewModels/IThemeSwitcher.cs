namespace ThemeGallery.ViewModels;

/// <summary>
/// WPF-free abstraction so the ViewModel can swap the application theme
/// without referencing System.Windows (P-002).
/// </summary>
public interface IThemeSwitcher
{
    void Apply(string themeName);
}
