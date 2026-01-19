namespace WpfCollectionViewSample.App.Views;

using System.Windows;
using WpfCollectionViewSample.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
