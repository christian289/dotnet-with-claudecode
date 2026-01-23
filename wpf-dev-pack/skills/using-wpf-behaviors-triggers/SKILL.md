---
name: using-wpf-behaviors-triggers
description: "Implements XAML behaviors and triggers using Microsoft.Xaml.Behaviors.Wpf. Use when adding interactivity to XAML without code-behind, implementing EventToCommand patterns, or creating reusable behaviors."
---

# WPF Behaviors and Triggers

## 1. Setup

### 1.1 Install NuGet Package

```xml
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.*" />
```

### 1.2 XAML Namespace

```xml
<Window xmlns:b="http://schemas.microsoft.com/xaml/behaviors">
```

---

## 2. EventTrigger

Executes actions when events occur.

### 2.1 InvokeCommandAction (MVVM Recommended)

```xml
<Button Content="Click Me">
    <b:Interaction.Triggers>
        <b:EventTrigger EventName="Click">
            <b:InvokeCommandAction Command="{Binding ClickCommand}"/>
        </b:EventTrigger>
    </b:Interaction.Triggers>
</Button>
```

### 2.2 With Event Args

```xml
<ListBox>
    <b:Interaction.Triggers>
        <b:EventTrigger EventName="SelectionChanged">
            <b:InvokeCommandAction Command="{Binding SelectionChangedCommand}"
                                   PassEventArgsToCommand="True"/>
        </b:EventTrigger>
    </b:Interaction.Triggers>
</ListBox>
```

### 2.3 ChangePropertyAction

```xml
<Button Content="Toggle Visibility">
    <b:Interaction.Triggers>
        <b:EventTrigger EventName="Click">
            <b:ChangePropertyAction TargetName="MyPanel"
                                    PropertyName="Visibility"
                                    Value="Collapsed"/>
        </b:EventTrigger>
    </b:Interaction.Triggers>
</Button>

<StackPanel x:Name="MyPanel"/>
```

---

## 3. DataTrigger

Executes actions based on data conditions.

```xml
<TextBlock Text="{Binding Status}">
    <b:Interaction.Triggers>
        <b:DataTrigger Binding="{Binding IsLoading}" Value="True">
            <b:ChangePropertyAction PropertyName="Text" Value="Loading..."/>
            <b:ChangePropertyAction PropertyName="Foreground" Value="Gray"/>
        </b:DataTrigger>
        <b:DataTrigger Binding="{Binding HasError}" Value="True">
            <b:ChangePropertyAction PropertyName="Foreground" Value="Red"/>
        </b:DataTrigger>
    </b:Interaction.Triggers>
</TextBlock>
```

---

## 4. Custom Behavior

Encapsulates reusable behaviors.

### 4.1 Basic Behavior

```csharp
public sealed class FocusOnLoadBehavior : Behavior<UIElement>
{
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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.Focus();
    }
}
```

```xml
<TextBox>
    <b:Interaction.Behaviors>
        <local:FocusOnLoadBehavior/>
    </b:Interaction.Behaviors>
</TextBox>
```

### 4.2 Behavior with DependencyProperty

```csharp
public sealed class SelectAllOnFocusBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(
            nameof(IsEnabled),
            typeof(bool),
            typeof(SelectAllOnFocusBehavior),
            new PropertyMetadata(true));

    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.GotFocus += OnGotFocus;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.GotFocus -= OnGotFocus;
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (IsEnabled)
        {
            AssociatedObject.SelectAll();
        }
    }
}
```

```xml
<TextBox Text="Select me on focus">
    <b:Interaction.Behaviors>
        <local:SelectAllOnFocusBehavior IsEnabled="{Binding IsSelectAllEnabled}"/>
    </b:Interaction.Behaviors>
</TextBox>
```

### 4.3 Drag and Drop Behavior

```csharp
public sealed class DragDropBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.Register(
            nameof(DropCommand),
            typeof(ICommand),
            typeof(DragDropBehavior));

    public ICommand? DropCommand
    {
        get => (ICommand?)GetValue(DropCommandProperty);
        set => SetValue(DropCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AllowDrop = true;
        AssociatedObject.Drop += OnDrop;
        AssociatedObject.DragOver += OnDragOver;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Drop -= OnDrop;
        AssociatedObject.DragOver -= OnDragOver;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            DropCommand?.Execute(files);
        }
    }
}
```

```xml
<Border Background="LightGray" MinHeight="100">
    <b:Interaction.Behaviors>
        <local:DragDropBehavior DropCommand="{Binding FileDroppedCommand}"/>
    </b:Interaction.Behaviors>
    <TextBlock Text="Drop files here" HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
</Border>
```

---

## 5. Custom TriggerAction

```csharp
public sealed class ShowMessageAction : TriggerAction<DependencyObject>
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(ShowMessageAction));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    protected override void Invoke(object parameter)
    {
        MessageBox.Show(Message, "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
```

```xml
<Button Content="Show Message">
    <b:Interaction.Triggers>
        <b:EventTrigger EventName="Click">
            <local:ShowMessageAction Message="Hello, World!"/>
        </b:EventTrigger>
    </b:Interaction.Triggers>
</Button>
```

---

## 6. Common Patterns

### 6.1 Close Window

```xml
<Button Content="Close">
    <b:Interaction.Triggers>
        <b:EventTrigger EventName="Click">
            <b:CallMethodAction TargetObject="{Binding RelativeSource={RelativeSource AncestorType=Window}}"
                                MethodName="Close"/>
        </b:EventTrigger>
    </b:Interaction.Triggers>
</Button>
```

### 6.2 Focus Next on Enter

```csharp
public sealed class MoveNextOnEnterBehavior : Behavior<UIElement>
{
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
            AssociatedObject.MoveFocus(
                new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
```

---

## 7. Best Practices

| DO | DON'T |
|----|-------|
| ✅ Implement `OnDetaching` in Behavior (prevent memory leaks) | ❌ Write event handlers directly in code-behind |
| ✅ Follow MVVM pattern (InvokeCommandAction) | ❌ Reference ViewModel directly in Behavior |
| ✅ Make configurable via DependencyProperty | ❌ Use hardcoded values |
| ✅ Design reusable Behaviors | ❌ Behaviors that only work with specific controls |

---

## 8. Related Skills

- `handling-wpf-input-commands` - ICommand, RoutedCommand
- `routing-wpf-events` - Routed Events
- `implementing-wpf-dragdrop` - Drag and Drop
