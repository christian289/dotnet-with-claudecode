---
name: make-wpf-behavior
description: "Generates WPF Behavior<T> classes using Microsoft.Xaml.Behaviors.Wpf. Usage: /wpf-dev-pack:make-wpf-behavior <BehaviorName> <TargetType>"
---

# WPF Behavior Generator

Generates Behavior<T> classes based on Microsoft.Xaml.Behaviors.Wpf.

## Usage

```bash
# Behavior for TextBox
/wpf-dev-pack:make-wpf-behavior SelectAllOnFocus TextBox

# General behavior for UIElement
/wpf-dev-pack:make-wpf-behavior DragDrop UIElement

# Behavior for Window
/wpf-dev-pack:make-wpf-behavior AutoClose Window
```

## Generated Code

```csharp
namespace {Namespace}.Behaviors;

/// <summary>
/// {Description}
/// </summary>
public sealed class {Name}Behavior : Behavior<{TargetType}>
{
    #region Dependency Properties

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(
            nameof(IsEnabled),
            typeof(bool),
            typeof({Name}Behavior),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether the behavior is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    #endregion

    #region Lifecycle

    protected override void OnAttached()
    {
        base.OnAttached();

        // Subscribe to events
        AssociatedObject.Loaded += OnLoaded;
        // TODO: Add more event subscriptions as needed
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        // IMPORTANT: Always unsubscribe to prevent memory leaks
        AssociatedObject.Loaded -= OnLoaded;
        // TODO: Remove all event subscriptions
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        // TODO: Implement behavior logic
    }

    #endregion
}
```

## XAML Usage

```xml
<Window xmlns:b="http://schemas.microsoft.com/xaml/behaviors"
        xmlns:behaviors="clr-namespace:MyApp.Behaviors">

    <TextBox>
        <b:Interaction.Behaviors>
            <behaviors:{Name}Behavior IsEnabled="{Binding IsBehaviorEnabled}"/>
        </b:Interaction.Behaviors>
    </TextBox>

</Window>
```

## Common Behavior Patterns

### Focus Management

```csharp
public sealed class FocusOnLoadBehavior : Behavior<UIElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += (s, e) => AssociatedObject.Focus();
    }
}
```

### Input Handling

```csharp
public sealed class EnterKeyBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand),
            typeof(EnterKeyBehavior));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.KeyDown += OnKeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.KeyDown -= OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Command?.Execute(AssociatedObject.Text);
        }
    }
}
```

## Required Package

```xml
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.*" />
```

## File Location

```
{Project}/
└── Behaviors/
    └── {Name}Behavior.cs
```

## Best Practices

| DO | DON'T |
|----|-------|
| ✅ Unsubscribe events in OnDetaching | ❌ Missing event unsubscription (memory leak) |
| ✅ Expose settings via DependencyProperty | ❌ Use hardcoded values |
| ✅ Apply IsEnabled pattern | ❌ Always execute without condition |
| ✅ Check AssociatedObject for null | ❌ Use without null check |
