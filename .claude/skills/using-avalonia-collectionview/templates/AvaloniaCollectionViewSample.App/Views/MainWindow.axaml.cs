namespace AvaloniaCollectionViewSample.App.Views;

using Avalonia.Controls;
using AvaloniaCollectionViewSample.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
