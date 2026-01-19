namespace AvaloniaDISample.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _message = "Hello, Avalonia DI!";

    [RelayCommand]
    private void ChangeMessage()
    {
        Message = $"Button clicked at {DateTime.Now:HH:mm:ss}";
    }
}
