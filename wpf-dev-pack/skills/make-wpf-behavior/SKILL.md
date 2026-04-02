---
description: "Generates WPF Behavior<T> classes using Microsoft.Xaml.Behaviors.Wpf. Use when adding reusable interaction logic to controls, creating drag behaviors, or scaffolding a new Behavior class. Usage: /wpf-dev-pack:make-wpf-behavior <BehaviorName>"
model: haiku
argument-hint: [BehaviorName]
---

# WPF Behavior Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the Behavior name (e.g., SelectAllOnFocus, DragMove)". Do NOT proceed until a valid name is provided. Use the response as the BehaviorName for all subsequent steps.**

Generate a `$0Behavior` class based on Microsoft.Xaml.Behaviors.Wpf.

- Replace `{TargetType}` with the appropriate WPF type (e.g., TextBox, UIElement, Window) based on the behavior name and context.
- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.
- Replace `{Project}` with the target project path.

## Generated Code

```csharp
namespace {Namespace}.Behaviors;

public sealed class $0Behavior : Behavior<{TargetType}>
{
    #region Dependency Properties

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(
            nameof(IsEnabled),
            typeof(bool),
            typeof($0Behavior),
            new PropertyMetadata(true));

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
        AssociatedObject.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnLoaded;
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
        xmlns:behaviors="clr-namespace:{Namespace}.Behaviors">

    <{TargetType}>
        <b:Interaction.Behaviors>
            <behaviors:$0Behavior IsEnabled="{Binding IsBehaviorEnabled}"/>
        </b:Interaction.Behaviors>
    </{TargetType}>

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
    └── $0Behavior.cs
```

## Best Practices

| DO | DON'T |
|----|-------|
| Unsubscribe events in OnDetaching | Missing event unsubscription (memory leak) |
| Expose settings via DependencyProperty | Use hardcoded values |
| Apply IsEnabled pattern | Always execute without condition |
| Check AssociatedObject for null | Use without null check |
