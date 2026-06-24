# WPF Dialog Focus Timing

Causes of `Keyboard.Focus()` failures in dialogs and the patterns used to fix them.

---

## Keyboard.Focus() Only Works When the Window Is Active

`Keyboard.Focus(element)` silently fails if the Window containing the element is not in the Active state.

Dialogs opened directly by a user click activate their Window immediately, but dialogs opened from async exception handlers or similar code paths may still have an inactive Window at `Loaded` time.

### Fix: Try at Loaded + Retry at Window.Activated

```csharp
private void OnLoaded(object sender, RoutedEventArgs e)
{
    _window = Window.GetWindow(AssociatedObject);
    if (_window is not null)
    {
        _window.Activated += OnWindowActivated;
    }

    TryFocus(); // Immediate attempt (normal path)
}

private void OnWindowActivated(object? sender, EventArgs e)
{
    _window!.Activated -= OnWindowActivated;
    TryFocus(); // Retry after Window activation (abnormal path)
}
```

## ItemContainerGenerator.StatusChanged ≠ Visual Tree Ready

Even when the `ContainersGenerated` event fires, child elements inside a DataTemplate (e.g., Button) may not yet be in the visual tree. `FindVisualChild<Button>()` will fail.

→ Do not rely on `StatusChanged`; walk the visual tree only after the Window has been activated.

## Use DispatcherPriority.ApplicationIdle

Scheduling focus with `DispatcherPriority.ApplicationIdle` defers execution until all layout, rendering, and binding work has completed, which prevents focus from being stolen by other work.

```csharp
button.Dispatcher.BeginInvoke(() =>
{
    button.Focus();
    Keyboard.Focus(button);
}, DispatcherPriority.ApplicationIdle);
```
