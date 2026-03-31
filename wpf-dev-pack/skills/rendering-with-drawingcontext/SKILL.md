---
description: "Renders high-performance graphics using WPF DrawingContext for 10-50x improvement over Shape. Use when drawing large numbers of shapes or optimizing rendering performance."
---

# WPF DrawingContext High-Performance Rendering

A pattern for achieving 10-50x performance improvement over Shape objects when rendering large numbers of shapes in WPF using DrawingContext.

## 1. Core Concepts

### Shape vs DrawingContext Approach

| Item | Shape (Polygon, Rectangle, etc.) | DrawingContext |
|------|----------------------------------|----------------|
| **Inheritance** | Canvas | FrameworkElement |
| **Visual count** | One per shape (n) | 1 |
| **Layout calculation** | O(n) Measure/Arrange | O(1) |
| **Memory usage** | Very high (WPF object overhead) | Very low (data only) |
| **Performance** | Baseline | **10-50x faster** |
| **Suitable for** | Few interactive shapes (tens to hundreds) | Large static shapes (thousands to tens of thousands) |

### Why is DrawingContext Fast?

1. **Single Visual**: Only 1 FrameworkElement registered in Visual Tree
2. **Layout bypass**: No Measure/Arrange calculations needed
3. **Batch rendering**: Sent to GPU as single batch
4. **Memory efficiency**: Only stores shape metadata

---

## 2. Basic Implementation Pattern

### 2.1 DrawingContext-Based Custom Control

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Media;

public sealed class HighPerformanceCanvas : FrameworkElement
{
    // 1. Struct for storing shape data (lightweight)
    private readonly record struct ShapeData(
        Point Position,
        double Width,
        double Height,
        Brush Fill);

    // 2. Only rendering data stored in memory
    private readonly List<ShapeData> _shapes = [];

    // 3. Optimized Pen (Freeze applied)
    private readonly Pen _pen = new(Brushes.Black, 1);

    public HighPerformanceCanvas()
    {
        // Freeze Pen for performance optimization
        _pen.Freeze();
    }

    // 4. Shape addition method
    public void AddShape(Point position, double width, double height, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();  // Freeze for performance optimization

        _shapes.Add(new ShapeData(position, width, height, brush));
    }

    // 5. Trigger rendering (call once after data addition is complete)
    public void Render()
    {
        InvalidateVisual();
    }

    // 6. Actual rendering - direct drawing in OnRender
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        foreach (var shape in _shapes)
        {
            dc.DrawRectangle(
                shape.Fill,
                _pen,
                new Rect(shape.Position, new Size(shape.Width, shape.Height)));
        }
    }

    // 7. Clear shapes
    public void Clear()
    {
        _shapes.Clear();
        InvalidateVisual();
    }
}
```

---

> **Advanced**: See [ADVANCED.md](ADVANCED.md) for complex shapes (StreamGeometry triangles/polygons), async rendering with performance measurement, MVVM integration (delegate pattern), and Shape approach comparison.

---

## 3. Key Optimization Techniques

### 3.1 Freeze() - Making Objects Immutable

```csharp
// ✅ Pen optimization
private readonly Pen _pen = new(Brushes.Black, 1);
public MyControl()
{
    _pen.Freeze();  // WPF can optimize internally
}

// ✅ Brush optimization
var brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
brush.Freeze();  // Can be shared in memory

// ✅ Geometry optimization
var geometry = new StreamGeometry();
// ... configure geometry ...
geometry.Freeze();  // Rendering pipeline optimization
```

### 3.2 Using record struct

```csharp
// ✅ Value type (stack allocation) → Memory efficient
private readonly record struct ShapeData(
    Point Position,
    Size Size,
    Brush Fill);

// Auto-generated Equals, GetHashCode
// Immutable semantics enforced
```

### 3.3 StreamGeometry vs PathGeometry

```csharp
// ✅ StreamGeometry - Lightweight, write-only
var geometry = new StreamGeometry();
using (var ctx = geometry.Open())
{
    ctx.BeginFigure(startPoint, true, true);
    ctx.LineTo(point2, true, false);
}

// ❌ PathGeometry - Relatively heavyweight
var geometry = new PathGeometry();
var figure = new PathFigure { StartPoint = startPoint };
figure.Segments.Add(new LineSegment(point2, true));
```

---

## 4. InvalidateVisual() Cautions

### O(n²) Complexity Pattern

```csharp
// ❌ Bad example: Calling InvalidateVisual() inside loop
for (int i = 0; i < count; i++)
{
    _items.Add(data);
    if (i % 10 == 0)
    {
        InvalidateVisual();  // OnRender iterates entire _items!
    }
}
// Result: 10 + 20 + ... + n = O(n²)
```

### ✅ Correct Pattern: Call Once at the End

```csharp
// ✅ Good example: Render only once after data collection
for (int i = 0; i < count; i++)
{
    _items.Add(data);
}

// Render only once at the end
InvalidateVisual();
```

**Performance Difference**:
- Bad pattern: 10,000 items takes **several seconds**
- Correct pattern: 10,000 items takes **tens of ms**

---

## 5. Checklist

- [ ] Inherit from FrameworkElement (instead of Canvas)
- [ ] Apply Freeze() to Pen, Brush
- [ ] Store shape data as record struct
- [ ] Use StreamGeometry for complex shapes
- [ ] Call InvalidateVisual() **only once** after data addition is complete
- [ ] Use Dispatcher.InvokeAsync to yield UI during large data generation
- [ ] ViewModel uses delegate pattern without View type reference

---

## 6. References

- [DrawingContext - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.drawingcontext)
- [StreamGeometry - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.streamgeometry)
- [Optimizing WPF Performance - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-2d-graphics-and-imaging)
