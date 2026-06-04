# WPF DrawingContext High-Performance Rendering — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

## Complex Shapes (Using StreamGeometry)

Use StreamGeometry for complex shapes like triangles and polygons.

### Triangle Rendering Example

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Media;

public sealed class TriangleCanvas : FrameworkElement
{
    private readonly record struct TriangleData(
        Point Point1, Point Point2, Point Point3, Brush Fill);

    private readonly List<TriangleData> _triangles = [];
    private readonly Pen _pen = new(Brushes.Black, 1);

    public TriangleCanvas()
    {
        _pen.Freeze();
    }

    public void AddTriangle(Point p1, Point p2, Point p3, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        _triangles.Add(new TriangleData(p1, p2, p3, brush));
    }

    public void Render()
    {
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        foreach (var triangle in _triangles)
        {
            // Create lightweight geometry using StreamGeometry
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(triangle.Point1, isFilled: true, isClosed: true);
                ctx.LineTo(triangle.Point2, isStroked: true, isSmoothJoin: false);
                ctx.LineTo(triangle.Point3, isStroked: true, isSmoothJoin: false);
            }

            geometry.Freeze();  // Optimize by making immutable

            dc.DrawGeometry(triangle.Fill, _pen, geometry);
        }
    }

    public void Clear()
    {
        _triangles.Clear();
        InvalidateVisual();
    }
}
```

---

## Pattern with Performance Measurement

### Async Rendering + Performance Measurement

```csharp
namespace MyApp.Controls;

using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

public sealed class BenchmarkCanvas : FrameworkElement
{
    private readonly record struct RectData(Rect Bounds, Brush Fill);

    private readonly List<RectData> _items = [];
    private readonly Pen _pen = new(Brushes.Black, 1);

    public BenchmarkCanvas()
    {
        _pen.Freeze();
    }

    /// <summary>
    /// Renders a large number of shapes and returns the elapsed time.
    /// </summary>
    public async Task<TimeSpan> DrawItemsAsync(int count)
    {
        _items.Clear();

        double width = ActualWidth > 0 ? ActualWidth : 400;
        double height = ActualHeight > 0 ? ActualHeight : 400;

        var random = new Random();

        // Step 1: Generate data only (before measurement)
        for (int i = 0; i < count; i++)
        {
            double x = random.NextDouble() * (width - 20);
            double y = random.NextDouble() * (height - 20);
            double size = 10 + random.NextDouble() * 20;

            var brush = new SolidColorBrush(Color.FromRgb(
                (byte)random.Next(256),
                (byte)random.Next(256),
                (byte)random.Next(256)));
            brush.Freeze();

            _items.Add(new RectData(new Rect(x, y, size, size), brush));

            // Yield periodically to prevent UI hang
            if (i % 100 == 0)
            {
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            }
        }

        // Step 2: Measure rendering only (call once)
        var stopwatch = Stopwatch.StartNew();
        InvalidateVisual();
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        stopwatch.Stop();

        return stopwatch.Elapsed;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        foreach (var item in _items)
        {
            dc.DrawRectangle(item.Fill, _pen, item.Bounds);
        }
    }

    public void Clear()
    {
        _items.Clear();
        InvalidateVisual();
    }
}
```

---

## Integration with MVVM Pattern

### ViewModel - Delegate Pattern

Pattern allowing ViewModel to call rendering methods without directly referencing View type:

```csharp
namespace MyApp.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class RenderViewModel : ObservableObject
{
    // Store only delegates without View type reference
    private Func<int, Task<TimeSpan>>? _drawItems;
    private Action? _clearCanvas;

    [ObservableProperty] private bool _isRendering;

    [ObservableProperty] private string _elapsedTime = "Waiting...";

    // Inject required methods from View
    public void SetRenderActions(
        Func<int, Task<TimeSpan>> drawItems,
        Action clearCanvas)
    {
        _drawItems = drawItems;
        _clearCanvas = clearCanvas;
    }

    [RelayCommand]
    private async Task RenderAsync()
    {
        if (_drawItems is null)
        {
            return;
        }

        IsRendering = true;
        _clearCanvas?.Invoke();

        var elapsed = await _drawItems(10000);
        ElapsedTime = $"{elapsed.TotalMilliseconds:F2} ms";

        IsRendering = false;
    }
}
```

### View - Delegate Connection

```csharp
namespace MyApp.Views;

using System.Windows;
using MyApp.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (DataContext is RenderViewModel vm)
            {
                vm.SetRenderActions(
                    MyCanvas.DrawItemsAsync,
                    MyCanvas.Clear);
            }
        };
    }
}
```

---

## Comparison with Shape Approach (Reference)

There are cases where Shape approach is needed:

```csharp
// Shape approach - suitable for few shapes requiring interaction
public sealed class ShapeBasedPanel : Canvas
{
    public void AddInteractiveShape()
    {
        var polygon = new Polygon
        {
            Points = [new Point(0, 0), new Point(50, 0), new Point(25, 50)],
            Fill = Brushes.Blue,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };

        // Can attach events to individual shapes
        polygon.MouseEnter += (s, e) => polygon.Fill = Brushes.Red;
        polygon.MouseLeave += (s, e) => polygon.Fill = Brushes.Blue;

        Children.Add(polygon);
    }
}
```

**When to Choose Shape Approach**:
- Number of shapes is tens to hundreds or less
- Mouse events needed on individual shapes
- Drag and drop functionality required

---

## Performance Comparison Example

**Based on 10,000 triangles**:

| Method | Expected Time | Notes |
|--------|---------------|-------|
| Shape (Polygon) | 500-2000ms | Visual Tree overhead |
| DrawingContext | 20-50ms | Direct drawing |
| **Performance Ratio** | **10-50x** | Varies by environment |
