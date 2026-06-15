namespace ClaudeDesk.Behaviors;

public sealed class EnterCommandBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EnterCommandBehavior), new(null));
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(EnterCommandBehavior), new(null));
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttached() => AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    protected override void OnDetaching() => AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // IME guard FIRST: an IME-composition confirm arrives as Key.ImeProcessed
        // (never Key.Enter), so it must not send.
        if (e.Key == Key.ImeProcessed) { return; }
        if (e.Key != Key.Enter) { return; }
        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) { return; } // Shift+Enter -> newline (default)
        if (Command?.CanExecute(CommandParameter) == true) { Command.Execute(CommandParameter); }
        e.Handled = true;                                              // suppress the newline insert
    }
}
