# Hit Testing

Determines which element is at a given point.

## Coordinate System

**Important**: MewUI uses parent-relative coordinates for hit testing. The `point` parameter contains coordinates relative to the parent element (or window), NOT local coordinates.

## Default Hit Test

```csharp
// In UIElement
public UIElement? HitTest(Point point) => OnHitTest(point);

protected virtual UIElement? OnHitTest(Point point)
{
    // Note: checks IsEffectivelyEnabled (considers parent enabled state)
    if (!IsVisible || !IsHitTestVisible || !IsEffectivelyEnabled)
        return null;

    // Point is in parent coordinates, compare with Bounds
    if (Bounds.Contains(point))
        return this;

    return null;
}
```

## Panel Hit Testing

Tests children in reverse order (topmost first). Note: no point translation occurs.

```csharp
protected override UIElement? OnHitTest(Point point)
{
    if (!IsVisible || !IsHitTestVisible || !IsEffectivelyEnabled)
        return null;

    // Hit test children in reverse order (top to bottom in visual order)
    for (int i = _children.Count - 1; i >= 0; i--)
    {
        if (_children[i] is UIElement child)
        {
            // Note: pass point unchanged - no translation
            var result = child.HitTest(point);
            if (result != null) return result;
        }
    }

    // Then check self
    if (Bounds.Contains(point))
        return this;

    return null;
}
```

## Window Hit Testing

Window tests popups first, then content:

```csharp
protected override UIElement? OnHitTest(Point point)
{
    // Test popups first (in reverse z-order)
    for (int i = _popups.Count - 1; i >= 0; i--)
    {
        if (!_popups[i].Bounds.Contains(point))
            continue;

        var hit = _popups[i].Element.HitTest(point);
        if (hit != null)
            return hit;
    }

    // Then test content
    return (Content as UIElement)?.HitTest(point);
}
```

## Custom Hit Test Regions

### Circular Button

```csharp
protected override UIElement? OnHitTest(Point point)
{
    if (!IsVisible || !IsHitTestVisible || !IsEffectivelyEnabled)
        return null;

    // Note: use Bounds position since point is in parent coordinates
    var center = new Point(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
    var distance = (point - center).Length;
    var radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
    return distance <= radius ? this : null;
}
```

### Donut Shape (Ring)

```csharp
protected override UIElement? OnHitTest(Point point)
{
    if (!IsVisible || !IsHitTestVisible || !IsEffectivelyEnabled)
        return null;

    var center = new Point(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
    var distance = (point - center).Length;
    return distance >= InnerRadius && distance <= OuterRadius ? this : null;
}
```

### Pass-Through (Transparent)

```csharp
protected override UIElement? OnHitTest(Point point)
{
    return null;  // Always pass through to elements behind
}
```

### Hotspots Only

```csharp
private readonly List<Rect> _hotspots = new();

protected override UIElement? OnHitTest(Point point)
{
    if (!IsVisible || !IsHitTestVisible || !IsEffectivelyEnabled)
        return null;

    // Hotspots are stored in local coordinates, adjust point
    var localPoint = new Point(point.X - Bounds.X, point.Y - Bounds.Y);
    foreach (var hotspot in _hotspots)
        if (hotspot.Contains(localPoint)) return this;
    return null;
}
```

## Hit Test Properties

| Property | Description |
|----------|-------------|
| `IsHitTestVisible` | If false, hit test returns null |
| `IsEffectivelyEnabled` | Considers parent enabled state |
| `IsVisible` | If false, hit test returns null |

## Point-in-Triangle Test

```csharp
private static bool PointInTriangle(Point p, Point v1, Point v2, Point v3)
{
    double Sign(Point p1, Point p2, Point p3) =>
        (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);

    double d1 = Sign(p, v1, v2);
    double d2 = Sign(p, v2, v3);
    double d3 = Sign(p, v3, v1);

    bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
    bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;

    return !(hasNeg && hasPos);
}
```
