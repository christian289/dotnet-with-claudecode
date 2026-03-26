---
description: "Syncs WPF Keyboard.Modifiers with ScottPlot's internal keyboard state before processing MouseWheel events. Use when ScottPlot WPF control requires Ctrl+MouseWheel for zoom but only works after clicking to give focus. Use when ScottPlot MouseWheel zoom ignores modifier keys without keyboard focus."
---

# Syncing Modifier Keys for MouseWheel in ScottPlot WPF Control

ScottPlot manages its own internal keyboard state via KeyDown/KeyUp events. Since these events require keyboard focus, modifier-dependent MouseWheel interactions (e.g., Ctrl+Wheel zoom) fail until the user clicks the control.

## 1. Problem Scenario

### Symptoms

- Ctrl+MouseWheel zoom only works **after clicking** the control once
- MouseWheel events fire (mouse position-based), but modifier key state is not tracked
- Losing keyboard focus (clicking elsewhere) breaks Ctrl+Wheel again

### Root Cause

WPF routes mouse events and keyboard events differently:

| Event | Routing Basis | Requires Keyboard Focus |
|-------|--------------|------------------------|
| `MouseWheel` | Mouse position (hit-test) | No |
| `MouseMove`, `MouseDown` | Mouse position (hit-test) | No |
| `KeyDown`, `KeyUp` | Keyboard focus | **Yes** |

ScottPlot tracks modifier keys through `KeyDown`/`KeyUp` into its internal `KeyboardState`. Without keyboard focus, `KeyDown` never fires, so the internal state never records Ctrl as pressed. The `MouseWheel` event arrives, but the zoom handler checks `keys.IsPressed(Control)` against the empty internal state and does nothing.

---

## 2. Solution: Sync from Keyboard.Modifiers

`System.Windows.Input.Keyboard.Modifiers` reads the OS-level key state **globally**, regardless of which element has keyboard focus.

Inject the current modifier state into the control's input processor **before** processing each MouseWheel event:

```csharp
using System.Windows;
using System.Windows.Input;

public static void ProcessMouseWheel(
    this UserInputProcessor processor,
    FrameworkElement fe,
    MouseWheelEventArgs e)
{
    // Sync modifier keys from WPF global state before processing
    SyncModifierKeys(processor);

    Pixel pixel = e.ToPixel(fe);

    IUserAction action = e.Delta > 0
        ? new MouseWheelUp(pixel)
        : new MouseWheelDown(pixel);

    processor.Process(action);
}

private static void SyncModifierKeys(UserInputProcessor processor)
{
    IUserAction action = (Keyboard.Modifiers & ModifierKeys.Control) != 0
        ? new KeyDown(StandardKeys.Control)
        : new KeyUp(StandardKeys.Control);

    processor.Process(action);
}
```

### Key Points

- `Keyboard.Modifiers` is **focus-independent** (OS-level global query)
- Send `KeyDown`/`KeyUp` to the input processor to update its internal keyboard state
- Sync **only the modifier keys** actually used by the control's interaction responses
- Place the sync call **before** the MouseWheel action processing

---

## 3. Why Not Focus on MouseEnter?

An alternative approach is to call `Keyboard.Focus(this)` on `MouseEnter`:

```csharp
// Alternative approach - has side effects
protected override void OnMouseEnter(MouseEventArgs e)
{
    Keyboard.Focus(this);
    base.OnMouseEnter(e);
}
```

| Approach | Pros | Cons |
|----------|------|------|
| **Sync Keyboard.Modifiers** | No side effects, no focus stealing | Must be called on each wheel event |
| **Focus on MouseEnter** | Simple | Steals focus from TextBox/other inputs, disrupts Tab navigation |

**Prefer Keyboard.Modifiers sync** - it solves the problem without affecting focus behavior.

---

## 4. Common Mistakes

### Only syncing on first use

```csharp
// Wrong: sync once and cache
private static bool _synced = false;

private static void SyncModifierKeys(UserInputProcessor processor)
{
    if (_synced) return;  // Ctrl state can change between wheel events!
    _synced = true;
    // ...
}
```

Modifier state must be synced **every time** before processing MouseWheel, because the user may press or release Ctrl between wheel events.

### Syncing after the wheel action

```csharp
// Wrong: sync after processing
processor.Process(action);
SyncModifierKeys(processor);  // Too late - wheel already processed with stale state
```

The sync must happen **before** `processor.Process(wheelAction)`.

---

## 5. Checklist

- [ ] Identify ScottPlot's internal keyboard state mechanism (`UserInputProcessor`)
- [ ] Add `Keyboard.Modifiers` sync in the MouseWheel processing path
- [ ] Sync **before** the wheel action, not after
- [ ] Sync only the modifier keys used by ScottPlot's interaction responses
- [ ] Verify Ctrl+Wheel zoom works without clicking the ScottPlot control first
- [ ] Verify Ctrl+Wheel zoom still works after clicking elsewhere and returning
