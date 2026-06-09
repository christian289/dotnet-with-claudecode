# Containing Decorative Overflow in WPF Custom Controls

> Fixes WPF custom-control decorative visuals (focus ring, hover glow, selection adorner) being clipped or mis-centered. Root cause is usually an ancestor ClipToBounds/CornerRadius or a control layout box smaller than its largest visual — not the visual's own size. Use when FocusRing/HoverGlow/glow adorner gets shaved at a control or track edge, enlarging the glow does not stop the clipping, a decorative ellipse/border larger than the control's Width/Height is off-center, a thumb/handle at a range slider extreme is cut asymmetrically (min vs max), or a clean straight-edge containment look is needed instead of full overflow. Do NOT use when the clipping is desired and already correct, when the issue is mouse hit-testing on OnRender visuals (use implementing-hit-testing), or for pure DrawingContext/DrawingVisual render performance (use rendering-* skills).

A guide for diagnosing and fixing WPF custom-control decorations
(focus ring, hover glow, selection halo, range-slider thumb adornment)
that get **clipped at an ancestor boundary** or **mis-centered** because the
control's layout box is smaller than its largest visual.

## TL;DR

Most decorative-clipping bugs are not solved by enlarging the decoration.
The clipper is almost always an **ancestor's** `ClipToBounds` /
`CornerRadius`, or the control's own layout box is **smaller than the largest
visual that must fit inside it**. Enlarge the box (and keep the code's
position constants in sync), and separate the decoration layer from any
ancestor that clips.

---

## 1. Diagnose the Real Clipper

When a focus ring, hover glow, or selection halo is shaved at an edge,
ask in this order:

1. **Is the control's own layout box smaller than the largest visual?**
   - Visible circle is 28 px but ring is 34 px → the 28 px layout box
     cannot contain the 34 px ring. Enlarging the ring does nothing; the
     box is the limit.
2. **Is there an ancestor with `ClipToBounds=True` or a rounded
   `CornerRadius`?**
   - A `Border` with `ClipToBounds=True` clips every descendant,
     regardless of how large the descendant element is set.
   - A `Border` with `CornerRadius` implicitly rounds and clips its content
     near the corners.
3. **Is a sibling control drawn on top at an extreme position?**
   - At `min`/`max` positions on a range slider, the *next* sibling input
     box can paint over the thumb's decoration, producing **asymmetric**
     clipping (one side cut, the other intact).

Only step 1 is fixed by sizing the element; steps 2 and 3 require a
**layout restructuring**, not a sizing tweak.

---

## 2. Expand the Layout Box + Keep Code Constants in Sync

If the largest decoration around a 28 px visible circle is 43 px, the host
element's `Width`/`Height` must be **≥ 43 px** so the decoration can be
centered on the circle without being trimmed.

```xml
<!-- Thumb visible circle is 28; the box must accommodate the 43-wide HoverGlow -->
<Style TargetType="{x:Type local:ColormapRangeSliderThumb}">
    <Setter Property="Width"  Value="44" />
    <Setter Property="Height" Value="44" />
</Style>
```

```csharp
public class ColormapRangeSliderThumb : Control
{
    // MUST match the Style's Width/Height above.
    // If these two get out of sync the thumb mis-positions silently.
    public const double ThumbBoxSize = 44d;

    private static double PositionFromValue(double value, double trackWidth)
        => value * (trackWidth - ThumbBoxSize) + ThumbBoxSize / 2d;
}
```

**Synchronization rule:** the layout-box size in the Style and the
position-calculation constant in code must always agree. A negative
`Margin` to "shift the visual back into view" is a symptom hack — the
underlying layout box is still wrong.

---

## 3. Separate Content and Decoration Layers

If content has to be clipped (e.g., a rounded spectrum band) but decoration
must overflow that clip, **split them into sibling layers**, not into a
parent-child pair where the decoration lives inside the clipper.

```xml
<Grid>
    <!-- Content layer: must stay inside the rounded track -->
    <Border x:Name="PART_Track"
            ClipToBounds="True"
            CornerRadius="6"
            Background="{TemplateBinding ColormapBrush}" />

    <!-- Decoration / thumb layer: outside the track's clip -->
    <Canvas x:Name="PART_ThumbLayer"
            ClipToBounds="False">
        <local:ColormapRangeSliderThumb x:Name="PART_LowThumb"  />
        <local:ColormapRangeSliderThumb x:Name="PART_HighThumb" />
    </Canvas>
</Grid>
```

Putting the thumb **inside** `PART_Track` causes the thumb's focus ring and
hover glow to be cut at the track's rounded edge, regardless of how large
the ring is set.

---

## 4. Rectangular vs Rounded Clip for Edge Containment

If the **intent** is "the decoration is allowed to overflow most of the
control but must be clipped cleanly at the outer extremes", use a
**rectangular** `ClipToBounds` on the decoration layer — not a rounded
clip. A rounded clip cuts the decoration along a curve and looks broken at
the corners; a rectangular clip yields the clean straight edge most UIs
intend.

```xml
<!-- Decoration is clipped at the outer rectangle only — corners stay straight -->
<Canvas x:Name="PART_ThumbLayer"
        ClipToBounds="True" />
```

Decide consciously per layer:

| Layer | Clip choice |
|---|---|
| Rounded content (spectrum band, card body) | `ClipToBounds` + `CornerRadius` |
| Decoration that must "stop at the wall" cleanly | Rectangular `ClipToBounds` on the decoration layer |
| Decoration that must overflow freely | No clip on the decoration layer, parent panel has `ClipToBounds="False"` |

---

## 5. Sibling Z-Order and Adjacent Opaque Controls

`Panel.ZIndex` controls which sibling paints on top. If an adjacent opaque
control is painted after the decoration, it covers the overflow.

```xml
<!-- Ensure the thumb layer paints above the adjacent numeric box -->
<Canvas x:Name="PART_ThumbLayer" Panel.ZIndex="2" ClipToBounds="True" />
<local:NumericInputBox      x:Name="PART_LowInput"  Panel.ZIndex="1" />
```

The asymmetric "one side cut, the other intact" symptom at a slider's
extremes is almost always a z-order problem, not a clip problem.

---

## 6. Common Pitfalls

- **Negative `Margin` to fake a larger box** — hides the symptom while
  position math stays wrong. Fix the layout box instead.
- **Enlarging the decoration** (FocusRing 34 → 50) — does nothing if the
  clipper is an ancestor.
- **`ClipToBounds="False"` on the wrong element** — must be set on the
  ancestor that has the clip, not on a descendant.
- **One `x:Name` for both the box and the visible circle** — confuses
  layout vs paint sizes. Keep the visible visual a child of an explicitly
  sized host.
- **Animating `Margin` to "compensate"** — fights the layout system on
  every frame; fragile under DPI/scale changes.

---

## 7. Verification Checklist

Before considering the bug fixed, **eyeball the control at all extremes**:

- [ ] At the minimum position the decoration is symmetric and not cut.
- [ ] At the maximum position the decoration is symmetric and not cut.
- [ ] Midway — the decoration is correctly centered on the visible visual.
- [ ] Focus state shows the full focus ring.
- [ ] Hover state shows the full hover glow.
- [ ] Adjacent sibling controls do not paint over the decoration.
- [ ] `dotnet build` of the full solution succeeds (no XAML/binding
      regressions introduced by the layer restructuring).

---

## Related Skills

- `implementing-hit-testing` — mouse event capture on `OnRender` visuals;
  distinct from visual clipping.
- `authoring-wpf-controls` — general control authoring decision tree and
  Template Part / Visual State naming contracts.
- `rendering-with-drawingcontext` / `rendering-with-drawingvisual` —
  pure-render performance topics; unrelated to this layout-clip issue.
