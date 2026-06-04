# 5.7 Popup Control Usage Considerations

> Manages focus behavior for WPF Popup controls using PreviewMouseDown events. Use when Popup loses focus unexpectedly or needs to stay open during user interaction.

In WPF, the Popup control only operates correctly when the WPF Application has focus. When focus moves to another application, the Popup may not display or function properly.

#### 5.7.1 Focus Management Pattern

When using the Popup control in WPF, **you must forcibly acquire focus through the PreviewMouseDown event.**

## Project Structure

The templates folder contains a WPF project example (use latest .NET per version mapping).

```
templates/
└── WpfPopupSample.App/                  ← WPF Application
    ├── Views/
    │   ├── MainWindow.xaml
    │   └── MainWindow.xaml.cs           ← Focus management pattern implementation
    ├── App.xaml
    ├── App.xaml.cs
    ├── GlobalUsings.cs
    └── WpfPopupSample.App.csproj
```

#### 5.7.2 Core Principles

- **Popup operation condition**: Only operates when WPF Application has focus
- **PreviewMouseDown event**: Check focus state on mouse click
- **IsKeyboardFocused check**: Verify keyboard focus status
- **Activate() call**: Activate window to restore focus if not focused
- **For UserControl**: Activate parent window with `Window.GetWindow(this)?.Activate()`

#### 5.7.3 Why Is This Necessary?

1. **Focus moves to another app**: When user clicks another application and returns
2. **Background execution**: Ensure Popup operation when WPF app is in background
3. **User experience**: Ensure Popup always works as expected

**⚠️ Important Notes:**

- This pattern must be applied to all Windows using Popup

#### 5.7.4 References

- [WPF Popup Focus Issue - .NET Dev Forum](https://forum.dotnetdev.kr/t/wpf-popup/8296)

---

## 5.8 Closing a Popup on Item Selection

`StaysOpen="False"` closes the Popup on **outside** clicks only — selecting
an item *inside* the Popup does not close it. For picker-style Popups
(ColormapPicker, FontPicker, …) the user expects the Popup to close on
selection, so the control must wire this explicitly.

The recommended pattern is to bind the Popup's `IsOpen` to the owning
`ToggleButton.IsChecked` (TwoWay) and flip the `ToggleButton.IsChecked`
to `false` when the inner selector raises `SelectionChanged`.

```xml
<ToggleButton x:Name="PART_DropDownToggle"
              Content="Choose colormap" />

<Popup IsOpen="{Binding IsChecked, ElementName=PART_DropDownToggle, Mode=TwoWay}"
       PlacementTarget="{Binding ElementName=PART_DropDownToggle}"
       Placement="Bottom"
       StaysOpen="False"
       AllowsTransparency="True">
    <Border Background="{DynamicResource SolidBackgroundFillColorSecondaryBrush}"
            BorderBrush="{DynamicResource ControlElevationBorderBrush}"
            BorderThickness="1"
            CornerRadius="4"
            Padding="8">
        <ListBox x:Name="PART_ColormapList" />
    </Border>
</Popup>
```

```csharp
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();

    // Detach the old subscription (template can be re-applied).
    if (_colormapList is not null)
        _colormapList.SelectionChanged -= OnColormapSelectionChanged;

    _toggle        = GetTemplateChild("PART_DropDownToggle") as ToggleButton;
    _colormapList  = GetTemplateChild("PART_ColormapList")   as ListBox;

    if (_colormapList is not null)
        _colormapList.SelectionChanged += OnColormapSelectionChanged;
}

private void OnColormapSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    // Closing the toggle flips Popup.IsOpen to false via TwoWay binding.
    if (_toggle is not null)
        _toggle.IsChecked = false;
}
```

**Notes:**

- Detach the old handler in `OnApplyTemplate` before reattaching — the
  template can be re-applied (theme change, runtime style swap) and a
  leaked subscription will fire on a stale list.
- `StaysOpen="False"` outside-click closing and the
  `SelectionChanged`-driven closing compose cleanly; both flip the same
  `IsChecked`/`IsOpen` source of truth.

## 5.9 Surface Brush Opacity (Acrylic vs Solid)

When a Popup or flyout shows over editable content, choose the surface
brush deliberately:

| Brush family (WPF-UI Fluent) | Behavior |
|---|---|
| `CardBackgroundFillColorDefaultBrush` / `*PrimaryBrush` | Translucent acrylic — the editor underneath shows through |
| `SolidBackgroundFillColorPrimaryBrush` / `*SecondaryBrush` | Opaque — masks the underlying content |

For pickers, dialogs, and flyouts where the underlying content must not
distract the user, prefer the **solid** family. The acrylic family is for
ambient surfaces (Tooltip, NavigationView pane), not for focused pickers.
See [`integrating-wpfui-fluent`](../integrating-wpfui-fluent/SKILL.md) for
the full WPF-UI brush taxonomy.

