# Creating Custom Panels

## Panel Base Pattern

```csharp
public class MyPanel : Panel
{
    public double Spacing { get; set => { field = value; InvalidateMeasure(); } }

    protected override Size MeasureContent(Size availableSize)
    {
        var paddedSize = availableSize.Deflate(Padding);  // Handle padding

        // Measure all children
        foreach (var child in Children)
        {
            // Note: IsVisible is on UIElement, need cast
            if (child is UIElement ui && !ui.IsVisible) continue;
            child.Measure(paddedSize);
        }
        // Return total desired size (inflate with padding)
        return CalculateTotalSize().Inflate(Padding);
    }

    // Note: ArrangeContent returns VOID, not Size
    protected override void ArrangeContent(Rect bounds)
    {
        var contentBounds = bounds.Deflate(Padding);  // Handle padding

        // Position each child
        foreach (var child in Children)
        {
            if (child is UIElement ui && !ui.IsVisible) continue;
            child.Arrange(CalculateChildBounds(child, contentBounds));
        }
        // No return - method is void
    }

    // Optional: respond to child collection changes
    protected override void OnChildAdded(Element child) { }
    protected override void OnChildRemoved(Element child) { }
}
```

## Example: RadialPanel

```csharp
public class RadialPanel : Panel
{
    public double Radius { get; set => { field = value; InvalidateMeasure(); } } = 100;

    protected override Size MeasureContent(Size availableSize)
    {
        var paddedSize = availableSize.Deflate(Padding);

        foreach (var child in Children)
        {
            if (child is UIElement ui && !ui.IsVisible) continue;
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }
        return new Size(Radius * 2, Radius * 2).Inflate(Padding);
    }

    protected override void ArrangeContent(Rect bounds)
    {
        var contentBounds = bounds.Deflate(Padding);
        var center = new Point(contentBounds.Width / 2, contentBounds.Height / 2);

        var visible = new List<Element>();
        foreach (var child in Children)
            if (child is UIElement ui && ui.IsVisible)
                visible.Add(child);

        if (visible.Count == 0) return;

        var angleStep = 360.0 / visible.Count;
        var angle = -90.0;  // Start at top

        foreach (var child in visible)
        {
            var radians = angle * Math.PI / 180;
            var x = contentBounds.X + center.X + Radius * Math.Cos(radians) - child.DesiredSize.Width / 2;
            var y = contentBounds.Y + center.Y + Radius * Math.Sin(radians) - child.DesiredSize.Height / 2;
            child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
            angle += angleStep;
        }
        // No return - void method
    }
}
```

## Example: TilePanel

```csharp
public class TilePanel : Panel
{
    public double TileWidth { get; set => { field = value; InvalidateMeasure(); } } = 100;
    public double TileHeight { get; set => { field = value; InvalidateMeasure(); } } = 100;
    public double Spacing { get; set => { field = value; InvalidateMeasure(); } } = 8;

    protected override Size MeasureContent(Size availableSize)
    {
        var paddedSize = availableSize.Deflate(Padding);

        foreach (var child in Children)
        {
            if (child is UIElement ui && !ui.IsVisible) continue;
            child.Measure(new Size(TileWidth, TileHeight));
        }

        int cols = Math.Max(1, (int)((paddedSize.Width + Spacing) / (TileWidth + Spacing)));
        int rows = (int)Math.Ceiling((double)Children.Count / cols);
        return new Size(
            cols * TileWidth + (cols - 1) * Spacing,
            rows * TileHeight + (rows - 1) * Spacing
        ).Inflate(Padding);
    }

    protected override void ArrangeContent(Rect bounds)
    {
        var contentBounds = bounds.Deflate(Padding);
        int cols = Math.Max(1, (int)((contentBounds.Width + Spacing) / (TileWidth + Spacing)));
        int col = 0, row = 0;

        foreach (var child in Children)
        {
            if (child is UIElement ui && !ui.IsVisible) continue;

            child.Arrange(new Rect(
                contentBounds.X + col * (TileWidth + Spacing),
                contentBounds.Y + row * (TileHeight + Spacing),
                TileWidth, TileHeight
            ));
            if (++col >= cols) { col = 0; row++; }
        }
        // No return - void method
    }
}
```

## Tips

1. Check visibility with cast: `child is UIElement ui && !ui.IsVisible`
2. Call `InvalidateMeasure()` for size-affecting properties
3. Call `InvalidateArrange()` for position-only properties (like Canvas.Left)
4. Handle `Padding` via `Deflate()` / `Inflate()`
5. Handle empty children case
6. Override `OnChildAdded`/`OnChildRemoved` for cleanup (see Canvas)
