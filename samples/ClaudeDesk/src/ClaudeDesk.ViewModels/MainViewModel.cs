namespace ClaudeDesk.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ChatViewModel _chat;
    private readonly SettingsViewModel _settings;

    [ObservableProperty] private string _title = "ClaudeDesk";

    // ViewModel First: assign a screen ViewModel to CurrentViewModel and
    // Mappings.xaml resolves the matching View into the shell ContentControl.
    [ObservableProperty] private object? _currentViewModel;

    public MainViewModel(ChatViewModel chat, SettingsViewModel settings)
    {
        _chat = chat;
        _settings = settings;
        CurrentViewModel = chat;
    }

    [RelayCommand]
    private void NavigateToChat() => CurrentViewModel = _chat;

    [RelayCommand]
    private void NavigateToSettings() => CurrentViewModel = _settings;
}
