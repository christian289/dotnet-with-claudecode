namespace WpfDataTemplateSample.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private object? _currentViewModel;

    public MainWindowViewModel()
    {
        // 초기 화면은 HomeViewModel
        // Initial view is HomeViewModel
        CurrentViewModel = new HomeViewModel();
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentViewModel = new HomeViewModel();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentViewModel = new SettingsViewModel();
    }
}
