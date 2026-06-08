# FlaUI Capture & Resize Robustness for WPF

> Keeps FlaUI screenshot capture and element manipulation robust when a WPF window or dialog resizes between runs. Use when captured screenshots are clipped or shifted after a window grows/shrinks, a click lands at the wrong spot after a layout change, SizeToContent or dynamic content changes the dialog size, fixed crop rectangles or hard-coded screen coordinates go stale, or a whole UI capture suite breaks because the target window size changed. Covers anchoring on UIA identifiers (ByName / ByControlType / ByClassName / ByAutomationId) instead of coordinates, why Capture.Element follows the element's runtime BoundingRectangle automatically, computing partial-capture rectangles from live bounds instead of fixed pixels, and the resize -> re-run -> passes-without-code-change regression expectation.

The same WPF dialog can render at a different size on every run — dynamic content is added or removed, `SizeToContent` grows the window to fit, variable panels expand, or the DPI scale changes. Automation that finds elements by **UIA identifier** and captures via the element's **live bounds** keeps working unchanged; automation built on **absolute screen coordinates** or **fixed crop rectangles** bakes one layout snapshot into the code and goes stale the moment the window resizes.

## Principle: Anchor on UIA Identifiers, Not Coordinates

Find and capture targets **always** through UIA identifiers — `ByName`, `ByControlType`, `ByClassName`, `ByAutomationId`. These resolve from the live UIA tree and are independent of pixel position and window size. Avoid absolute screen coordinates, fixed crop rectangles, and hard-coded offset constants.

| Approach | On window resize |
|----------|------------------|
| `ByName` / `ByControlType` / `ByClassName` / `ByAutomationId` lookup | Resolves from the live tree — unaffected by position/size |
| `Capture.Element(element)` | Reads the element's current `BoundingRectangle` at capture time — screenshot follows the new size |
| Sub-region computed from `element.BoundingRectangle` | Tracks the element as it moves/resizes |
| Hard-coded screen point (`Mouse.MoveTo(new Point(840, 410))`) | Stale — click lands at the wrong spot |
| Fixed crop rectangle (`Capture.Rectangle(new Rectangle(800, 380, 220, 120))`) | Stale — screenshot is shifted or clipped |

**Why it works:** FlaUI's `Capture.Element` reads the element's current `BoundingRectangle` (computed from the live visual tree) at the moment of capture, so it stores exactly the region the element now occupies. UIA element queries resolve from the live tree with no dependence on pixel position. Coordinate-based input and fixed crop rectangles, by contrast, are a layout snapshot frozen into the code — when the window resizes, that snapshot is stale.

## Capture Follows Resize Automatically

`Capture.Element` (FlaUI.Core v5, `FlaUI.Core.Capturing.Capture`) re-reads the live bounds on every call. No coordinates are baked in, so the screenshot always matches the element's current size.

```csharp
using System.IO;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Definitions;

// Anchor by a UIA identifier — independent of pixel position and window size.
AutomationElement panel = window.FindFirstDescendant(cf =>
    cf.ByAutomationId("ResultPanel"))
    ?? throw new InvalidOperationException("ResultPanel not found");

// Capture.Element reads the element's CURRENT BoundingRectangle at capture
// time, so the screenshot follows any resize with no code change.
Capture.Element(panel).ToFile(Path.Combine(outputDir, "result-panel.png"));
```

## Robust vs Stale Patterns

```csharp
// BAD — absolute screen point + fixed crop rectangle bake in one layout
// snapshot. After the window resizes, the click misses and the crop is
// shifted or clipped.
Mouse.MoveTo(new Point(840, 410));                     // hard-coded screen point
Mouse.Click(MouseButton.Left);
Capture.Rectangle(new Rectangle(800, 380, 220, 120))   // hard-coded crop
    .ToFile(Path.Combine(outputDir, "save-button.png"));
```

```csharp
// GOOD — find by identifier, act through a UIA pattern, capture by element.
// Nothing here depends on the window size.
AutomationElement saveButton = window.FindFirstDescendant(cf =>
    cf.ByAutomationId("SaveButton"))
    ?? throw new InvalidOperationException("Save button not found");

saveButton.AsButton().Invoke();                         // pattern-based action, no coordinates
Capture.Element(saveButton)                             // capture follows resize
    .ToFile(Path.Combine(outputDir, "save-button.png"));
```

## Partial-Region Capture from Runtime Bounds

When you need a sub-region rather than the whole element, compute the rectangle from the element's **runtime** `BoundingRectangle`, using relative padding/expansion. Never crop with fixed pixel constants.

`element.BoundingRectangle` returns a `System.Drawing.Rectangle` in physical pixels, recomputed live. Two robust options:

```csharp
using System.Drawing;   // Rectangle, Point

// Option A — compute an absolute region from live bounds, capture by screen rect.
Rectangle bounds = panel.BoundingRectangle;            // physical pixels, live
int pad = bounds.Height / 10;                           // padding relative to size, not a constant
var topRegion = new Rectangle(
    bounds.Left,
    bounds.Top,
    bounds.Width,
    bounds.Height / 2 + pad);                           // top half + relative padding
Capture.Rectangle(topRegion).ToFile(Path.Combine(outputDir, "result-top.png"));

// Option B — capture a rectangle expressed RELATIVE to the element.
// FlaUI offsets it from the element's current top-left, so it tracks resize too.
var relative = new Rectangle(0, 0, panel.BoundingRectangle.Width, panel.BoundingRectangle.Height / 2);
Capture.ElementRectangle(panel, relative).ToFile(Path.Combine(outputDir, "result-top-rel.png"));
```

> **DPI note:** `BoundingRectangle` is already physical pixels, so capturing from it needs no DPI correction. Only **computed (element-less) DIP offsets** — e.g. a grid cell on an empty canvas — need the DIP -> physical scale. That scaling, and coordinate input in general, belongs to `flaui-cross-process-input`.

## Regression Expectation

When every capture/action step is identifier-based and every sub-region is derived from live bounds, a deliberate resize of the target window is a non-event for the code:

1. Change the target window/dialog size (add content, toggle `SizeToContent`, change DPI scale).
2. Re-run the full UI capture suite.
3. Identifier-based steps **pass without any code change**; only the capture artifacts are regenerated at the new size.

Treat a failure here as a signal that some step still depends on a coordinate or fixed crop — find it and convert it to identifier + live-bounds.

## Quick Reference

| Situation | Do this |
|-----------|---------|
| Capture a dialog/panel that resizes between runs | `Capture.Element(element)` — reads the live `BoundingRectangle` |
| Need a sub-region screenshot | Compute the rect from `element.BoundingRectangle` (relative padding) or use `Capture.ElementRectangle(element, relativeRect)` — never fixed pixels |
| Act on a control | Find by UIA identifier + pattern (`Invoke`/`Toggle`/`SetValue`) or live bounds — never a hard-coded screen point |
| Locate the target element | `ByName` / `ByControlType` / `ByClassName` / `ByAutomationId` — resolved from the live tree, position-independent |
| Window resized; suite re-run | Identifier-based steps pass unchanged; captures regenerate at the new size |
| Coordinate input genuinely required (drag, hit-test) | See `flaui-cross-process-input` (SendInput, interpolation, DPI scaling) |

## See also

- `flaui-wpf-element-discovery` — how to *find* elements when they are missing from the UIA tree (AutomationId placement, `FindAllDescendants` depth, Shape controls without an AutomationPeer). This topic covers how to *anchor* on those identifiers so capture survives resize.
- `flaui-cross-process-input` — how to *inject* input correctly cross-process (SendInput vs `Mouse.MoveTo`, stuck keys, drag interpolation) and the DPI scaling for computed (element-less) coordinates. Adjacent but distinct boundary: that topic is "how to inject input / is the coordinate system right", this topic is "how to anchor so you do not depend on coordinates at all".
