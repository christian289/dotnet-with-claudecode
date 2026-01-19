namespace WpfMvvmSample.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(UserViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
