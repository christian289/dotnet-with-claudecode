namespace ClaudeDesk.Behaviors;

public sealed class StickToBottomBehavior : Behavior<ScrollViewer>
{
    private bool _pinned = true;

    protected override void OnAttached() => AssociatedObject.ScrollChanged += OnScrollChanged;
    protected override void OnDetaching() => AssociatedObject.ScrollChanged -= OnScrollChanged;

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var sv = AssociatedObject;
        if (e.ExtentHeightChange == 0)
        {
            // user moved (no content growth): recompute whether we're at the bottom
            _pinned = sv.VerticalOffset >= sv.ScrollableHeight - 1.0;
        }
        else if (_pinned)
        {
            // content grew while pinned: follow it
            sv.ScrollToVerticalOffset(sv.ScrollableHeight);
        }
    }
}
