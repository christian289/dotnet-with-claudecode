---
name: make-wpf-usercontrol
description: "Generates WPF UserControl with XAML and code-behind. Usage: /wpf-dev-pack:make-wpf-usercontrol <ControlName> [--with-viewmodel]"
---

# WPF UserControl Generator

Generates UserControl XAML and code-behind files.

## Usage

```bash
# Basic UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# With ViewModel
/wpf-dev-pack:make-wpf-usercontrol UserProfile --with-viewmodel
```

## Generated Files

### {Name}Control.xaml

```xml
<UserControl x:Class="{Namespace}.Controls.{Name}Control"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="100" d:DesignWidth="300">

    <Grid>
        <!-- TODO: Add your UI elements here -->
    </Grid>

</UserControl>
```

### {Name}Control.xaml.cs

```csharp
namespace {Namespace}.Controls;

/// <summary>
/// {Name} UserControl.
/// </summary>
public partial class {Name}Control : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof({Name}Control),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the header text.
    /// </summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    #endregion

    public {Name}Control()
    {
        InitializeComponent();
    }
}
```

### With ViewModel Option

**{Name}ControlViewModel.cs**

```csharp
namespace {Namespace}.ViewModels;

public partial class {Name}ControlViewModel : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;

    [RelayCommand]
    private void Submit()
    {
        // TODO: Implement submit logic
    }
}
```

**{Name}Control.xaml (with ViewModel)**

```xml
<UserControl x:Class="{Namespace}.Controls.{Name}Control"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:{Namespace}.ViewModels">

    <UserControl.DataContext>
        <vm:{Name}ControlViewModel/>
    </UserControl.DataContext>

    <Grid>
        <TextBlock Text="{Binding Title}"/>
    </Grid>

</UserControl>
```

## UserControl vs CustomControl

| Aspect | UserControl | CustomControl |
|--------|-------------|---------------|
| Purpose | Reuse within specific app | Library distribution |
| Styling | Limited | Full ControlTemplate replacement |
| Complexity | Low | High |
| Creation | XAML + Code-behind | Class + Generic.xaml |

## When to Use UserControl

- ✅ UI composition used only within the app
- ✅ Rapid prototyping
- ✅ Simple composite controls
- ❌ External library distribution → Use CustomControl

## File Location

```
{Project}/
├── Controls/
│   ├── {Name}Control.xaml
│   └── {Name}Control.xaml.cs
└── ViewModels/
    └── {Name}ControlViewModel.cs  (with --with-viewmodel option)
```

## DependencyProperty Pattern

Use DependencyProperty to receive external bindings in UserControl:

```csharp
public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof({Name}Control),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnValueChanged));

public string Value
{
    get => (string)GetValue(ValueProperty);
    set => SetValue(ValueProperty, value);
}

private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    if (d is {Name}Control control)
    {
        // Handle value change
    }
}
```

```xml
<!-- Usage example -->
<local:{Name}Control Value="{Binding MyValue, Mode=TwoWay}"/>
```
