namespace WpfDISample.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _message = "Hello, Dependency Injection!";

    [RelayCommand]
    private void ChangeMessage()
    {
        Message = $"Button clicked at {DateTime.Now:HH:mm:ss}";
    }
}
