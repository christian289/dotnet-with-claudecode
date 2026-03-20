---
description: "Fixes FlaUI mouse click/drag failures in WPF UI automation. Use when FlaUI drag-and-drop silently fails, Mouse.Down has no effect, SendInput click is ignored, adorner flickers during drag, or Keyboard.Press breaks the next mouse gesture. Covers stuck-key diagnosis, ReleaseAllKeys pattern, SetCursorPos vs SendInput hit-test issues, and drag interpolation tuning."
---

# FlaUI Cross-Process Input for WPF

When automating WPF applications with FlaUI from a separate test process, mouse gestures can fail silently due to cross-process input delivery timing. This skill covers the two most common causes and their fixes.

## Problem 1: Stuck Keys Block WPF Gesture Matching

FlaUI's `Keyboard.Press()` sends key-down + key-up via `SendInput`, but the target WPF process may not have processed the key-up by the time the next mouse event arrives. WPF controls that check `Keyboard.IsKeyDown()` during mouse handling (e.g., Nodify's `MouseGesture.MatchesKeyboard()`) silently reject the gesture because they see a key still pressed.

**Symptoms:**
- Mouse-down on a control produces no visual feedback (e.g., no filled effect, no drag start)
- The same interaction works when the app runs without the test session
- No exceptions or errors anywhere — the gesture is silently ignored

**Diagnosis:** Add a `PreviewMouseLeftButtonDown` handler to the target control's parent (e.g., NodifyEditor) that logs keyboard state, mouse capture, and hit test results to a temp file. This is the fastest way to identify the root cause:

```csharp
// Add to the View's code-behind (temporary diagnostic — remove after debugging)
editor.PreviewMouseLeftButtonDown += (sender, e) =>
{
    var position = e.GetPosition((UIElement)sender);
    var hitResult = VisualTreeHelper.HitTest((Visual)sender, position);

    // Walk visual tree to check if hit target is the expected control
    bool isOverTarget = false;
    var current = hitResult?.VisualHit as DependencyObject;
    while (current is not null)
    {
        if (current is Nodify.Connector) // or your target control type
        {
            isOverTarget = true;
            break;
        }
        current = VisualTreeHelper.GetParent(current);
    }

    // Check for stuck keys
    var pressedKeys = new List<string>();
    foreach (Key key in Enum.GetValues(typeof(Key)))
    {
        if (key != Key.None && Keyboard.IsKeyDown(key))
        {
            pressedKeys.Add(key.ToString());
        }
    }

    string log = $"""
        [{DateTime.Now:HH:mm:ss.fff}] PreviewMouseLeftButtonDown
          ClickCount: {e.ClickCount}
          Mouse.Captured: {Mouse.Captured?.GetType().FullName ?? "null"}
          IsOverTarget: {isOverTarget}
          HitTest: {hitResult?.VisualHit?.GetType().Name ?? "null"}
          PressedKeys: [{string.Join(", ", pressedKeys)}]
        """;

    File.AppendAllText(
        Path.Combine(Path.GetTempPath(), "flaui_input_diag.log"), log + "\n");
};
```

If `PressedKeys` is not empty when it shouldn't be, stuck keys are the cause. If `IsOverTarget` is false, the hit test is missing the control. If `Mouse.Captured` is not null, another element is blocking mouse capture.

**Fix — force key-up before mouse interaction:**

```csharp
using System.Runtime.InteropServices;

public static partial class InputHelper
{
    [LibraryImport("user32.dll")]
    private static partial void keybd_event(
        byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static void ReleaseAllKeys()
    {
        byte[] keysToRelease =
        [
            0x2E, // VK_DELETE
            0x1B, // VK_ESCAPE
            0x0D, // VK_RETURN
            0x20, // VK_SPACE
            0x09, // VK_TAB
            0x11, // VK_CONTROL
            0x10, // VK_SHIFT
            0x12, // VK_MENU (Alt)
        ];

        foreach (byte vk in keysToRelease)
        {
            keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
```

Call `ReleaseAllKeys()` + `Thread.Sleep(200)` at every keyboard-to-mouse transition point:

```csharp
// After keyboard operations
Keyboard.Press(VirtualKeyShort.DELETE);
Thread.Sleep(500);

// Force-release before mouse gesture
InputHelper.ReleaseAllKeys();
Thread.Sleep(200);

// Now the mouse gesture works
Mouse.MoveTo(target);
Mouse.Down(MouseButton.Left);
```

## Problem 2: SetCursorPos Doesn't Update WPF Hit Test

FlaUI's `Mouse.MoveTo()` uses Win32 `SetCursorPos`, which repositions the cursor but does not inject `WM_MOUSEMOVE` into the target process. WPF only updates its visual hit test when `WM_MOUSEMOVE` is received, so the click may go to the wrong element.

**Symptoms:**
- Cursor is visually over the correct element
- But the click activates a different element or does nothing

**Fix — use `SendInput` with `MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE`:**

```csharp
public static void SendInputMove(Point screenPoint)
{
    int w = GetSystemMetrics(SM_CXSCREEN);
    int h = GetSystemMetrics(SM_CYSCREEN);
    int nx = screenPoint.X * 65535 / w;
    int ny = screenPoint.Y * 65535 / h;

    var inputs = new INPUT[]
    {
        new()
        {
            type = INPUT_MOUSE,
            mi = new MOUSEINPUT
            {
                dx = nx, dy = ny,
                dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE
            }
        }
    };
    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
}
```

For atomic move + button press (prevents ClickCount miscalculation):

```csharp
public static void SendInputMoveAndDown(Point screenPoint)
{
    var (nx, ny) = ToNormalized(screenPoint);
    var inputs = new INPUT[]
    {
        new()
        {
            type = INPUT_MOUSE,
            mi = new MOUSEINPUT
            {
                dx = nx, dy = ny,
                dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE
            }
        },
        new()
        {
            type = INPUT_MOUSE,
            mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN }
        }
    };
    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
}
```

## Problem 3: Adorner Flickering During Drag

When sending interpolated mouse-move events during a drag, the target WPF control may show rapid visual flickering — adorners or preview elements appearing and disappearing in short cycles. This happens because intermediate drag points cross over interactive zones (e.g., other connectors, drop targets) that trigger hover/preview state changes on each entry and exit.

**Symptoms:**
- Drag preview (adorner, highlight, dotted line) rapidly appears and disappears
- The drag itself may still complete, but the visual behavior is glitchy
- Reducing the number of interpolation steps may fix it, or make it worse

**Fixes:**

1. **Increase sleep between move steps** — give the WPF dispatcher time to settle each frame:
```csharp
// Too fast — causes flickering
for (int i = 1; i <= 10; i++)
{
    SendInputMove(Interpolate(source, target, i, 10));
    Thread.Sleep(10); // too short
}

// Better — each step gets enough processing time
for (int i = 1; i <= steps; i++)
{
    SendInputMove(Interpolate(source, target, i, steps));
    Thread.Sleep(50); // enough for WPF dispatcher to settle
}
```

2. **Use fewer intermediate steps** — 3-5 steps is often enough to register a drag without crossing unrelated hit zones:
```csharp
// Minimal: source → midpoint → target
SendInputMove(source);
Thread.Sleep(100);
var mid = new Point((source.X + target.X) / 2, (source.Y + target.Y) / 2);
SendInputMove(mid);
Thread.Sleep(100);
SendInputMove(target);
Thread.Sleep(100);
```

3. **Route around obstacles** — if intermediate points must avoid other connectors or interactive elements, offset the path vertically:
```csharp
// Arc above/below the straight line to avoid crossing other connectors
var mid = new Point(
    (source.X + target.X) / 2,
    Math.Min(source.Y, target.Y) - 50); // offset above
```

## Quick Reference

| Scenario | Action |
|----------|--------|
| After `Keyboard.Press()`, before mouse interaction | `ReleaseAllKeys()` + `Thread.Sleep(200)` |
| Moving cursor for WPF hit test | `SendInput(MOUSEEVENTF_MOVE \| MOUSEEVENTF_ABSOLUTE)` instead of `Mouse.MoveTo()` |
| Move + click atomically | Bundle MOVE and LEFTDOWN in a single `SendInput` call |
| Drag between two points | 3-5 intermediate `SendInput(MOVE)` steps, `Thread.Sleep(50)` between each |
| Drag causes adorner flickering | Fewer steps, longer sleep, or route the path around interactive zones |
| First click on background window | `SetForegroundWindow` via `AttachThreadInput` — the first click activates the window without reaching the control |

## Diagnostic Checklist

When a FlaUI mouse gesture fails silently on a WPF control, check these in order:

1. **Keyboard state** — Log `Keyboard.IsKeyDown()` in `PreviewMouseLeftButtonDown`. Any stuck key blocks gesture matching.
2. **`Mouse.Captured`** — If another element holds capture, `CaptureMouse()` silently fails.
3. **`ClickCount`** — Must be 1 for single-click gestures. Cross-process timing can produce 0 or 2.
4. **Hit test target** — `VisualTreeHelper.HitTest()` in `PreviewMouseDown` to verify the correct element is under the cursor.
5. **Correct binary** — When using git worktrees or separate build outputs, verify the test launches the exe that contains your changes.
