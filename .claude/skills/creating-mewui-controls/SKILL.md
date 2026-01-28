---
name: creating-mewui-controls
description: "Creates custom MewUI controls with proper measure, arrange, render, and input handling. Use when building new interactive controls, extending existing controls, or implementing custom rendering."
---

## Control Base Classes

| Base | Use Case |
|------|----------|
| `Control` | Interactive elements (buttons, inputs) |
| `ContentControl` | Single-child containers |
| `Panel` | Multi-child layouts |

---

## Basic Control Structure

```csharp
public class MyButton : Control
{
    private bool _isPressed;

    // Property with invalidation
    public string Text
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                InvalidateMeasure();
            }
        }
    } = "";

    // Enable focus by overriding (not setting property)
    public override bool Focusable => true;

    // Measure: calculate desired size
    protected override Size MeasureContent(Size availableSize)
    {
        var factory = GetGraphicsFactory();
        using var ctx = factory.CreateMeasurementContext(GetDpi());
        var textSize = ctx.MeasureText(Text, GetFont());
        return new Size(textSize.Width + Padding.Horizontal, textSize.Height + Padding.Vertical);
    }

    // Render: draw the control
    protected override void OnRender(IGraphicsContext context)
    {
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

        var bgColor = _isPressed ? Theme.Palette.ButtonPressedBackground
            : IsMouseOver ? Theme.Palette.ButtonHoverBackground
            : Theme.Palette.ButtonFace;

        context.FillRoundedRectangle(bounds, 4, 4, bgColor);
        context.DrawRoundedRectangle(bounds, 4, 4, Theme.Palette.BorderColor, 1);
        context.DrawText(Text, bounds, GetFont(), Foreground,
            TextAlignment.Center, TextAlignment.Center, TextWrapping.NoWrap);
    }
}
```

---

## Input Handling

```csharp
// Mouse - note: MouseEventArgs, not MouseButtonEventArgs
protected override void OnMouseDown(MouseEventArgs e)
{
    if (e.LeftButton)
    {
        _isPressed = true;
        // Capture via Window
        var root = FindVisualRoot();
        if (root is Window window)
            window.CaptureMouse(this);
        InvalidateVisual();
        e.Handled = true;
    }
}

protected override void OnMouseUp(MouseEventArgs e)
{
    if (_isPressed)
    {
        _isPressed = false;
        var root = FindVisualRoot();
        if (root is Window window)
            window.ReleaseMouseCapture();
        if (IsMouseOver) Click?.Invoke(this, EventArgs.Empty);
        InvalidateVisual();
    }
}

// Keyboard
protected override void OnKeyDown(KeyEventArgs e)
{
    if (e.Key == Key.Space || e.Key == Key.Enter)
    {
        Click?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }
}
```

---

## Adding Binding Support

```csharp
private ValueBinding<string>? _textBinding;

public void SetTextBinding(Func<string> get, Action<string>? set = null,
    Action<Action>? subscribe = null, Action<Action>? unsubscribe = null)
{
    _textBinding?.Dispose();
    _textBinding = new ValueBinding<string>(get, set, subscribe, unsubscribe, () => Text = get());
    // RegisterBinding is internal - use within MewUI assembly
    Text = get();
}
```

---

## Fluent Extensions

```csharp
public static class MyButtonExtensions
{
    public static MyButton Text(this MyButton btn, string text) { btn.Text = text; return btn; }
    public static MyButton OnClick(this MyButton btn, Action handler) { btn.Click += (s,e) => handler(); return btn; }
    public static MyButton BindText(this MyButton btn, ObservableValue<string> src)
    {
        btn.SetTextBinding(() => src.Value, v => src.Value = v,
            h => src.Changed += h, h => src.Changed -= h);
        return btn;
    }
}
```

---

**Input details**: See [input-handling.md](input-handling.md)
**ContentControl pattern**: See [content-control.md](content-control.md)
