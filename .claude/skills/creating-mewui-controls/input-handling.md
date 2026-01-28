# MewUI Input Handling

## Mouse Events

All mouse events use `MouseEventArgs` (no separate `MouseButtonEventArgs`):

| Event | Description |
|-------|-------------|
| `OnMouseDown(MouseEventArgs)` | Button pressed - check `e.Button` or `e.LeftButton` |
| `OnMouseUp(MouseEventArgs)` | Button released |
| `OnMouseMove(MouseEventArgs)` | Mouse moved |
| `OnMouseEnter()` | Mouse entered bounds (no parameters) |
| `OnMouseLeave()` | Mouse left bounds (no parameters) |
| `OnMouseWheel(MouseWheelEventArgs)` | Wheel scrolled |

## Mouse Capture

Capture is managed through the Window, not the element:

```csharp
// Capture mouse (receive events even when mouse leaves)
var root = FindVisualRoot();
if (root is Window window)
    window.CaptureMouse(this);

// Release capture
var root = FindVisualRoot();
if (root is Window window)
    window.ReleaseMouseCapture();

// Check state
bool IsMouseCaptured;   // Read-only property on UIElement
bool IsMouseOver;       // Mouse is over element
```

## Mouse Position

```csharp
// Position is relative to the element that received the event
Point local = e.Position;

// Screen coordinates
Point screen = e.ScreenPosition;

// Note: No GetPosition(element) method - Position is already element-relative
```

## Keyboard Events

| Event | Description |
|-------|-------------|
| `OnKeyDown(KeyEventArgs)` | Key pressed |
| `OnKeyUp(KeyEventArgs)` | Key released |
| `OnTextInput(TextInputEventArgs)` | Character typed |

## Key Handling

```csharp
protected override void OnKeyDown(KeyEventArgs e)
{
    bool ctrl = e.Modifiers.HasFlag(ModifierKeys.Control);

    switch (e.Key)
    {
        case Key.Enter: Submit(); e.Handled = true; break;
        case Key.Escape: Cancel(); e.Handled = true; break;
    }

    if (ctrl && e.Key == Key.S) { Save(); e.Handled = true; }
}
```

## Focus

```csharp
// Enable focus by OVERRIDING the property (not setting)
public override bool Focusable => true;

Focus();             // Request focus (returns bool)
bool IsFocused;      // Check focus state

// Focus events take NO parameters
protected override void OnGotFocus() { InvalidateVisual(); }
protected override void OnLostFocus() { InvalidateVisual(); }
```

## Hit Testing

```csharp
// Custom hit test region (e.g., circular button)
// Note: point is in parent coordinates, use Bounds for position
protected override UIElement? OnHitTest(Point point)
{
    if (!IsVisible || !IsHitTestVisible || !IsEffectivelyEnabled)
        return null;

    var center = new Point(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
    var distance = (point - center).Length;
    var radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
    return distance <= radius ? this : null;
}
```

## Drag Implementation

```csharp
private bool _isDragging;
private Point _dragStart;

protected override void OnMouseDown(MouseEventArgs e)  // Note: MouseEventArgs
{
    if (e.LeftButton)
    {
        _isDragging = true;
        _dragStart = e.Position;

        // Capture via Window
        var root = FindVisualRoot();
        if (root is Window window)
            window.CaptureMouse(this);

        e.Handled = true;
    }
}

protected override void OnMouseMove(MouseEventArgs e)
{
    if (_isDragging)
    {
        var delta = e.Position - _dragStart;
        // Update position using Canvas attached properties
        this.CanvasLeft(Canvas.GetLeft(this) + delta.X);
        this.CanvasTop(Canvas.GetTop(this) + delta.Y);
        _dragStart = e.Position;
    }
}

protected override void OnMouseUp(MouseEventArgs e)  // Note: MouseEventArgs
{
    if (_isDragging)
    {
        _isDragging = false;
        var root = FindVisualRoot();
        if (root is Window window)
            window.ReleaseMouseCapture();
    }
}
```
