# Advanced Visual/Logical Tree Patterns

## 1. Template Access Patterns

### 1.1 Access in OnApplyTemplate

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;

public sealed class CustomControl : Control
{
    private Border? _border;
    private ContentPresenter? _contentPresenter;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // GetTemplateChild: find element in current control template
        _border = GetTemplateChild("PART_Border") as Border;
        _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

        if (_border is not null)
        {
            _border.MouseEnter += OnBorderMouseEnter;
        }
    }

    private void OnBorderMouseEnter(object sender, MouseEventArgs e)
    {
        // Interact with template element
    }
}
```

### 1.2 External Access to Template Internals

```csharp
// Visual Tree is complete after Loaded
button.Loaded += (s, e) =>
{
    // Navigate template internals with VisualTreeHelper
    var border = VisualTreeSearcher.FindVisualChild<Border>(button);
};
```

---

## 2. Performance Considerations

### 2.1 Optimized Tree Search

```csharp
namespace MyApp.Helpers;

using System.Windows;
using System.Windows.Media;

public static class OptimizedTreeSearcher
{
    /// <summary>
    /// Depth-limited search (performance optimization)
    /// </summary>
    public static T? FindVisualChild<T>(
        DependencyObject parent,
        int maxDepth) where T : DependencyObject
    {
        if (maxDepth <= 0)
        {
            return null;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child, maxDepth - 1);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Use cached search results
    /// </summary>
    private static readonly ConditionalWeakTable<DependencyObject, Dictionary<Type, DependencyObject?>> _cache = new();

    public static T? FindVisualChildCached<T>(DependencyObject parent) where T : DependencyObject
    {
        var cache = _cache.GetOrCreateValue(parent);

        if (cache.TryGetValue(typeof(T), out var cached))
        {
            return cached as T;
        }

        var result = VisualTreeSearcher.FindVisualChild<T>(parent);
        cache[typeof(T)] = result;

        return result;
    }
}
```
