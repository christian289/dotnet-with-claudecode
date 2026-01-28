# ContentControl Pattern

## Single-Child Container

```csharp
public class Card : ContentControl
{
    public string Title { get; set; } = "";
    public double HeaderHeight { get; set; } = 40;

    protected override Size MeasureContent(Size availableSize)
    {
        var contentSize = Size.Empty;
        if (Content != null)
        {
            var contentAvailable = availableSize.Deflate(Padding);
            contentAvailable = contentAvailable.WithHeight(contentAvailable.Height - HeaderHeight);
            Content.Measure(contentAvailable);
            contentSize = Content.DesiredSize;
        }
        return new Size(
            contentSize.Width + Padding.Horizontal,
            contentSize.Height + Padding.Vertical + HeaderHeight
        );
    }

    // Note: ArrangeContent returns VOID, not Size
    protected override void ArrangeContent(Rect bounds)
    {
        if (Content != null)
        {
            var contentBounds = bounds.Deflate(Padding);
            contentBounds = new Rect(
                contentBounds.X,
                contentBounds.Y + HeaderHeight,
                contentBounds.Width,
                contentBounds.Height - HeaderHeight
            );
            Content.Arrange(contentBounds);
        }
        // No return statement - method is void
    }

    // Note: Override Render (not just OnRender) to also render Content
    public override void Render(IGraphicsContext context)
    {
        base.Render(context);
        Content?.Render(context);  // Must explicitly render child
    }

    protected override void OnRender(IGraphicsContext context)
    {
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        var radius = 4.0;

        // Card background (note: two radius parameters)
        context.FillRoundedRectangle(bounds, radius, radius, Colors.White);

        // Header
        var headerBounds = new Rect(0, 0, Bounds.Width, HeaderHeight);
        context.FillRoundedRectangle(headerBounds, radius, radius, Theme.Palette.Accent);

        // Title (note: text first, two alignment params)
        context.DrawText(Title, headerBounds.Deflate(12, 0), GetFont(), Colors.White,
            TextAlignment.Left, TextAlignment.Center, TextWrapping.NoWrap);

        // Border
        context.DrawRoundedRectangle(bounds, radius, radius, Theme.Palette.BorderColor, 1);
    }
}
```

## Content Property Management

The actual ContentControl.Content property manages parent relationships:

```csharp
public Element? Content
{
    get;
    set
    {
        if (field != value)
        {
            if (field != null)
                field.Parent = null;  // Clear old parent

            field = value;

            if (field != null)
                field.Parent = this;  // Set new parent

            InvalidateMeasure();  // Trigger re-layout
        }
    }
}
```

## Hit Testing

ContentControl overrides hit testing to check children first:

```csharp
protected override UIElement? OnHitTest(Point point)
{
    if (!IsVisible || !IsHitTestVisible)
        return null;

    // First check children
    if (Content is UIElement uiContent)
    {
        var result = uiContent.HitTest(point);
        if (result != null)
            return result;
    }

    // Then check self
    if (Bounds.Contains(point))
        return this;

    return null;
}
```

## IVisualTreeHost (Internal)

ContentControl implements `IVisualTreeHost` for tree traversal:

```csharp
void IVisualTreeHost.VisitChildren(Action<Element> visitor)
{
    if (Content != null)
        visitor(Content);
}
```

Note: `IVisualTreeHost` is an internal interface.

## Usage

```csharp
new Card()
    .Title("Settings")
    .HeaderHeight(48)
    .Content(
        new StackPanel()
            .Spacing(8)
            .Children(
                new CheckBox().Text("Enable notifications"),  // Note: .Text() not .Content()
                new Slider().Minimum(0).Maximum(100)
            )
    )
```
