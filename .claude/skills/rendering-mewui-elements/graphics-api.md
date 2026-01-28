# IGraphicsContext API Reference

## Drawing Methods

| Method | Description |
|--------|-------------|
| `Clear(Color)` | Clear entire surface |
| `DrawLine(Point, Point, Color, double)` | Line with thickness |
| `DrawRectangle(Rect, Color, double)` | Rectangle outline |
| `FillRectangle(Rect, Color)` | Filled rectangle |
| `DrawRoundedRectangle(Rect, radiusX, radiusY, Color, double)` | Rounded outline (separate X/Y radii) |
| `FillRoundedRectangle(Rect, radiusX, radiusY, Color)` | Filled rounded rect (separate X/Y radii) |
| `DrawEllipse(Rect, Color, double)` | Ellipse outline |
| `FillEllipse(Rect, Color)` | Filled ellipse |
| `DrawText(text, Rect, IFont, Color, hAlign, vAlign, wrap)` | Render text (text first, two alignments) |
| `DrawText(text, Point, IFont, Color)` | Render text at point |
| `DrawImage(IImage, Rect destRect)` | Draw image to destination |
| `DrawImage(IImage, Point)` | Draw image at point |
| `DrawImage(IImage, Rect destRect, Rect sourceRect)` | Draw image portion (dest before source) |

## State Methods

| Method | Description |
|--------|-------------|
| `Save()` | Push current state |
| `Restore()` | Pop state |
| `SetClip(Rect)` | Set clipping region |
| `Translate(double dx, double dy)` | Offset coordinate system |

## Measurement

| Method | Description |
|--------|-------------|
| `MeasureText(ReadOnlySpan<char>, IFont)` | Get text size (accepts string) |
| `MeasureText(ReadOnlySpan<char>, IFont, double maxWidth)` | With wrapping |

Note: `ReadOnlySpan<char>` accepts `string` implicitly.

## Properties

| Property | Description |
|----------|-------------|
| `DpiScale` | Current DPI scale factor |
| `ImageScaleQuality` | Interpolation: Default, Fast, Normal, HighQuality |

## Complete Example

```csharp
public class GradientButton : Control
{
    public string Text { get; set; } = "";
    private bool _isPressed;

    protected override Size MeasureContent(Size availableSize)
    {
        var factory = GetGraphicsFactory();
        using var ctx = factory.CreateMeasurementContext(GetDpi());  // uint dpi
        var textSize = ctx.MeasureText(Text, GetFont());
        return new Size(textSize.Width + 24, textSize.Height + 12);
    }

    protected override void OnRender(IGraphicsContext context)
    {
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        var radius = 4.0;

        // Background
        var bgColor = _isPressed
            ? Theme.Palette.ButtonPressedBackground
            : IsMouseOver
                ? Theme.Palette.ButtonHoverBackground
                : Theme.Palette.ButtonFace;

        // Note: TWO radius parameters (radiusX, radiusY)
        context.FillRoundedRectangle(bounds, radius, radius, bgColor);
        context.DrawRoundedRectangle(bounds, radius, radius, Theme.Palette.BorderColor, 1);

        // Text - parameter order: text, bounds, font, color, hAlign, vAlign, wrap
        context.DrawText(
            Text,
            bounds,
            GetFont(),
            IsEnabled ? Foreground : Theme.Palette.DisabledText,
            TextAlignment.Center,   // horizontal
            TextAlignment.Center,   // vertical
            TextWrapping.NoWrap
        );
    }
}
```
