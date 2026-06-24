# WPF HelixToolkit ESC Key Routing

Why `IsCancel` buttons and `KeyDown` handlers fail to receive the ESC key in dialogs containing a `HelixViewport3D`, and how to fix it.

---

## Symptoms

In a Prism Dialog / Window hosting a `HelixViewport3D`, none of the following work:

- An `IsCancel="True"` button does not respond to the ESC key
- `KeyDown += ...` handlers registered on the Window are never invoked
- A `KeyBinding Key="Escape"` input binding does not fire
- Users can only close the dialog by clicking the X button

Dialogs of the same structure using 2D visualizations (ScottPlot, WpfPlot, etc.) work correctly, so this is a **HelixToolkit-specific issue**.

## Cause: Shape3DDrawingBehavior Consumes ESC During Bubbling

`HelixToolkit.UI`'s `Shape3DDrawingBehavior` (or similar custom behaviors attached to `HelixViewport3D`) is implemented to intercept ESC in order to cancel drawing:

```csharp
// Shape3DDrawingBehavior
protected override void OnAttached()
{
    base.OnAttached();
    AssociatedObject.KeyDown += OnKeyDown;
    AssociatedObject.Focusable = true;  // So the Viewport can receive key input
}

private void OnKeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Escape)
    {
        _currentDrawer?.Cancel();
        e.Handled = true;   // ← Always marks Handled, even when not drawing
    }
}
```

The problem is that `e.Handled = true` always runs regardless of whether drawing is in progress. As a result, ESC **never bubbles up** to the Window level (`IsCancel` routing, Window KeyDown handlers), so it is not processed there.

### WPF Event Routing Recap

| Phase | Event | Direction | Effect of Handled=true |
|------|--------|------|-----------------|
| 1. Tunneling | `PreviewKeyDown` | Root (Window) → Leaf (focused) | Subsequent Preview/KeyDown are skipped (default subscription) |
| 2. Bubbling | `KeyDown` | Leaf → Root (Window) | Subsequent KeyDown handlers are skipped (default subscription) |

`IsCancel` operates in the final bubbling stage (Window), so if any intermediate child marks the event Handled, it never reaches the Window.

## Fix: Handle It in the Window's `OnPreviewKeyDown`

Overriding `OnPreviewKeyDown` at the Window level lets the Window receive ESC during the **first tunneling phase**. At this point, no child has seen the event yet, so no behavior can have marked it Handled.

```csharp
public class CommonWindow : Window
{
    public static readonly DependencyProperty IsEscapeToCloseProperty =
        DependencyProperty.Register(
            nameof(IsEscapeToClose),
            typeof(bool),
            typeof(CommonWindow),
            new PropertyMetadata(true));

    public bool IsEscapeToClose
    {
        get => (bool)GetValue(IsEscapeToCloseProperty);
        set => SetValue(IsEscapeToCloseProperty, value);
    }

    /// <summary>
    /// Logic for closing the dialog with the ESC key.
    /// Why PreviewKeyDown (tunneling): the traditional IsCancel approach fails
    /// if any child control marks KeyDown as Handled, because the event never
    /// reaches the Window. HelixToolkit's Shape3DDrawingBehavior intercepts ESC
    /// and marks it Handled, which causes dialogs containing a 3D Viewport to
    /// not close on ESC. PreviewKeyDown tunnels from the root (Window) down to
    /// the leaf, so it is processed before any child can intervene.
    /// </summary>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (IsEscapeToClose && e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}
```

### Why the `IsCancel` Approach Doesn't Work

One might try embedding an invisible `IsCancel` button in the CommonWindow template like this:

```xml
<!-- Does not work -->
<Button x:Name="PART_EscapeCloseButton"
        IsCancel="{TemplateBinding IsEscapeToClose}"
        Width="0"
        Height="0"
        Focusable="False"/>
```

But `IsCancel` only works when the Window detects ESC from the bubbling KeyDown event. Once the HelixToolkit behavior marks it `Handled=true`, it never reaches the Window, so the invisible button's Click event never fires either.

## Side Effect: ESC UX During Drawing Changes

When `OnPreviewKeyDown` processes ESC first, any drawing operation in progress inside the HelixViewport3D is also terminated by `Window.Close()`. State is usually cleaned up because the behavior's `OnDetaching` calls `_currentDrawer?.Cancel()`, but the original "ESC cancels drawing only, dialog stays open" UX is lost.

To preserve that behavior, `OnPreviewKeyDown` would need to check whether an active drawing exists and skip closing, but this requires the Window to directly reference the behavior, which is not recommended from a layer-separation standpoint.

## Generalization: When a Library Consumes Bubbling KeyDown

This pattern applies beyond HelixToolkit. Consider overriding `PreviewKeyDown` in these situations:

- A third-party control (Helix, SciChart, Telerik 3D, etc.) intercepts `KeyDown`
- A behavior attached via `Interaction.Behaviors` sets `e.Handled = true`
- None of `IsCancel` button / `KeyBinding` / Window `KeyDown` handlers work

In these cases, overriding the tunneling event (`Preview*`) is the only way to bypass bubbling.

## Things to Remember

- **Bubbling** flows leaf → root; if any intermediate handler marks it Handled, the root never sees it
- **Tunneling** flows root → leaf; the root receives the event before any child can mark it Handled
- `IsCancel` / `KeyBinding` / Window `KeyDown` are all bubbling-based
- Overriding `OnPreviewKeyDown` is tunneling-based, so it is unaffected by child interference
- Windows containing a 3D Viewport should always handle ESC in `OnPreviewKeyDown`
