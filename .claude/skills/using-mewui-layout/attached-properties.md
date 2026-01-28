# Attached Properties

MewUI uses `ConditionalWeakTable` for attached properties (no DependencyProperty system).

## Pattern

```csharp
public class MyPanel : Panel
{
    private static readonly ConditionalWeakTable<Element, MyData> _attached = new();

    private class MyData { public int Priority; }

    public static int GetPriority(Element e) =>
        _attached.TryGetValue(e, out var d) ? d.Priority : 0;

    public static void SetPriority(Element e, int value)
    {
        var data = _attached.GetOrCreateValue(e);
        if (data.Priority != value)
        {
            data.Priority = value;
            e.InvalidateArrange();
        }
    }
}

// Fluent extension
public static T Priority<T>(this T el, int value) where T : Element
{
    MyPanel.SetPriority(el, value);
    return el;
}
```

## Built-in Attached Properties

### Grid

```csharp
element.Row(1)           // Grid.Row
element.Column(2)        // Grid.Column
element.RowSpan(2)       // Grid.RowSpan
element.ColumnSpan(3)    // Grid.ColumnSpan

// Convenience method
element.GridPosition(1, 2)              // Row, Column
element.GridPosition(1, 2, 2, 3)        // Row, Column, RowSpan, ColumnSpan
```

### Canvas

```csharp
element.CanvasLeft(10)   // Canvas.Left
element.CanvasTop(20)    // Canvas.Top
element.CanvasRight(10)  // Canvas.Right
element.CanvasBottom(20) // Canvas.Bottom

// Convenience method
element.CanvasPosition(10, 20)  // Left, Top
```

### DockPanel

```csharp
// Note: .DockTo() not .Dock()
element.DockTo(Dock.Top)
element.DockTo(Dock.Left)
element.DockTo(Dock.Bottom)
element.DockTo(Dock.Right)

// Convenience methods
element.DockTop()
element.DockLeft()
element.DockBottom()
element.DockRight()
```

## Key Benefits

| Feature | Description |
|---------|-------------|
| Weak references | Auto-cleanup when element disposed |
| No inheritance | Works with any Element |
| Thread-safe | ConditionalWeakTable is thread-safe |
| Lazy creation | Data created only when property set |
