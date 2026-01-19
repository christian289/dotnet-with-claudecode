---
name: implementing-wpf-adorners
description: Implements WPF Adorner decoration layers with AdornerLayer, AdornerDecorator, and custom Adorner patterns. Use when building drag handles, validation indicators, watermarks, selection visuals, or resize grips.
---

# WPF Adorner Patterns

Adorner is a mechanism for overlaying decorative visual elements on top of UIElements.

## 1. Adorner Concept

### 1.1 Characteristics

- **AdornerLayer**: Separate rendering layer that holds Adorners
- **Z-Order**: Always renders above the adorned element
- **Layout Independent**: No effect on target element's layout
- **Event Support**: Can receive mouse/keyboard events

### 1.2 Usage Scenarios

| Scenario | Description |
|----------|-------------|
| **Validation Display** | Input field error display |
| **Drag Handles** | Element move/resize handles |
| **Watermark** | Hint text for empty TextBox |
| **Selection Display** | Highlight selected elements |
| **Tooltip/Badge** | Additional info display on elements |
| **Drag and Drop** | Preview during drag |

---

## 2. Basic Adorner Implementation

### 2.1 Simple Adorner

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

/// <summary>
/// Adorner that draws border around element
/// </summary>
public sealed class BorderAdorner : Adorner
{
    private readonly Pen _borderPen;

    public BorderAdorner(UIElement adornedElement) : base(adornedElement)
    {
        _borderPen = new Pen(Brushes.Red, 2)
        {
            DashStyle = DashStyles.Dash
        };
        _borderPen.Freeze();

        // Disable mouse events (decoration only)
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var rect = new Rect(AdornedElement.RenderSize);

        // Draw border
        drawingContext.DrawRectangle(null, _borderPen, rect);
    }
}
```

### 2.2 Applying Adorner

```csharp
// Get AdornerLayer
var adornerLayer = AdornerLayer.GetAdornerLayer(targetElement);

if (adornerLayer is not null)
{
    // Add Adorner
    var adorner = new BorderAdorner(targetElement);
    adornerLayer.Add(adorner);
}
```

### 2.3 Removing Adorner

```csharp
// Remove all Adorners from specific element
var adornerLayer = AdornerLayer.GetAdornerLayer(targetElement);
var adorners = adornerLayer?.GetAdorners(targetElement);

if (adorners is not null)
{
    foreach (var adorner in adorners)
    {
        adornerLayer!.Remove(adorner);
    }
}
```

---

## 3. AdornerDecorator

### 3.1 Default Location

```xml
<!-- Window default template includes AdornerDecorator -->
<Window>
    <!-- AdornerDecorator is automatically included -->
    <Grid>
        <TextBox x:Name="MyTextBox"/>
    </Grid>
</Window>
```

### 3.2 Explicit AdornerDecorator

```xml
<!-- Explicit AdornerDecorator in ControlTemplate -->
<ControlTemplate TargetType="{x:Type ContentControl}">
    <AdornerDecorator>
        <ContentPresenter/>
    </AdornerDecorator>
</ControlTemplate>

<!-- In Popup or special containers -->
<Popup>
    <AdornerDecorator>
        <Border>
            <StackPanel>
                <TextBox/>
                <Button Content="OK"/>
            </StackPanel>
        </Border>
    </AdornerDecorator>
</Popup>
```

---

## 4. Practical Adorner Examples

### 4.1 Watermark Adorner

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

/// <summary>
/// Display watermark (hint text) on TextBox
/// </summary>
public sealed class WatermarkAdorner : Adorner
{
    private readonly TextBlock _watermarkText;

    public WatermarkAdorner(UIElement adornedElement, string watermark)
        : base(adornedElement)
    {
        _watermarkText = new TextBlock
        {
            Text = watermark,
            Foreground = Brushes.Gray,
            FontStyle = FontStyles.Italic,
            Margin = new Thickness(4, 2, 0, 0),
            IsHitTestVisible = false
        };

        AddVisualChild(_watermarkText);

        IsHitTestVisible = false;
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index) => _watermarkText;

    protected override Size MeasureOverride(Size constraint)
    {
        _watermarkText.Measure(constraint);
        return _watermarkText.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _watermarkText.Arrange(new Rect(finalSize));
        return finalSize;
    }
}
```

### 4.2 Watermark Attached Property

```csharp
namespace MyApp.Behaviors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MyApp.Adorners;

public static class Watermark
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(Watermark),
            new PropertyMetadata(null, OnTextChanged));

    public static string GetText(DependencyObject obj) =>
        (string)obj.GetValue(TextProperty);

    public static void SetText(DependencyObject obj, string value) =>
        obj.SetValue(TextProperty, value);

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.Loaded -= OnTextBoxLoaded;
        textBox.Loaded += OnTextBoxLoaded;
        textBox.TextChanged -= OnTextBoxTextChanged;
        textBox.TextChanged += OnTextBoxTextChanged;
    }

    private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateWatermark(textBox);
        }
    }

    private static void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateWatermark(textBox);
        }
    }

    private static void UpdateWatermark(TextBox textBox)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
        if (adornerLayer is null)
        {
            return;
        }

        // Remove existing watermark
        RemoveWatermark(textBox, adornerLayer);

        // Add watermark if text is empty
        if (string.IsNullOrEmpty(textBox.Text))
        {
            var watermark = GetText(textBox);
            if (!string.IsNullOrEmpty(watermark))
            {
                adornerLayer.Add(new WatermarkAdorner(textBox, watermark));
            }
        }
    }

    private static void RemoveWatermark(TextBox textBox, AdornerLayer adornerLayer)
    {
        var adorners = adornerLayer.GetAdorners(textBox);
        if (adorners is null)
        {
            return;
        }

        foreach (var adorner in adorners)
        {
            if (adorner is WatermarkAdorner)
            {
                adornerLayer.Remove(adorner);
            }
        }
    }
}
```

### 4.3 Using Watermark in XAML

```xml
<TextBox local:Watermark.Text="Enter email address"/>
```

---

## 5. Resize Handle Adorner

### 5.1 Implementation

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

/// <summary>
/// Handle Adorner for element resizing
/// </summary>
public sealed class ResizeAdorner : Adorner
{
    private readonly VisualCollection _visuals;
    private readonly Thumb _topLeft, _topRight, _bottomLeft, _bottomRight;
    private readonly Thumb _top, _bottom, _left, _right;

    private const double ThumbSize = 8;

    public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
    {
        _visuals = new VisualCollection(this);

        // Corner handles
        _topLeft = CreateThumb(Cursors.SizeNWSE);
        _topRight = CreateThumb(Cursors.SizeNESW);
        _bottomLeft = CreateThumb(Cursors.SizeNESW);
        _bottomRight = CreateThumb(Cursors.SizeNWSE);

        // Edge handles
        _top = CreateThumb(Cursors.SizeNS);
        _bottom = CreateThumb(Cursors.SizeNS);
        _left = CreateThumb(Cursors.SizeWE);
        _right = CreateThumb(Cursors.SizeWE);

        // Connect drag events
        _bottomRight.DragDelta += OnBottomRightDrag;
        _topLeft.DragDelta += OnTopLeftDrag;
        _topRight.DragDelta += OnTopRightDrag;
        _bottomLeft.DragDelta += OnBottomLeftDrag;
        _top.DragDelta += OnTopDrag;
        _bottom.DragDelta += OnBottomDrag;
        _left.DragDelta += OnLeftDrag;
        _right.DragDelta += OnRightDrag;
    }

    private Thumb CreateThumb(Cursor cursor)
    {
        var thumb = new Thumb
        {
            Width = ThumbSize,
            Height = ThumbSize,
            Cursor = cursor,
            Background = Brushes.White,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1)
        };
        _visuals.Add(thumb);
        return thumb;
    }

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Visual GetVisualChild(int index) => _visuals[index];

    protected override Size ArrangeOverride(Size finalSize)
    {
        var halfThumb = ThumbSize / 2;
        var width = AdornedElement.RenderSize.Width;
        var height = AdornedElement.RenderSize.Height;

        // Arrange corner handles
        _topLeft.Arrange(new Rect(-halfThumb, -halfThumb, ThumbSize, ThumbSize));
        _topRight.Arrange(new Rect(width - halfThumb, -halfThumb, ThumbSize, ThumbSize));
        _bottomLeft.Arrange(new Rect(-halfThumb, height - halfThumb, ThumbSize, ThumbSize));
        _bottomRight.Arrange(new Rect(width - halfThumb, height - halfThumb, ThumbSize, ThumbSize));

        // Arrange edge handles
        _top.Arrange(new Rect(width / 2 - halfThumb, -halfThumb, ThumbSize, ThumbSize));
        _bottom.Arrange(new Rect(width / 2 - halfThumb, height - halfThumb, ThumbSize, ThumbSize));
        _left.Arrange(new Rect(-halfThumb, height / 2 - halfThumb, ThumbSize, ThumbSize));
        _right.Arrange(new Rect(width - halfThumb, height / 2 - halfThumb, ThumbSize, ThumbSize));

        return finalSize;
    }

    private void OnBottomRightDrag(object sender, DragDeltaEventArgs e)
    {
        if (AdornedElement is FrameworkElement element)
        {
            element.Width = Math.Max(element.MinWidth, element.Width + e.HorizontalChange);
            element.Height = Math.Max(element.MinHeight, element.Height + e.VerticalChange);
        }
    }

    // Additional drag handlers for other directions...
    private void OnTopLeftDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
    private void OnTopRightDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
    private void OnBottomLeftDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
    private void OnTopDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
    private void OnBottomDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
    private void OnLeftDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
    private void OnRightDrag(object sender, DragDeltaEventArgs e) { /* ... */ }
}
```

---

## 6. Validation Error Adorner

### 6.1 ValidationErrorAdorner

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

/// <summary>
/// Validation error display Adorner
/// </summary>
public sealed class ValidationErrorAdorner : Adorner
{
    private readonly string _errorMessage;
    private readonly Pen _borderPen;
    private readonly Brush _errorBrush;

    public ValidationErrorAdorner(UIElement adornedElement, string errorMessage)
        : base(adornedElement)
    {
        _errorMessage = errorMessage;

        _borderPen = new Pen(Brushes.Red, 2);
        _borderPen.Freeze();

        _errorBrush = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
        _errorBrush.Freeze();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var rect = new Rect(AdornedElement.RenderSize);

        // Error background
        drawingContext.DrawRectangle(_errorBrush, _borderPen, rect);

        // Error icon (top right)
        var iconSize = 16.0;
        var iconRect = new Rect(rect.Right - iconSize - 2, rect.Top + 2, iconSize, iconSize);

        drawingContext.DrawEllipse(Brushes.Red, null,
            new Point(iconRect.Left + iconSize / 2, iconRect.Top + iconSize / 2),
            iconSize / 2, iconSize / 2);

        // Exclamation mark
        var formattedText = new FormattedText(
            "!",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            12,
            Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        drawingContext.DrawText(formattedText,
            new Point(iconRect.Left + iconSize / 2 - formattedText.Width / 2,
                      iconRect.Top + iconSize / 2 - formattedText.Height / 2));
    }
}
```

---

## 7. Drag Preview Adorner

### 7.1 DragPreviewAdorner

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

/// <summary>
/// Display preview during drag
/// </summary>
public sealed class DragPreviewAdorner : Adorner
{
    private readonly VisualBrush _visualBrush;
    private Point _offset;

    public DragPreviewAdorner(UIElement adornedElement, Point offset)
        : base(adornedElement)
    {
        _offset = offset;

        _visualBrush = new VisualBrush(adornedElement)
        {
            Opacity = 0.7
        };

        IsHitTestVisible = false;
    }

    public void UpdatePosition(Point position)
    {
        _offset = position;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var size = AdornedElement.RenderSize;
        var rect = new Rect(
            _offset.X - size.Width / 2,
            _offset.Y - size.Height / 2,
            size.Width,
            size.Height);

        drawingContext.DrawRectangle(_visualBrush, null, rect);
    }
}
```

---

## 8. Adorner Management Service

```csharp
namespace MyApp.Services;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

/// <summary>
/// Adorner lifecycle management
/// </summary>
public sealed class AdornerService
{
    private readonly Dictionary<UIElement, List<Adorner>> _adornerMap = [];

    /// <summary>
    /// Add Adorner
    /// </summary>
    public bool AddAdorner(UIElement element, Adorner adorner)
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer is null)
        {
            return false;
        }

        layer.Add(adorner);

        if (!_adornerMap.TryGetValue(element, out var list))
        {
            list = [];
            _adornerMap[element] = list;
        }
        list.Add(adorner);

        return true;
    }

    /// <summary>
    /// Remove specific type Adorner
    /// </summary>
    public void RemoveAdorner<T>(UIElement element) where T : Adorner
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer is null || !_adornerMap.TryGetValue(element, out var list))
        {
            return;
        }

        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] is T adorner)
            {
                layer.Remove(adorner);
                list.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Remove all Adorners
    /// </summary>
    public void ClearAdorners(UIElement element)
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer is null || !_adornerMap.TryGetValue(element, out var list))
        {
            return;
        }

        foreach (var adorner in list)
        {
            layer.Remove(adorner);
        }
        list.Clear();
    }

    /// <summary>
    /// Check if specific type Adorner exists
    /// </summary>
    public bool HasAdorner<T>(UIElement element) where T : Adorner
    {
        if (!_adornerMap.TryGetValue(element, out var list))
        {
            return false;
        }

        foreach (var adorner in list)
        {
            if (adorner is T)
            {
                return true;
            }
        }
        return false;
    }
}
```

---

## 9. Checklist

- [ ] Verify AdornerLayer exists before adding Adorner
- [ ] Set `IsHitTestVisible = false` for decoration-only Adorners
- [ ] Correctly implement VisualChildrenCount and GetVisualChild
- [ ] Arrange children using MeasureOverride and ArrangeOverride
- [ ] Remove unnecessary Adorners (prevent memory leaks)
- [ ] Explicitly add AdornerDecorator in Popup, etc.

---

## 10. References

- [Adorners Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/adorners-overview)
- [Adorner Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.documents.adorner)
