namespace WpfDataTemplateSample.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "Settings";

    [ObservableProperty] private bool _isDarkModeEnabled;

    [ObservableProperty] private bool _isNotificationsEnabled = true;
}
