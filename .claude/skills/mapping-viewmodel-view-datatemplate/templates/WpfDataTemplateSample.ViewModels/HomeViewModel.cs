namespace WpfDataTemplateSample.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private string _welcomeMessage = "Welcome to Home Page!";

    [ObservableProperty] private string _description = "This is the home page content. DataTemplate automatically maps this ViewModel to HomeView.";
}
