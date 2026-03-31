# WPF Input and Commands — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

## Custom RoutedCommand

### Define Custom Command

```csharp
namespace MyApp.Commands;

using System.Windows.Input;

public static class CustomCommands
{
    // Define custom command
    public static readonly RoutedCommand RefreshData = new(
        nameof(RefreshData),
        typeof(CustomCommands),
        new InputGestureCollection
        {
            new KeyGesture(Key.F5),
            new KeyGesture(Key.R, ModifierKeys.Control)
        });

    public static readonly RoutedCommand ExportToPdf = new(
        nameof(ExportToPdf),
        typeof(CustomCommands),
        new InputGestureCollection
        {
            new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Shift)
        });

    public static readonly RoutedCommand ToggleFullScreen = new(
        nameof(ToggleFullScreen),
        typeof(CustomCommands),
        new InputGestureCollection
        {
            new KeyGesture(Key.F11)
        });
}
```

### Use Custom Command in XAML

```xml
<Window xmlns:cmd="clr-namespace:MyApp.Commands">
    <Window.CommandBindings>
        <CommandBinding Command="{x:Static cmd:CustomCommands.RefreshData}"
                        Executed="RefreshData_Executed"
                        CanExecute="RefreshData_CanExecute"/>
        <CommandBinding Command="{x:Static cmd:CustomCommands.ExportToPdf}"
                        Executed="ExportToPdf_Executed"/>
    </Window.CommandBindings>

    <Menu>
        <MenuItem Header="_File">
            <MenuItem Header="_Refresh"
                      Command="{x:Static cmd:CustomCommands.RefreshData}"
                      InputGestureText="F5"/>
            <MenuItem Header="_Export to PDF"
                      Command="{x:Static cmd:CustomCommands.ExportToPdf}"
                      InputGestureText="Ctrl+Shift+E"/>
        </MenuItem>
    </Menu>
</Window>
```

---

## Handling Keyboard Input

### Key Events

```csharp
namespace MyApp.Views;

using System.Windows;
using System.Windows.Input;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Preview events (Tunneling - captured before child elements)
        PreviewKeyDown += OnPreviewKeyDown;

        // Normal events (Bubbling - captured after child elements)
        KeyDown += OnKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Global shortcut handling
        if (e.Key == Key.Escape)
        {
            // Close popup or cancel operation
            e.Handled = true;
        }

        // Modifier key combinations
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.G)
        {
            // Ctrl+G: Go to line
            ShowGoToLineDialog();
            e.Handled = true;
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Handle if not processed by child elements
    }
}
```

### TextInput Event

```csharp
private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
{
    // Allow only numeric input
    if (!char.IsDigit(e.Text, 0))
    {
        e.Handled = true;
    }
}
```

---

## Handling Mouse Input

### Mouse Events

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Input;

public partial class DrawingCanvas : FrameworkElement
{
    private Point _startPoint;
    private bool _isDragging;

    public DrawingCanvas()
    {
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseWheel += OnMouseWheel;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        _isDragging = true;

        // Capture mouse to receive events outside the element
        CaptureMouse();

        // Click count detection
        if (e.ClickCount == 2)
        {
            // Double click
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _startPoint;

        // Draw or drag logic
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // e.Delta: positive = scroll up, negative = scroll down
        var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
        ApplyZoom(zoomFactor);
    }
}
```

---

## Focus Management

### Programmatic Focus Control

```csharp
// Set focus
myTextBox.Focus();

// Set keyboard focus specifically
Keyboard.Focus(myTextBox);

// Check focus
if (myTextBox.IsFocused)
{
    // Has focus
}

if (myTextBox.IsKeyboardFocused)
{
    // Has keyboard focus
}
```

### FocusManager

```xml
<!-- Set default focused element -->
<Window FocusManager.FocusedElement="{Binding ElementName=FirstTextBox}">
    <StackPanel>
        <TextBox x:Name="FirstTextBox"/>
        <TextBox x:Name="SecondTextBox"/>
    </StackPanel>
</Window>

<!-- Define focus scope -->
<ToolBar FocusManager.IsFocusScope="True">
    <Button Content="Button 1"/>
    <Button Content="Button 2"/>
</ToolBar>
```
