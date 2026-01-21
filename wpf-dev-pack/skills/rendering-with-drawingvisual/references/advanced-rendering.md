# Advanced DrawingVisual Rendering

## 1. Large-scale Rendering Example (Scatter Plot)

### 1.1 ScatterPlot Control

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

### 1.2 Usage Example

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

## 2. RenderTargetBitmap (Off-screen Rendering)

### 2.1 Convert Visual to Image

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

## 3. Performance Optimization Tips

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
