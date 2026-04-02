---
description: "Generates WPF CustomControl C# class and XAML ControlTemplate style from a control name. Use when creating a new custom control, scaffolding a templated control, or generating CustomControl boilerplate code. Usage: /wpf-dev-pack:make-wpf-custom-control <ControlName>"
model: sonnet
argument-hint: [ControlName]
---

# WPF CustomControl Generation

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the CustomControl name (e.g., CircularProgress, RangeSlider)". Do NOT proceed until a valid name is provided. Use the response as the ControlName for all subsequent steps.**

Generate a `$0` CustomControl.

- Replace `{BaseClass}` with the appropriate WPF base class (e.g., Control, Button, ContentControl, ItemsControl) based on the control name and context.
- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.

## Workflow

### Step 1: Validate Input

- `$0` must be PascalCase
- Determine the best BaseClass based on `$0` name and intended usage

### Step 3: Generate C# Class File

Create `$0.cs`:

```csharp
using System.Windows;
using System.Windows.Controls;

namespace {Namespace}.Controls;

/// <summary>
/// $0 - Custom WPF control based on {BaseClass}
/// </summary>
[TemplatePart(Name = "PART_Root", Type = typeof(Border))]
[TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
[TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
[TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
[TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
public class $0 : {BaseClass}
{
    #region Static Constructor

    static $0()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof($0),
            new FrameworkPropertyMetadata(typeof($0)));
    }

    #endregion

    #region Dependency Properties

    /// <summary>
    /// Example dependency property
    /// </summary>
    public static readonly DependencyProperty ExampleProperty =
        DependencyProperty.Register(
            nameof(Example),
            typeof(string),
            typeof($0),
            new FrameworkPropertyMetadata(
                defaultValue: string.Empty,
                flags: FrameworkPropertyMetadataOptions.AffectsRender,
                propertyChangedCallback: OnExampleChanged));

    public string Example
    {
        get => (string)GetValue(ExampleProperty);
        set => SetValue(ExampleProperty, value);
    }

    private static void OnExampleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is $0 control)
        {
            control.OnExampleChanged((string)e.OldValue, (string)e.NewValue);
        }
    }

    protected virtual void OnExampleChanged(string oldValue, string newValue)
    {
        // Handle property change
    }

    #endregion

    #region Template Parts

    private Border? _partRoot;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _partRoot = GetTemplateChild("PART_Root") as Border;

        UpdateVisualState(false);
    }

    #endregion

    #region Visual States

    private void UpdateVisualState(bool useTransitions)
    {
        if (!IsEnabled)
        {
            VisualStateManager.GoToState(this, "Disabled", useTransitions);
        }
        else if (IsMouseOver)
        {
            VisualStateManager.GoToState(this, "MouseOver", useTransitions);
        }
        else
        {
            VisualStateManager.GoToState(this, "Normal", useTransitions);
        }
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        UpdateVisualState(true);
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        UpdateVisualState(true);
    }

    #endregion
}
```

### Step 4: Generate XAML Style File

Create `Themes/$0.xaml`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:{Namespace}.Controls">

    <!-- Resources -->
    <SolidColorBrush x:Key="$0Background" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="$0Foreground" Color="#000000"/>
    <SolidColorBrush x:Key="$0BorderBrush" Color="#CCCCCC"/>
    <SolidColorBrush x:Key="$0MouseOverBackground" Color="#E5F3FF"/>

    <!-- Style -->
    <Style TargetType="{x:Type local:$0}">
        <Setter Property="Background" Value="{StaticResource $0Background}"/>
        <Setter Property="Foreground" Value="{StaticResource $0Foreground}"/>
        <Setter Property="BorderBrush" Value="{StaticResource $0BorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="8,4"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:$0}">
                    <Border x:Name="PART_Root"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Padding="{TemplateBinding Padding}"
                            CornerRadius="4">
                        <VisualStateManager.VisualStateGroups>
                            <VisualStateGroup x:Name="CommonStates">
                                <VisualState x:Name="Normal"/>
                                <VisualState x:Name="MouseOver">
                                    <Storyboard>
                                        <ColorAnimation
                                            Storyboard.TargetName="PART_Root"
                                            Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                            To="#E5F3FF"
                                            Duration="0:0:0.2"/>
                                    </Storyboard>
                                </VisualState>
                                <VisualState x:Name="Pressed">
                                    <Storyboard>
                                        <ColorAnimation
                                            Storyboard.TargetName="PART_Root"
                                            Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                            To="#CCE4FF"
                                            Duration="0:0:0.1"/>
                                    </Storyboard>
                                </VisualState>
                                <VisualState x:Name="Disabled">
                                    <Storyboard>
                                        <DoubleAnimation
                                            Storyboard.TargetName="PART_Root"
                                            Storyboard.TargetProperty="Opacity"
                                            To="0.5"
                                            Duration="0:0:0"/>
                                    </Storyboard>
                                </VisualState>
                            </VisualStateGroup>
                        </VisualStateManager.VisualStateGroups>

                        <ContentPresenter
                            HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                            VerticalAlignment="{TemplateBinding VerticalContentAlignment}"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

### Step 5: Update Generic.xaml

Add to `Themes/Generic.xaml` MergedDictionaries:

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Existing entries -->
    <ResourceDictionary Source="/Themes/$0.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

## Output Summary

After generation, provide:
1. Created files list
2. Next steps for customization
3. Usage example in XAML

```xml
<!-- Usage Example -->
<local:$0 Example="Hello World"/>
```
