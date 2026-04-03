# Freezable Performance

Rules for using `Freeze()` on WPF Freezable objects to maximize rendering performance.

---

## Rule: Freeze All Freezable Objects

Call `Freeze()` on every `Freezable` after it is fully configured and before it is used for rendering.

Applies to: `Brush`, `Pen`, `Geometry`, `Transform`, `PathGeometry`, `GradientBrush`, `ImageBrush`, `DrawingGroup`, and all other `Freezable` subclasses.

---

## Why Freeze Matters

| Without Freeze | With Freeze |
|---|---|
| Object tracked for change notifications | No change tracking overhead |
| Cannot be shared across threads | Sharable across all threads |
| WPF retains full change-event infrastructure | Infrastructure freed immediately |
| Higher memory footprint | Minimal memory footprint |

Frozen objects are immutable. WPF can cache and reuse them across render passes without defensive copies.

---

## Correct vs Incorrect

```csharp
// Incorrect: brush created but never frozen
protected override void OnRender(DrawingContext dc)
{
    var brush = new SolidColorBrush(Colors.CornflowerBlue);
    var pen   = new Pen(brush, 2.0);
    dc.DrawRectangle(brush, pen, new Rect(0, 0, 100, 50));
}

// Correct: freeze immediately after configuration
protected override void OnRender(DrawingContext dc)
{
    var brush = new SolidColorBrush(Colors.CornflowerBlue);
    brush.Freeze();

    var pen = new Pen(brush, 2.0);
    pen.Freeze();

    dc.DrawRectangle(brush, pen, new Rect(0, 0, 100, 50));
}
```

---

## OnRender Pattern: Create in Constructor, Reuse in OnRender

Creating Freezable objects inside `OnRender` allocates on every paint cycle.
Create them once in the constructor (or a lazy initializer), freeze them, then reuse the frozen instances in every `OnRender` call.

```csharp
public class MyVisual : FrameworkElement
{
    private readonly SolidColorBrush _fillBrush;
    private readonly Pen             _borderPen;

    public MyVisual()
    {
        _fillBrush = new SolidColorBrush(Colors.CornflowerBlue);
        _fillBrush.Freeze();

        _borderPen = new Pen(Brushes.DarkBlue, 1.5);
        _borderPen.Freeze();
    }

    protected override void OnRender(DrawingContext dc)
    {
        // No allocation here — reuse frozen instances
        dc.DrawRectangle(_fillBrush, _borderPen, new Rect(0, 0, ActualWidth, ActualHeight));
    }
}
```

If a property changes and a new brush is needed, create and freeze the replacement, then call `InvalidateVisual()` once.
