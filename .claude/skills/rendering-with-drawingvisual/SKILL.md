---
name: rendering-with-drawingvisual
description: Implements lightweight rendering using WPF DrawingVisual with ContainerVisual, VisualCollection, and DrawingContext. Use when rendering large-scale graphics, charts, game graphics, or custom visuals.
---

# WPF DrawingVisual Patterns

DrawingVisual is a lightweight visual element faster than Shape, suitable for large-scale rendering.

## 1. Visual Hierarchy

```
Visual (abstract)
├── UIElement
│   └── FrameworkElement
│       └── Shape (heavyweight, event support)
│
├── DrawingVisual       ← Lightweight, no events, direct rendering
├── ContainerVisual     ← Groups multiple Visuals
└── HostVisual          ← Cross-thread Visual
```

---

## 2. DrawingVisual vs Shape

| Aspect | DrawingVisual | Shape |
|--------|--------------|-------|
| **Overhead** | Low | High |
| **Layout** | Non-participating | Participating |
| **Events** | Manual implementation | Built-in support |
| **Data binding** | Not available | Available |
| **Use case** | Large elements, performance critical | Interactive UI |

---

## 3. Basic DrawingVisual Host

### 3.1 FrameworkElement Host

```csharp
namespace MyApp.Controls;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

/// <summary>
/// Control that hosts DrawingVisual
/// </summary>
public sealed class DrawingVisualHost : FrameworkElement
{
    private readonly List<Visual> _visuals = [];

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Visual GetVisualChild(int index)
    {
        return _visuals[index];
    }

    /// <summary>
    /// Add Visual
    /// </summary>
    public void AddVisual(Visual visual)
    {
        _visuals.Add(visual);
        AddVisualChild(visual);
        AddLogicalChild(visual);
    }

    /// <summary>
    /// Remove Visual
    /// </summary>
    public void RemoveVisual(Visual visual)
    {
        _visuals.Remove(visual);
        RemoveVisualChild(visual);
        RemoveLogicalChild(visual);
    }

    /// <summary>
    /// Remove all Visuals
    /// </summary>
    public void ClearVisuals()
    {
        foreach (var visual in _visuals)
        {
            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);
        }
        _visuals.Clear();
    }

    /// <summary>
    /// Find Visual at coordinate (Hit Testing)
    /// </summary>
    public Visual? GetVisualAt(Point point)
    {
        var hitResult = VisualTreeHelper.HitTest(this, point);
        return hitResult?.VisualHit;
    }
}
```

### 3.2 Creating DrawingVisual

```csharp
namespace MyApp.Graphics;

using System.Windows;
using System.Windows.Media;

public static class DrawingVisualFactory
{
    /// <summary>
    /// Create circular DrawingVisual
    /// </summary>
    public static DrawingVisual CreateCircle(
        Point center,
        double radius,
        Brush fill,
        Pen? stroke = null)
    {
        var visual = new DrawingVisual();

        using (var dc = visual.RenderOpen())
        {
            dc.DrawEllipse(fill, stroke, center, radius, radius);
        }

        return visual;
    }

    /// <summary>
    /// Create rectangle DrawingVisual
    /// </summary>
    public static DrawingVisual CreateRectangle(
        Rect rect,
        Brush fill,
        Pen? stroke = null)
    {
        var visual = new DrawingVisual();

        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(fill, stroke, rect);
        }

        return visual;
    }

    /// <summary>
    /// Create text DrawingVisual
    /// </summary>
    public static DrawingVisual CreateText(
        string text,
        Point origin,
        Brush foreground,
        double fontSize = 12)
    {
        var visual = new DrawingVisual();

        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            fontSize,
            foreground,
            VisualTreeHelper.GetDpi(visual).PixelsPerDip);

        using (var dc = visual.RenderOpen())
        {
            dc.DrawText(formattedText, origin);
        }

        return visual;
    }

    /// <summary>
    /// Create image DrawingVisual
    /// </summary>
    public static DrawingVisual CreateImage(
        ImageSource image,
        Rect rect)
    {
        var visual = new DrawingVisual();

        using (var dc = visual.RenderOpen())
        {
            dc.DrawImage(image, rect);
        }

        return visual;
    }
}
```

---

## 4. ContainerVisual (Grouping)

### 4.1 Using ContainerVisual

```csharp
namespace MyApp.Graphics;

using System.Windows;
using System.Windows.Media;

public sealed class VisualGroup
{
    public ContainerVisual Container { get; } = new();

    /// <summary>
    /// Add child Visual
    /// </summary>
    public void Add(Visual visual)
    {
        Container.Children.Add(visual);
    }

    /// <summary>
    /// Remove child Visual
    /// </summary>
    public void Remove(Visual visual)
    {
        Container.Children.Remove(visual);
    }

    /// <summary>
    /// Move entire group
    /// </summary>
    public void SetOffset(double x, double y)
    {
        Container.Offset = new Vector(x, y);
    }

    /// <summary>
    /// Transform entire group
    /// </summary>
    public void SetTransform(Transform transform)
    {
        Container.Transform = transform;
    }

    /// <summary>
    /// Set opacity for entire group
    /// </summary>
    public void SetOpacity(double opacity)
    {
        Container.Opacity = opacity;
    }
}
```

### 4.2 Hierarchical Visual Structure

```csharp
// Hierarchical structure example
//
// ContainerVisual (root)
// ├── ContainerVisual (layer 1 - background)
// │   ├── DrawingVisual (grid)
// │   └── DrawingVisual (background image)
// ├── ContainerVisual (layer 2 - content)
// │   ├── DrawingVisual (node 1)
// │   ├── DrawingVisual (node 2)
// │   └── DrawingVisual (connections)
// └── ContainerVisual (layer 3 - overlay)
//     └── DrawingVisual (selection area)

public sealed class LayeredCanvas : FrameworkElement
{
    private readonly ContainerVisual _rootVisual = new();
    private readonly ContainerVisual _backgroundLayer = new();
    private readonly ContainerVisual _contentLayer = new();
    private readonly ContainerVisual _overlayLayer = new();

    public LayeredCanvas()
    {
        _rootVisual.Children.Add(_backgroundLayer);
        _rootVisual.Children.Add(_contentLayer);
        _rootVisual.Children.Add(_overlayLayer);

        AddVisualChild(_rootVisual);
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => _rootVisual;

    public void AddToBackground(DrawingVisual visual)
    {
        _backgroundLayer.Children.Add(visual);
    }

    public void AddToContent(DrawingVisual visual)
    {
        _contentLayer.Children.Add(visual);
    }

    public void AddToOverlay(DrawingVisual visual)
    {
        _overlayLayer.Children.Add(visual);
    }
}
```

---

## 5. Hit Testing

### 5.1 Basic Hit Testing

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

public sealed class InteractiveDrawingHost : FrameworkElement
{
    private readonly List<DrawingVisual> _visuals = [];
    private DrawingVisual? _hoveredVisual;
    private DrawingVisual? _selectedVisual;

    public InteractiveDrawingHost()
    {
        MouseMove += OnMouseMove;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    // ... VisualChildrenCount, GetVisualChild implementation omitted ...

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(this);
        var hitVisual = HitTestVisual(position);

        if (hitVisual != _hoveredVisual)
        {
            // Hover state changed
            _hoveredVisual = hitVisual;
            Cursor = hitVisual is not null ? Cursors.Hand : Cursors.Arrow;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this);
        _selectedVisual = HitTestVisual(position);

        if (_selectedVisual is not null)
        {
            // Handle selected Visual
            OnVisualSelected(_selectedVisual);
        }
    }

    private DrawingVisual? HitTestVisual(Point point)
    {
        DrawingVisual? result = null;

        VisualTreeHelper.HitTest(
            this,
            null,
            hitResult =>
            {
                if (hitResult.VisualHit is DrawingVisual visual)
                {
                    result = visual;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(point));

        return result;
    }

    private void OnVisualSelected(DrawingVisual visual)
    {
        // Raise selection event
    }
}
```

### 5.2 Geometry-based Hit Testing

```csharp
/// <summary>
/// Find all Visuals within specific area
/// </summary>
public List<DrawingVisual> HitTestArea(Rect area)
{
    var results = new List<DrawingVisual>();
    var geometry = new RectangleGeometry(area);

    VisualTreeHelper.HitTest(
        this,
        null,
        hitResult =>
        {
            if (hitResult.VisualHit is DrawingVisual visual)
            {
                results.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        },
        new GeometryHitTestParameters(geometry));

    return results;
}
```

---

## 6. Large-scale Rendering Example (Scatter Plot)

### 6.1 ScatterPlot Control

```csharp
namespace MyApp.Controls;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

public sealed class ScatterPlot : FrameworkElement
{
    private readonly DrawingVisual _plotVisual = new();
    private readonly List<Point> _dataPoints = [];

    public ScatterPlot()
    {
        AddVisualChild(_plotVisual);
    }

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _plotVisual;

    /// <summary>
    /// Set data and render
    /// </summary>
    public void SetData(IEnumerable<Point> points)
    {
        _dataPoints.Clear();
        _dataPoints.AddRange(points);
        Render();
    }

    private void Render()
    {
        var width = ActualWidth;
        var height = ActualHeight;

        if (width <= 0 || height <= 0 || _dataPoints.Count is 0)
        {
            return;
        }

        // Calculate data range
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        var minY = double.MaxValue;
        var maxY = double.MinValue;

        foreach (var p in _dataPoints)
        {
            minX = Math.Min(minX, p.X);
            maxX = Math.Max(maxX, p.X);
            minY = Math.Min(minY, p.Y);
            maxY = Math.Max(maxY, p.Y);
        }

        var rangeX = maxX - minX;
        var rangeY = maxY - minY;

        // Rendering
        using var dc = _plotVisual.RenderOpen();

        // Background
        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

        // Axes
        var axisPen = new Pen(Brushes.Black, 1);
        dc.DrawLine(axisPen, new Point(40, height - 30), new Point(width - 10, height - 30));
        dc.DrawLine(axisPen, new Point(40, 10), new Point(40, height - 30));

        // Data points
        var plotArea = new Rect(50, 20, width - 70, height - 60);
        var pointBrush = new SolidColorBrush(Color.FromArgb(180, 33, 150, 243));
        pointBrush.Freeze();

        foreach (var dataPoint in _dataPoints)
        {
            var x = plotArea.Left + (dataPoint.X - minX) / rangeX * plotArea.Width;
            var y = plotArea.Bottom - (dataPoint.Y - minY) / rangeY * plotArea.Height;

            dc.DrawEllipse(pointBrush, null, new Point(x, y), 3, 3);
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        Render();
    }
}
```

### 6.2 Usage Example

```csharp
// Generate 10,000 points
var random = new Random();
var points = Enumerable.Range(0, 10000)
    .Select(_ => new Point(
        random.NextDouble() * 100,
        random.NextDouble() * 100))
    .ToList();

scatterPlot.SetData(points);
```

---

## 7. RenderTargetBitmap (Off-screen Rendering)

### 7.1 Convert Visual to Image

```csharp
namespace MyApp.Graphics;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class VisualRenderer
{
    /// <summary>
    /// Render Visual to BitmapSource
    /// </summary>
    public static BitmapSource RenderToBitmap(
        Visual visual,
        int width,
        int height,
        double dpi = 96)
    {
        var renderTarget = new RenderTargetBitmap(
            width,
            height,
            dpi,
            dpi,
            PixelFormats.Pbgra32);

        renderTarget.Render(visual);
        renderTarget.Freeze();

        return renderTarget;
    }

    /// <summary>
    /// Save FrameworkElement as PNG
    /// </summary>
    public static void SaveAsPng(FrameworkElement element, string filePath)
    {
        var width = (int)element.ActualWidth;
        var height = (int)element.ActualHeight;

        if (width <= 0 || height <= 0)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));

            width = (int)element.ActualWidth;
            height = (int)element.ActualHeight;
        }

        var bitmap = RenderToBitmap(element, width, height);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = System.IO.File.Create(filePath);
        encoder.Save(stream);
    }
}
```

---

## 8. Performance Optimization Tips

```csharp
// 1. Reuse and Freeze Brush/Pen
var brush = new SolidColorBrush(Colors.Blue);
brush.Freeze();

var pen = new Pen(Brushes.Black, 1);
pen.Freeze();

// 2. Use StreamGeometry (immutable, optimized)
var geometry = new StreamGeometry();
using (var ctx = geometry.Open())
{
    ctx.BeginFigure(new Point(0, 0), true, true);
    ctx.LineTo(new Point(100, 0), true, false);
    ctx.LineTo(new Point(100, 100), true, false);
}
geometry.Freeze();

// 3. Batch rendering with DrawingGroup
var drawingGroup = new DrawingGroup();
using (var dc = drawingGroup.Open())
{
    // Draw multiple elements at once
}

// 4. Redraw only dirty region
dc.PushClip(new RectangleGeometry(dirtyRect));
```

---

## 9. References

- [DrawingVisual Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.drawingvisual)
- [Using DrawingVisual Objects - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/using-drawingvisual-objects)
- [Hit Testing in the Visual Layer - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/hit-testing-in-the-visual-layer)
