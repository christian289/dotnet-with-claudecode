namespace ClaudeDesk.Behaviors;

/// <summary>
/// PasswordBox.Password is deliberately NOT a DependencyProperty, so it cannot
/// be a binding target. This attached property surfaces the entered password
/// one-way out — bind with Mode=OneWayToSource.
/// </summary>
public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
        "BoundPassword", typeof(string), typeof(PasswordBoxAssistant),
        new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged)
        {
            BindsTwoWayByDefault = false,
        });

    public static string GetBoundPassword(DependencyObject o) => (string)o.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject o, string v) => o.SetValue(BoundPasswordProperty, v);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) { return; }
        pb.PasswordChanged -= Handler; // idempotent re-attach (guards re-entrancy)
        pb.PasswordChanged += Handler;

        static void Handler(object s, RoutedEventArgs _)
        {
            var box = (PasswordBox)s;
            SetBoundPassword(box, box.Password); // push entered value back to the bound property
        }
    }
}
