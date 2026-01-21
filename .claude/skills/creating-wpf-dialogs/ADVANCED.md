# Advanced WPF Dialog Patterns

## MVVM Dialog Pattern

### Dialog Service Interface

```csharp
namespace MyApp.Services;

public interface IDialogService
{
    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class;
    void ShowMessage(string message, string title = "Information");
    bool ShowConfirmation(string message, string title = "Confirm");
    string? ShowOpenFileDialog(string filter = "All files (*.*)|*.*");
    string? ShowSaveFileDialog(string filter = "All files (*.*)|*.*", string defaultFileName = "");
}
```

### Dialog Service Implementation

```csharp
namespace MyApp.Services;

using System.Windows;
using Microsoft.Win32;

public sealed class DialogService : IDialogService
{
    private readonly Dictionary<Type, Type> _mappings = new();

    public void Register<TViewModel, TView>()
        where TViewModel : class
        where TView : Window
    {
        _mappings[typeof(TViewModel)] = typeof(TView);
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);

        if (!_mappings.TryGetValue(viewModelType, out var viewType))
        {
            throw new InvalidOperationException(
                $"No view registered for {viewModelType.Name}");
        }

        var dialog = (Window)Activator.CreateInstance(viewType)!;
        dialog.DataContext = viewModel;
        dialog.Owner = Application.Current.MainWindow;

        return dialog.ShowDialog();
    }

    public void ShowMessage(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool ShowConfirmation(string message, string title = "Confirm")
    {
        var result = MessageBox.Show(message, title,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public string? ShowOpenFileDialog(string filter = "All files (*.*)|*.*")
    {
        var dialog = new OpenFileDialog { Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter = "All files (*.*)|*.*",
        string defaultFileName = "")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
```

### Usage in ViewModel

```csharp
namespace MyApp.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settingsVm = new SettingsDialogViewModel(_currentSettings);

        if (_dialogService.ShowDialog(settingsVm) == true)
        {
            ApplySettings(settingsVm.Settings);
        }
    }

    [RelayCommand]
    private void Delete()
    {
        if (_dialogService.ShowConfirmation("Are you sure you want to delete?"))
        {
            PerformDelete();
        }
    }
}
```

---

## Modeless Dialog (Tool Window)

```csharp
namespace MyApp.Views;

using System.Windows;

public partial class FindReplaceWindow : Window
{
    private static FindReplaceWindow? _instance;

    public static void ShowInstance(Window owner)
    {
        if (_instance == null || !_instance.IsLoaded)
        {
            _instance = new FindReplaceWindow { Owner = owner };
        }

        _instance.Show();
        _instance.Activate();
    }

    public FindReplaceWindow()
    {
        InitializeComponent();

        // Keep window on top of owner
        // 오너 위에 창 유지
        Owner = Application.Current.MainWindow;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _instance = null;
    }
}
```

---

## Input Dialog

### Simple Input Dialog

```csharp
namespace MyApp.Dialogs;

using System.Windows;
using System.Windows.Controls;

public static class InputDialog
{
    public static string? Show(string prompt, string title = "Input",
        string defaultValue = "")
    {
        var dialog = new Window
        {
            Title = title,
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false
        };

        var grid = new Grid { Margin = new Thickness(15) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var textBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 15) };
        Grid.SetRow(textBox, 1);
        grid.Children.Add(textBox);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(buttonPanel, 2);

        var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0) };
        var cancelButton = new Button { Content = "Cancel", Width = 75 };

        string? result = null;

        okButton.Click += (s, e) =>
        {
            result = textBox.Text;
            dialog.DialogResult = true;
        };

        cancelButton.Click += (s, e) => dialog.DialogResult = false;

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;
        textBox.Focus();
        textBox.SelectAll();

        return dialog.ShowDialog() == true ? result : null;
    }
}
```

### Usage

```csharp
var fileName = InputDialog.Show("Enter file name:", "New File", "untitled.txt");

if (fileName != null)
{
    CreateFile(fileName);
}
```
