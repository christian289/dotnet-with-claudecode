# Virtualization Patterns

UI virtualization rules for WPF controls that display large collections.

---

## ItemsControl with VirtualizingStackPanel

Plain `ItemsControl` does not virtualize by default — it materializes every item at load.
For large collections, replace the default panel with `VirtualizingStackPanel` and enable container recycling.

```xml
<!-- Wrong: no virtualization, all items materialized at once -->
<ItemsControl ItemsSource="{Binding LargeCollection}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

```xml
<!-- Correct: VirtualizingStackPanel with recycling -->
<ItemsControl ItemsSource="{Binding LargeCollection}"
              VirtualizingPanel.IsVirtualizing="True"
              VirtualizingPanel.VirtualizationMode="Recycling"
              VirtualizingPanel.ScrollUnit="Pixel"
              ScrollViewer.CanContentScroll="True">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

---

## ListView / ListBox Attached Properties

`ListView` and `ListBox` use a `VirtualizingStackPanel` by default, but the optimal settings must still be set explicitly via attached properties:

```xml
<ListView ItemsSource="{Binding LargeCollection}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          VirtualizingPanel.ScrollUnit="Pixel"
          ScrollViewer.CanContentScroll="True">
    ...
</ListView>
```

---

## Key Settings

| Attached Property | Recommended Value | Effect |
|---|---|---|
| `VirtualizingPanel.IsVirtualizing` | `True` | Enable UI virtualization |
| `VirtualizingPanel.VirtualizationMode` | `Recycling` | Reuse containers instead of re-creating |
| `VirtualizingPanel.ScrollUnit` | `Pixel` | Smooth pixel scrolling (vs. item-by-item) |
| `ScrollViewer.CanContentScroll` | `True` | Required for panel-level virtualization |

---

## Notes

- `VirtualizationMode="Recycling"` is almost always preferred over `Standard` — it avoids GC pressure from container creation/destruction during scroll.
- `ScrollUnit="Pixel"` eliminates the "jumping" effect that occurs with item-based scrolling when items have variable height.
- Virtualization is disabled if the `ScrollViewer` wrapping the items panel has `CanContentScroll="False"` — always verify this setting.
- Do not place a `VirtualizingStackPanel` inside an `ScrollViewer` with unconstrained height — it will lose virtualization because all items become "visible".
