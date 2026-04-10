# WPF Button Keyboard Behavior

Rules for WPF Button keyboard input handling. Required reading when implementing dialog buttons.

---

## Button Handles Space but Ignores Enter

When a `System.Windows.Controls.Button` has keyboard focus:
- **Space** → Triggers Click (starts on KeyDown, completes on KeyUp)
- **Enter** → Ignored (only `IsDefault="True"` buttons are exempt)

If users expect focused buttons to respond to Enter, additional handling is required:

```csharp
// Handle Enter key via Behavior<ButtonBase>
private void OnKeyDown(object sender, KeyEventArgs e)
{
    if (e.Key != Key.Enter)
    {
        return;
    }

    if (AssociatedObject.Command is { } command && command.CanExecute(AssociatedObject.CommandParameter))
    {
        command.Execute(AssociatedObject.CommandParameter);
        e.Handled = true;
    }
}
```

## IsDefault Works Regardless of Focus Location

An `IsDefault="True"` button always intercepts the Enter key. The IsDefault button is triggered even when another button has focus.

→ This **conflicts** with UX patterns where users select a button with arrow keys and execute it with Enter.

## IsCancel Is Safe Regardless of Focus

An `IsCancel="True"` button responds to the ESC key. Since ESC always means "close/cancel," it does not conflict with focus location.

However, if all you need is to close the Window, `Window.Close()` is simpler.
