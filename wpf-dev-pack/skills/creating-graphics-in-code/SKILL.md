---
name: creating-graphics-in-code
description: "Creates WPF graphics dynamically in C# code using Shape, PathGeometry, and PathFigure classes. Use when building charts, diagrams, or procedurally generated graphics."
---

# Creating WPF Graphics in Code

Programmatically create shapes and geometry for dynamic graphics.

## 1. Shape Factory Pattern

```csharp
namespace MyApp.Graphics;

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

public static class ShapeFactory
{
    /// <summary>
    /// Create circular marker
    /// </summary>
    public static Ellipse CreateCircle(double size, Brush fill, Brush? stroke = null)
    {
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = fill
        };

        if (stroke is not null)
        {
            ellipse.Stroke = stroke;
            ellipse.StrokeThickness = 1;
        }

        return ellipse;
    }

    /// <summary>
    /// Create rectangle with optional corner radius
    /// </summary>
    public static Rectangle CreateRectangle(
        double width,
        double height,
        Brush fill,
        double cornerRadius = 0)
    {
        return new Rectangle
        {
            Width = width,
            Height = height,
            Fill = fill,
            RadiusX = cornerRadius,
            RadiusY = cornerRadius
        };
    }

    /// <summary>
    /// Create line between two points
    /// </summary>
    public static Line CreateLine(Point start, Point end, Brush stroke, double thickness = 1)
    {
        return new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            Stroke = stroke,
            StrokeThickness = thickness
        };
    }
}
```

---

## 2. Geometry Builder Pattern

```csharp
namespace MyApp.Graphics;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

public static class GeometryBuilder
{
    /// <summary>
    /// Create polygon from points
    /// </summary>
    public static PathGeometry CreatePolygon(IReadOnlyList<Point> points)
    {
        if (points.Count < 3)
            return new PathGeometry();

        var figure = new PathFigure
        {
            StartPoint = points[0],
            IsClosed = true,
            IsFilled = true
        };

        for (var i = 1; i < points.Count; i++)
        {
            figure.Segments.Add(new LineSegment(points[i], isStroked: true));
        }

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// Create polyline (open path)
    /// </summary>
    public static PathGeometry CreatePolyline(IReadOnlyList<Point> points)
    {
        if (points.Count < 2)
            return new PathGeometry();

        var figure = new PathFigure
        {
            StartPoint = points[0],
            IsClosed = false
        };

        for (var i = 1; i < points.Count; i++)
        {
            figure.Segments.Add(new LineSegment(points[i], isStroked: true));
        }

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// Create cubic Bezier curve
    /// </summary>
    public static PathGeometry CreateBezierCurve(
        Point start,
        Point control1,
        Point control2,
        Point end)
    {
        var figure = new PathFigure { StartPoint = start };
        figure.Segments.Add(new BezierSegment(control1, control2, end, isStroked: true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// Create arc
    /// </summary>
    public static PathGeometry CreateArc(
        Point start,
        Point end,
        Size size,
        SweepDirection direction = SweepDirection.Clockwise)
    {
        var figure = new PathFigure { StartPoint = start };
        figure.Segments.Add(new ArcSegment(end, size, 0, false, direction, isStroked: true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }
}
```

---

## 3. Arrow Creation

```csharp
/// <summary>
/// Create arrow with head
/// </summary>
public static Path CreateArrow(Point start, Point end, Brush stroke, double headSize = 10)
{
    var geometry = new PathGeometry();

    // Arrow body
    var bodyFigure = new PathFigure { StartPoint = start };
    bodyFigure.Segments.Add(new LineSegment(end, isStroked: true));
    geometry.Figures.Add(bodyFigure);

    // Calculate arrow head direction
    var direction = end - start;
    direction.Normalize();
    var perpendicular = new Vector(-direction.Y, direction.X);

    var headBase = end - direction * headSize;
    var headLeft = headBase + perpendicular * (headSize / 2);
    var headRight = headBase - perpendicular * (headSize / 2);

    // Arrow head
    var headFigure = new PathFigure { StartPoint = end, IsClosed = true, IsFilled = true };
    headFigure.Segments.Add(new LineSegment(headLeft, isStroked: true));
    headFigure.Segments.Add(new LineSegment(headRight, isStroked: true));
    geometry.Figures.Add(headFigure);

    return new Path
    {
        Data = geometry,
        Stroke = stroke,
        Fill = stroke,
        StrokeThickness = 2
    };
}
```

---

## 4. StreamGeometry (Performance)

```csharp
/// <summary>
/// Create optimized geometry using StreamGeometry
/// </summary>
public static StreamGeometry CreateOptimizedPolygon(IReadOnlyList<Point> points)
{
    var geometry = new StreamGeometry();

    using (var context = geometry.Open())
    {
        context.BeginFigure(points[0], isFilled: true, isClosed: true);

        for (var i = 1; i < points.Count; i++)
        {
            context.LineTo(points[i], isStroked: true, isSmoothJoin: false);
        }
    }

    geometry.Freeze(); // Make immutable for performance
    return geometry;
}

/// <summary>
/// Create optimized smooth curve
/// </summary>
public static StreamGeometry CreateOptimizedCurve(IReadOnlyList<Point> points)
{
    var geometry = new StreamGeometry();

    using (var context = geometry.Open())
    {
        context.BeginFigure(points[0], isFilled: false, isClosed: false);
        context.PolyBezierTo(points.Skip(1).ToList(), isStroked: true, isSmoothJoin: true);
    }

    geometry.Freeze();
    return geometry;
}
```

---

## 5. Adding to Canvas

```csharp
public void AddShapesToCanvas(Canvas canvas, IReadOnlyList<DataPoint> data)
{
    canvas.Children.Clear();

    foreach (var point in data)
    {
        var marker = ShapeFactory.CreateCircle(10, Brushes.Blue);
        Canvas.SetLeft(marker, point.X - 5);
        Canvas.SetTop(marker, point.Y - 5);
        canvas.Children.Add(marker);
    }

    // Add connecting line
    var linePoints = data.Select(d => new Point(d.X, d.Y)).ToList();
    var linePath = new Path
    {
        Data = GeometryBuilder.CreatePolyline(linePoints),
        Stroke = Brushes.Blue,
        StrokeThickness = 2
    };
    canvas.Children.Add(linePath);
}
```

---

## 6. Performance Guidelines

| Approach | Use Case | Performance |
|----------|----------|-------------|
| **Shape** | Interactive elements | Lower (full layout) |
| **PathGeometry** | Complex dynamic shapes | Medium |
| **StreamGeometry** | Static complex shapes | Higher |
| **DrawingVisual** | Large datasets | Highest |

**Always call `Freeze()`** on geometry, brushes, and pens for static graphics.

---

## 7. References

- [Geometry Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/geometry-overview)
- [StreamGeometry - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.streamgeometry)
