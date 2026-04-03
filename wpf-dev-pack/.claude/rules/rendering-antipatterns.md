# Rendering Anti-Patterns

Common WPF rendering mistakes that cause performance problems or visual corruption.

---

## Anti-Pattern 1: InvalidateVisual() in a Loop

Calling `InvalidateVisual()` inside a loop schedules a separate render pass for each iteration.
WPF does not coalesce these — each call queues a layout/render cycle, causing frame drops or freezes.

```csharp
// Prohibited: InvalidateVisual inside a loop
foreach (var item in items)
{
    item.Value = ComputeNewValue(item);
    InvalidateVisual();   // ← queues N render passes
}
```

### Correct: Batch Update Pattern

Mutate all state first, then invalidate once after the loop completes.

```csharp
// Correct: one invalidation after all state is updated
foreach (var item in items)
{
    item.Value = ComputeNewValue(item);
}
InvalidateVisual();   // ← single render pass
```

For data-driven scenarios where items notify individually, suppress notifications during bulk update and raise a single reset at the end:

```csharp
// Correct: bulk update with deferred notification
_suppressNotifications = true;
try
{
    foreach (var item in items)
        item.Value = ComputeNewValue(item);
}
finally
{
    _suppressNotifications = false;
    InvalidateVisual();
}
```

---

## Anti-Pattern 2: Creating Resources in OnRender

`OnRender` is called on every render pass (layout changes, animations, window resize).
Allocating `Brush`, `Pen`, `Geometry`, or other objects inside `OnRender` creates garbage on every frame.

```csharp
// Prohibited: resource allocation inside OnRender
protected override void OnRender(DrawingContext dc)
{
    var brush = new SolidColorBrush(Colors.Red);   // ← allocated every frame
    var pen   = new Pen(brush, 1.0);               // ← allocated every frame
    dc.DrawEllipse(brush, pen, _center, _rx, _ry);
}
```

Move resource creation to the constructor or a property setter.
See `freezable-performance.md` for the correct constructor-initialize-and-freeze pattern.
