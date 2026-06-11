# WPF UI Virtualization

> Implements WPF UI virtualization for large data sets using VirtualizingStackPanel. Use when displaying 1000+ items in ItemsControl, ListView, or DataGrid to prevent memory and performance issues.

## Quick Setup
```xml
<ListBox ItemsSource="{Binding LargeCollection}"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling"
         VirtualizingPanel.ScrollUnit="Pixel"
         VirtualizingPanel.CacheLength="2,2"
         VirtualizingPanel.CacheLengthUnit="Page"/>
```

## Key Properties

| Property | Recommended | Purpose |
|----------|-------------|---------|
| `IsVirtualizing` | True | Enable virtualization |
| `VirtualizationMode` | Recycling | Reuse containers |
| `ScrollUnit` | Pixel | Smooth scrolling |
| `CacheLength` | "1,1" to "2,2" | Buffer pages |

## Virtualization Breakers

**These disable virtualization:**
```xml
<!-- ❌ ScrollViewer wrapper -->
<ScrollViewer>
    <ListBox/>
</ScrollViewer>

<!-- ❌ CanContentScroll disabled -->
<ListBox ScrollViewer.CanContentScroll="False"/>

<!-- ❌ Grouping without flag -->
<ListBox>
    <ListBox.GroupStyle>...</ListBox.GroupStyle>
</ListBox>
```

**Fixes:**
```xml
<!-- ✅ No wrapper needed - ListBox has built-in ScrollViewer -->
<ListBox ItemsSource="{Binding Items}"/>

<!-- ✅ Grouping with virtualization -->
<ListBox VirtualizingPanel.IsVirtualizingWhenGrouping="True">
    <ListBox.GroupStyle>...</ListBox.GroupStyle>
</ListBox>
```

## Recycling Mode Considerations
```csharp
// When using Recycling mode, clean up in PrepareContainerForItemOverride
protected override void PrepareContainerForItemOverride(
    DependencyObject element, object item)
{
    base.PrepareContainerForItemOverride(element, item);

    var container = (ListBoxItem)element;
    // Reset any manually attached handlers or state
}
```

## Performance Tips

### Deferred Scrolling
```xml
<!-- Faster scrollbar dragging -->
<ListBox ScrollViewer.IsDeferredScrollingEnabled="True"/>
```

### Diagnostic Check
```csharp
public static bool IsVirtualizing(ItemsControl control)
{
    var panel = FindVisualChild<VirtualizingStackPanel>(control);
    return panel != null && VirtualizingPanel.GetIsVirtualizing(control);
}

public static int GetRealizedCount(ItemsControl control)
{
    var generator = control.ItemContainerGenerator;
    return Enumerable.Range(0, control.Items.Count)
        .Count(i => generator.ContainerFromIndex(i) != null);
}
```

## DataGrid Virtualization
```xml
<DataGrid ItemsSource="{Binding Items}"
          EnableRowVirtualization="True"
          EnableColumnVirtualization="True"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"/>
```

## Variable-Height, Selectable Chat Bubbles

A long conversation (hundreds of turns) where each turn is a selectable
`RichTextBox` bubble is a hard case for virtualization. A plain `ItemsControl`
does not virtualize at all, so memory and layout cost grow with the history.
Turning virtualization on then interacts badly with two properties of chat
bubbles: their **variable height**, and their **per-bubble transient state**
(text selection, internal scroll). Under `VirtualizationMode="Recycling"` the
host reuses a container for a different turn, so any visual state left on the
container reappears against the wrong message; and a `RichTextBox` is a
comparatively heavy element to realize per item.

Guidance:

- **Use a virtualizing host for long histories.** Enable
  `VirtualizingPanel.IsVirtualizing="True"`,
  `VirtualizingPanel.VirtualizationMode="Recycling"`, and
  `ScrollViewer.CanContentScroll="True"`.
- **Keep all per-turn state in the turn ViewModel, never in the visual.**
  Containers are recycled and re-bound, so anything stored on the realized
  bubble (not the VM) is lost or leaks onto another turn.
- **Treat the `RichTextBox` as expensive.** Consider capping the number of
  retained turns, or rendering off-screen turns with a lighter read-only
  representation and promoting to a full `RichTextBox` only when the container
  is realized.
- **Mind the tension with pin-to-bottom auto-scroll.** A streaming chat usually
  pins the view to the newest content with a stick-to-bottom `ScrollViewer`
  behavior. `CanContentScroll="True"` hands scrolling to the items panel, and
  the unit of `VerticalOffset` / `ScrollableHeight` then depends on
  `VirtualizingPanel.ScrollUnit`: with `Item` (the WPF default) they are
  measured in item units, while with the `Pixel` setting recommended in Quick
  Setup above they stay pixel-based. Either way the extent now changes as
  containers realize/virtualize — reconcile the auto-scroll "am I at the
  bottom" math with the scroll unit you enable here.

### Related topics

- [`displaying-selectable-rich-text-in-wpf`](../displaying-selectable-rich-text-in-wpf/TOPIC.md) — the heavy, content-hugging read-only `RichTextBox` bubble whose per-item cost motivates virtualization, and whose selection/scroll state must live in the VM under recycling.
- [`styling-chat-bubbles-in-wpf`](../styling-chat-bubbles-in-wpf/TOPIC.md) — the role-differentiated bubble `DataTemplate` rendered inside the virtualized item host.
