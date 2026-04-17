# Advanced Drag and Drop Patterns

## Visual Feedback

### Drag Adorner

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

public sealed class DragAdorner : Adorner
{
    private readonly UIElement _draggedElement;
    private Point _offset;

    public DragAdorner(UIElement adornedElement, UIElement draggedElement, Point offset)
        : base(adornedElement)
    {
        _draggedElement = draggedElement;
        _offset = offset;

        IsHitTestVisible = false;
    }

    public void UpdatePosition(Point position)
    {
        _offset = position;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var brush = new VisualBrush(_draggedElement)
        {
            Opacity = 0.7
        };

        var size = new Size(_draggedElement.RenderSize.Width, _draggedElement.RenderSize.Height);
        var rect = new Rect(_offset, size);

        drawingContext.DrawRectangle(brush, null, rect);
    }
}
```

### Drop Indicator Adorner

```csharp
namespace MyApp.Adorners;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

public sealed class DropIndicatorAdorner : Adorner
{
    private static readonly Brush IndicatorBrush;
    private static readonly Pen IndicatorPen;

    static DropIndicatorAdorner()
    {
        IndicatorBrush = new SolidColorBrush(Color.FromArgb(64, 0, 120, 215));
        IndicatorBrush.Freeze();
        IndicatorPen = new Pen(new SolidColorBrush(Color.FromRgb(0, 120, 215)), 2);
        IndicatorPen.Freeze();
    }

    public DropIndicatorAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var rect = new Rect(AdornedElement.RenderSize);
        drawingContext.DrawRectangle(IndicatorBrush, IndicatorPen, rect);
    }
}
```

---

## ListBox Reordering

### Drag and Drop within ListBox

```csharp
namespace MyApp.Controls;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public sealed class ReorderableListBox : ListBox
{
    private Point _startPoint;
    private int _draggedIndex = -1;

    public ReorderableListBox()
    {
        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        PreviewMouseMove += OnPreviewMouseMove;
        Drop += OnDrop;
        AllowDrop = true;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        var diff = _startPoint - e.GetPosition(null);

        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
            return;

        // Find dragged item
        var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (listBoxItem == null)
            return;

        _draggedIndex = Items.IndexOf(listBoxItem.Content);
        if (_draggedIndex < 0)
            return;

        var data = new DataObject("ReorderItem", listBoxItem.Content);
        DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("ReorderItem"))
            return;

        var droppedData = e.Data.GetData("ReorderItem");
        var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

        if (targetItem == null || ItemsSource is not ObservableCollection<object> collection)
            return;

        var targetIndex = Items.IndexOf(targetItem.Content);

        if (_draggedIndex >= 0 && targetIndex >= 0 && _draggedIndex != targetIndex)
        {
            collection.Move(_draggedIndex, targetIndex);
        }

        _draggedIndex = -1;
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T ancestor)
                return ancestor;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
```

---

## Custom Data Formats

### Registering Custom Format

```csharp
namespace MyApp.DragDrop;

using System.Windows;

public static class CustomDataFormats
{
    // Register custom data format
    public static readonly string TreeNodeFormat = DataFormats.GetDataFormat("MyApp.TreeNode").Name;
    public static readonly string GridRowFormat = DataFormats.GetDataFormat("MyApp.GridRow").Name;
}

// Usage
var data = new DataObject();
data.SetData(CustomDataFormats.TreeNodeFormat, treeNode);

// Check format
if (e.Data.GetDataPresent(CustomDataFormats.TreeNodeFormat))
{
    var node = e.Data.GetData(CustomDataFormats.TreeNodeFormat) as TreeNode;
}
```

---

## Inter-Application Drag and Drop

### Receiving from External Apps

```csharp
private void Window_Drop(object sender, DragEventArgs e)
{
    // Files from Explorer
    if (e.Data.GetDataPresent(DataFormats.FileDrop))
    {
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
    }

    // Text from other apps
    if (e.Data.GetDataPresent(DataFormats.UnicodeText))
    {
        var text = (string)e.Data.GetData(DataFormats.UnicodeText);
    }

    // HTML content
    if (e.Data.GetDataPresent(DataFormats.Html))
    {
        var html = (string)e.Data.GetData(DataFormats.Html);
    }

    // Bitmap image
    if (e.Data.GetDataPresent(DataFormats.Bitmap))
    {
        var image = e.Data.GetData(DataFormats.Bitmap) as System.Drawing.Bitmap;
    }
}
```
