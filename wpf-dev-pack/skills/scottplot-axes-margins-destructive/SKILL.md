---
description: "Warns that ScottPlot 5's `plot.Axes.Margins(x, y)` is destructive: it replaces `plot.Axes.AutoScaler` with a new `FractionalAutoScaler` instance and immediately calls `AutoScale()`, wiping prior state such as `InvertedY`. Use when ScottPlot setup code runs in a WPF reactive hot path (IValueConverter, PropertyChanged, slider drag) and a subsequent `AutoScaler.InvertedY = true` no longer inverts the Y axis, or when axis-sign-sensitive overlays (Rectangle, Polygon) flip between converter invocations while `ImageRect` appears stable."
user-invocable: false
---

# Destructive `Axes.Margins(x, y)` in ScottPlot 5 — InvertedY Lost on Reactive Hot Paths

ScottPlot 5's `plot.Axes.Margins(double, double)` is named like a simple setter but is actually destructive: it replaces the entire `AutoScaler` instance and immediately runs an auto-scale pass. When ScottPlot setup code is invoked once (cookbook style) this is harmless, but in WPF reactive scenarios where the same setup runs many times — IValueConverter on a binding that re-evaluates, PropertyChanged on a wrapper, slider drag — any state stored on the previous `AutoScaler` (most notably `InvertedY`) is wiped on every call.

## 1. Problem Scenario

### Symptoms

- `plot.Axes.AutoScaler.InvertedY = true` is set right after `plot.Axes.Margins(0, 0)`, but the Y axis is not inverted.
- The bug is **invisible while only `ImageRect` is on the plot** — `ImageRect` paints relative to its own `CoordinateRect`, so the visual stays correct regardless of axis sign.
- The bug becomes visible the moment an **axis-sign-sensitive overlay** (`Rectangle`, `Polygon`, custom annotation) is added to the same plot — overlays follow the axis range sign and appear Y-flipped.
- Triggered by interactive changes (colormap toggle, slider drag, PropertyChanged on a wrapper) that re-run a single setup function.

### Root Cause: `Margins(double, double)` Replaces the AutoScaler

Inside `AxisManager.Margins(double horizontal, double vertical)` (ScottPlot 5.x), two side effects fire in order:

1. `plot.Axes.AutoScaler` is **replaced with a new `FractionalAutoScaler(horizontal, vertical)`** instance. The new AutoScaler's `InvertedY` defaults to `false`. Any state on the previous AutoScaler is discarded.
2. `AutoScale()` is **called immediately** against the new (un-inverted) AutoScaler, recomputing axis ranges in non-inverted orientation.

Therefore the common pattern below is a silent footgun in reactive code:

```csharp
private Plot ConfigureImagePlot(Plot plot, IImageSourceContainer container)
{
    ConfigurePlot(plot);                            // may itself call AutoScale()
    plot.Axes.Margins(0, 0);                        // destructive: new AutoScaler + AutoScale
    plot.Axes.AutoScaler.InvertedY = true;          // sets flag on the NEW AutoScaler
                                                    // axis range is already non-inverted
    // ...
}
```

After the third line, the new AutoScaler holds `InvertedY = true`, but the YAxis range was already recomputed with `InvertedY = false` on line 2. The flag has no retroactive effect — it only matters on the **next** `AutoScale()` call.

### Why the Bug Sometimes Hides

`InvertedY = true` set lazily can appear to work if some other code path triggers an `AutoScale()` later in the same pass. A common masking pattern: `ConfigurePlot()` itself called `AutoScale()` after the `Margins` + `InvertedY` lines, by coincidence re-inverting the axis. Moving that `AutoScale()` (refactor) exposes the latent destructiveness.

---

## 2. Solution: Update Margins In-Place via `FractionalAutoScaler`

`FractionalAutoScaler.SetMarginsX(double)` and `SetMarginsY(double)` mutate the existing instance's margin fractions without replacing the AutoScaler and without calling `AutoScale()`. The previously computed inverted Y range and the `InvertedY = true` flag are both preserved.

```csharp
using ScottPlot.AutoScalers;

private Plot ConfigureImagePlot(Plot plot, IImageSourceContainer container)
{
    ConfigurePlot(plot);

    if (plot.Axes.AutoScaler is FractionalAutoScaler fas)
    {
        fas.SetMarginsX(0);
        fas.SetMarginsY(0);
    }
    plot.Axes.AutoScaler.InvertedY = true;

    // ...
}
```

If the AutoScaler is not a `FractionalAutoScaler` (custom AutoScaler injected upstream), the pattern-match `is FractionalAutoScaler fas` branch is skipped and the InvertedY assignment still applies cleanly.

### Alternative: Run Setup Once, Not on Every Reactive Tick

If `Margins(x, y)` is genuinely needed (custom AutoScaler swap is intended), call it only on the **first** materialization of the plot — e.g., when an `ImageRect` is first inserted into the `PlottableList` — and skip it on subsequent in-place swaps. This separates the cookbook-once setup from the reactive update path.

---

## 3. When This Fires

All of the following must hold for the bug to manifest visually:

| Condition | Detail |
|---|---|
| WPF binding triggers same setup repeatedly | `IValueConverter`, `PropertyChanged` on a wrapper, slider drag, colormap toggle |
| Setup function calls `Margins(x, y)` + sets `InvertedY = true` together | The destructive call and the lazy flag set are adjacent |
| Plot contains an axis-sign-sensitive overlay | `Rectangle`, `Polygon`, custom annotation — anything that paints in axis coordinates and follows the range sign |

`ImageRect`-only plots will not surface this bug visually because `ImageRect` paints in its own `CoordinateRect` frame regardless of axis direction. Adding a `Rectangle` overlay on top of the same plot exposes the latent flip.

---

## 4. Why ScottPlot 5 Designs It This Way

ScottPlot 5's cookbook examples assume a single setup invocation followed by interactive use. `Margins(x, y)` is documented as a convenience for picking a different `AutoScaler` shape (margin fractions); replacing the AutoScaler instance and running `AutoScale()` is intentional in that context — it gives the user a "set the margins and see the result immediately" experience.

The destructiveness conflicts only with the reactive WPF data-binding model, where the same cookbook-once setup is re-executed by the framework on every binding tick. This skill exists to bridge that gap.

---

## 5. Common Mistakes

### Treating `InvertedY` as Eager

```csharp
// Wrong: InvertedY is lazy. The next AutoScale call uses the flag,
// but assigning it does not retroactively invert an already-computed range.
plot.Axes.Margins(0, 0);
plot.Axes.AutoScaler.InvertedY = true;
plot.YAxis.Range.Set(0, height);   // still non-inverted at this point
```

### Adding an Extra `AutoScale()` to "Fix" It

```csharp
// Wrong: works by accident, but every invocation triggers an extra
// AutoScale pass. Hot-path overhead and timing fragility.
plot.Axes.Margins(0, 0);
plot.Axes.AutoScaler.InvertedY = true;
plot.Axes.AutoScale();   // forces the lazy flag to take effect
```

Use `SetMarginsX/Y` on the existing `FractionalAutoScaler` instead.

### Assuming `ImageRect`-only Visual Stability Means the Axis is Correct

```csharp
// Wrong: ImageRect masks axis direction issues. If you later add a Rectangle
// overlay to the same PlotControl, it will flip — and the root cause is here.
plot.Axes.Margins(0, 0);
plot.Axes.AutoScaler.InvertedY = true;
```

Verify with an axis-sign-sensitive overlay during development.

---

## 6. Validation

After applying the fix:

1. Add a `Rectangle` (or `Polygon`) overlay to the same `PlotControl` with the `ImageRect`.
2. Trigger the reactive update repeatedly — drag a colormap slider, toggle a checkbox bound to the same converter, force PropertyChanged on the wrapper.
3. Confirm the `Rectangle` stays in the same screen position across invocations.
4. Confirm the Y axis range still reads as inverted (`Min > Max` in image orientation) after each tick.

---

## 7. Related Skills

- `scottplot-syncing-modifier-keys-for-mousewheel` — covers a different ScottPlot 5 WPF integration footgun (modifier-key state not synced for `MouseWheel` without keyboard focus). Both skills share the broader pattern: **ScottPlot 5 APIs are designed for cookbook-once setup and need bridging code when reused on a WPF reactive hot path**.

---

## 8. Checklist

- [ ] Audit every call site of `plot.Axes.Margins(double, double)` in WPF projects.
- [ ] For each call site, determine whether it runs in a reactive hot path (Converter, PropertyChanged, command handler that re-runs).
- [ ] Replace destructive `Margins(x, y)` with `FractionalAutoScaler.SetMarginsX/Y` when the AutoScaler is a `FractionalAutoScaler`.
- [ ] Verify `InvertedY` (and any other AutoScaler state) is preserved across reactive invocations.
- [ ] Add an axis-sign-sensitive overlay (`Rectangle`, `Polygon`) during development to surface latent flips.
- [ ] Confirm no extra `AutoScale()` calls are introduced as a workaround.
