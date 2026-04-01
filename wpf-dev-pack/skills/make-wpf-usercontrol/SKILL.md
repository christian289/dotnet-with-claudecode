---
description: "Generates WPF UserControl with XAML and code-behind. Use when creating a new UserControl, scaffolding a composite UI component, or generating a UserControl with optional ViewModel. Usage: /wpf-dev-pack:make-wpf-usercontrol <ControlName> [--with-viewmodel]"
argument-hint: [ControlName]
---

# WPF UserControl Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the UserControl name (e.g., SearchBox, UserProfile)". Do NOT proceed until a valid name is provided. Use the response as the ControlName for all subsequent steps.**

Generate a `$0Control` UserControl with XAML and code-behind.
If `--with-viewmodel` is appended, also generate a corresponding ViewModel.

- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.
- Replace `{Project}` with the target project path.

## Usage

```bash
# Basic UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# With ViewModel
/wpf-dev-pack:make-wpf-usercontrol UserProfile --with-viewmodel
```

## Generated Files

### $0Control.xaml

```xml
<UserControl x:Class="{Namespace}.Controls.$0Control"
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

### $0Control.xaml.cs

```csharp
namespace {Namespace}.Controls;

/// <summary>
/// $0 UserControl.
/// </summary>
public partial class $0Control : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof($0Control),
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

    public $0Control()
    {
        InitializeComponent();
    }
}
```

### With ViewModel Option

**$0ControlViewModel.cs**

```csharp
namespace {Namespace}.ViewModels;

public partial class $0ControlViewModel : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;

    [RelayCommand]
    private void Submit()
    {
        // TODO: Implement submit logic
    }
}
```

**$0Control.xaml (with ViewModel)**

```xml
<UserControl x:Class="{Namespace}.Controls.$0Control"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:{Namespace}.ViewModels">

    <UserControl.DataContext>
        <vm:$0ControlViewModel/>
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
│   ├── $0Control.xaml
│   └── $0Control.xaml.cs
└── ViewModels/
    └── $0ControlViewModel.cs  (with --with-viewmodel option)
```

## DependencyProperty Pattern

Use DependencyProperty to receive external bindings in UserControl:

```csharp
public static readonly DependencyProperty ValueProperty =
    DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof($0Control),
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
    if (d is $0Control control)
    {
        // Handle value change
    }
}
```

```xml
<!-- Usage example -->
<local:$0Control Value="{Binding MyValue, Mode=TwoWay}"/>
```
