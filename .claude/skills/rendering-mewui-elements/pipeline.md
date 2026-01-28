# MewUI Rendering Pipeline

## Three-Pass Layout

```
Window.PerformLayout()
├── Content.Measure(availableSize)    // Pass 1: Calculate DesiredSize
├── Content.Arrange(finalRect)         // Pass 2: Set Bounds
└── Window.RenderFrame()
    └── Content.Render(context)        // Pass 3: Draw visuals
```

## Class Hierarchy

```
Element (base) - MeasureCore/ArrangeCore abstract
└── UIElement - MeasureOverride/ArrangeOverride virtual, Render, OnRender
    └── FrameworkElement - MeasureContent/ArrangeContent virtual
```

## Measure Pass

Called top-down with size constraints. Each element calculates its `DesiredSize`.

```csharp
// In Element.cs
public void Measure(Size availableSize)
{
    if (!IsMeasureDirty && _hasMeasureConstraint && _lastMeasureConstraint == availableSize)
        return; // Skip if clean

    var measured = MeasureCore(availableSize);
    DesiredSize = ApplyLayoutRounding(measured);  // DPI snap
    IsMeasureDirty = false;
}

// Override in your control (FrameworkElement-derived):
protected override Size MeasureContent(Size availableSize)
{
    // Measure children or calculate based on content
    return new Size(desiredWidth, desiredHeight);
}
```

## Arrange Pass

Called top-down with final bounds. Each element sets its `Bounds`.

```csharp
// In Element.cs
public void Arrange(Rect finalRect)
{
    var arrangedRect = ApplyLayoutRounding(GetArrangedBounds(finalRect));

    if (!IsArrangeDirty && Bounds == arrangedRect)
        return;

    Bounds = arrangedRect;
    ArrangeCore(Bounds);
    IsArrangeDirty = false;
}

// Override in panels (returns void, not Size):
protected override void ArrangeContent(Rect bounds)
{
    foreach (var child in Children)
    {
        var childBounds = CalculateChildBounds(child);
        child.Arrange(childBounds);
    }
    // No return - method is void
}
```

## Render Pass

Called per-frame. Each element draws via `IGraphicsContext`.

```csharp
// In UIElement.cs - simpler than you might expect
public override void Render(IGraphicsContext context)
{
    if (!IsVisible)
        return;

    OnRender(context);  // No automatic Save/Translate/Clip
}

// Override to draw:
protected override void OnRender(IGraphicsContext context)
{
    // Drawing code here - coordinates relative to Bounds
}
```

Note: There is no automatic coordinate translation or clipping in the base implementation. Individual controls handle coordinate systems as needed.

## Layout Rounding

Snaps to physical pixels for crisp rendering:

```csharp
// DPI-aware pixel snapping
double rounded = LayoutRounding.RoundToPixel(value, DpiScale);
Rect snapped = LayoutRounding.SnapRectEdgesToPixels(rect, DpiScale);

// Additional methods available:
// RoundSizeToPixels, RoundRectToPixels, SnapBoundsRectToPixels,
// SnapViewportRectToPixels, SnapConstraintRectToPixels
```

## Key Files

- `src/MewUI/Elements/Element.cs` - Measure/Arrange base
- `src/MewUI/Elements/UIElement.cs` - MeasureOverride/ArrangeOverride, Render
- `src/MewUI/Elements/FrameworkElement.cs` - MeasureContent/ArrangeContent, layout properties
- `src/MewUI/Controls/Window.cs` - PerformLayout, RenderFrame
- `src/MewUI/Core/LayoutRounding.cs` - DPI rounding
