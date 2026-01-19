namespace WpfMvvmSample.ViewModels;

public sealed partial class UserViewModel : ObservableObject
{
    // 단일 Attribute - Inline 작성
    // Single attribute - written inline
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private int _age;
    [ObservableProperty] private string _statusMessage = string.Empty;

    // 여러 Attribute - ObservableProperty는 항상 마지막에 inline
    // Multiple attributes - ObservableProperty always inline at the end
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty] private string _email = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // 저장 로직 시뮬레이션
        // Simulate save logic
        await Task.Delay(500);

        StatusMessage = $"Saved: {FirstName} {LastName} ({Email})";
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Email);
}
