---
name: rendering-mewui-elements
description: "Renders custom graphics in MewUI controls using IGraphicsContext. Use when implementing OnRender, drawing shapes, text, images, or understanding the Measure/Arrange/Render pipeline."
---

## Rendering Pipeline

MewUI uses a three-pass layout system:

```
Measure (calculate DesiredSize) → Arrange (set Bounds) → Render (draw via IGraphicsContext)
```

**Key overrides in custom controls:**
- `MeasureContent(Size availableSize)` → return desired size
- `ArrangeContent(Rect bounds)` → position children
- `OnRender(IGraphicsContext context)` → draw visuals

---

## IGraphicsContext Essentials

```csharp
protected override void OnRender(IGraphicsContext context)
{
    var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

    // State management (always Save/Restore)
    context.Save();
    context.Translate(10, 10);
    context.SetClip(clipRect);
    // ... drawing ...
    context.Restore();

    // Drawing primitives
    context.FillRectangle(bounds, Colors.Blue);
    context.DrawRectangle(bounds, Colors.Black, thickness: 1);
    context.FillRoundedRectangle(bounds, radiusX: 4, radiusY: 4, Colors.White);
    context.DrawLine(start, end, Colors.Gray, thickness: 1);
    context.FillEllipse(bounds, Colors.Red);

    // Text - note parameter order: text, bounds, font, color, alignments, wrapping
    context.DrawText("Hello", bounds, font, Colors.Black,
        TextAlignment.Center, TextAlignment.Center, TextWrapping.NoWrap);

    // Images
    context.DrawImage(image, destRect);
}
```

---

## Text Measurement

```csharp
protected override Size MeasureContent(Size availableSize)
{
    var factory = GetGraphicsFactory();
    using var ctx = factory.CreateMeasurementContext(GetDpi());  // uint dpi (96, 144, etc.)
    return ctx.MeasureText(Text, GetFont(), availableSize.Width);
}
```

---

## Graphics Backends

```csharp
Application.DefaultGraphicsBackend = GraphicsBackend.Direct2D;  // GPU (Windows)
Application.DefaultGraphicsBackend = GraphicsBackend.Gdi;       // Software (Windows)
Application.DefaultGraphicsBackend = GraphicsBackend.OpenGL;    // Cross-platform
```

---

## Invalidation

```csharp
InvalidateMeasure();  // Re-measure + re-arrange + re-render
InvalidateArrange();  // Re-arrange + re-render
InvalidateVisual();   // Re-render only
```

---

**Detailed API**: See [graphics-api.md](graphics-api.md)
**Pipeline details**: See [pipeline.md](pipeline.md)
