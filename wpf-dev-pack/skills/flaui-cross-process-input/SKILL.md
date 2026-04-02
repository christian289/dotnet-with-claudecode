---
description: "Fixes FlaUI mouse click/drag failures in WPF UI automation. Use when FlaUI drag-and-drop silently fails, Mouse.Down has no effect, SendInput click is ignored, adorner flickers during drag, Keyboard.Press breaks the next mouse gesture, ReleaseAllKeys causes unexpected panning, canvas drag selection hits the wrong position, or FlaUI xUnit tests fail intermittently due to parallel execution. Covers xUnit parallel test disabling (xunit.runner.json), stuck-key diagnosis, ReleaseAllKeys vs ReleaseModifierKeys pattern, Keyboard.Press vs Type distinction, SetCursorPos vs SendInput hit-test issues, drag interpolation tuning, and UIA BoundingRectangle-based coordinate calculation."
user-invocable: false
model: sonnet
---

# FlaUI Cross-Process Input for WPF

When automating WPF applications with FlaUI from a separate test process, mouse gestures can fail silently due to cross-process input delivery timing. This skill covers the two most common causes and their fixes.

## Prerequisite: Disable xUnit Parallel Execution

FlaUI tests control a shared OS resource (mouse, keyboard, window focus). Parallel test execution causes tests to fight over these resources, producing random failures that are impossible to diagnose.

**Create `xunit.runner.json` in the test project root:**

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false
}
```

**Add to `.csproj`:**

```xml
<ItemGroup>
  <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

| Setting | Purpose |
|---------|---------|
| `parallelizeAssembly: false` | 어셈블리 간 병렬 실행 비활성화 — 여러 테스트 프로젝트가 동시에 UI를 조작하는 것을 방지 |
| | Disables cross-assembly parallelism — prevents multiple test projects from manipulating UI simultaneously |
| `parallelizeTestCollections: false` | 컬렉션 간 병렬 실행 비활성화 — 하나의 테스트가 끝나야 다음 테스트가 마우스/키보드 사용 |
| | Disables cross-collection parallelism — ensures one test finishes before the next uses mouse/keyboard |

> **Warning:** 이 설정 없이 FlaUI 테스트를 실행하면, 테스트가 간헐적으로 실패하며 원인을 키보드 stuck key나 hit test 문제로 오진하기 쉽습니다.
> Without this configuration, FlaUI tests fail intermittently and the root cause is easily misdiagnosed as stuck keys or hit test issues.

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

## Problem 4: ReleaseAllKeys Sends Non-Modifier Key-Up That Breaks Gesture Matching

When `ReleaseAllKeys()` sends key-up for non-modifier keys like `VK_DELETE (0x2E)`, the target WPF process receives a DELETE key-up event. Controls that check `Keyboard.IsKeyDown()` during `MouseGesture.Matches()` (e.g., Nodify's `IsAnyKeyPressed()`) may see transient key state during this processing window, causing the next mouse gesture to fail silently.

**Symptoms:**
- `ReleaseAllKeys()` before a mouse click causes a slight "drag" or "pan" effect instead of a clean click
- Node selection works when clicking manually but fails after `ReleaseAllKeys()` in FlaUI
- The issue is intermittent and timing-dependent

**Root cause:** Nodify's `MouseGesture.MatchesKeyboard()` for plain LeftClick checks `!IsAnyKeyPressed()`. A stale DELETE key-up in the OS message queue makes this return `true` during the critical mouse-down window.

**Fix — separate modifier-only release:**

```csharp
/// <summary>
/// Releases only modifier keys (Ctrl, Shift, Alt).
/// Non-modifier keys (DELETE, ESCAPE, etc.) are excluded because their
/// key-up events can create transient states that break gesture matching
/// in controls that check IsAnyKeyPressed() during mouse events.
/// </summary>
public static void ReleaseModifierKeys()
{
    byte[] modifierKeys =
    [
        0x11, // VK_CONTROL
        0x10, // VK_SHIFT
        0x12, // VK_MENU (Alt)
    ];

    foreach (byte vk in modifierKeys)
    {
        keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
```

**When to use each:**

| Method | Use when |
|--------|----------|
| `ReleaseAllKeys()` + **500ms** delay | After `Keyboard.Press(ESCAPE)` or `Keyboard.Press(DELETE)` left keys stuck — needs time for all key states to settle |
| `ReleaseModifierKeys()` + **200ms** delay | Between keyboard and mouse operations where only Ctrl/Shift/Alt may be stuck — avoids injecting non-modifier key-up noise |

## Problem 5: Keyboard.Press Leaves Keys Permanently Down

FlaUI's `Keyboard.Press(key)` sends **key-down only** — it does NOT send key-up. The key remains "pressed" in the OS input state until explicitly released. This accumulates across test steps, leaving multiple keys stuck.

**Symptoms:**
- After `ResetEditorState()` calls `Keyboard.Press(ESCAPE)` twice, VK_ESCAPE stays pressed
- After `ClearDesigner()` calls `Keyboard.Press(DELETE)`, VK_DELETE stays pressed
- Subsequent mouse gestures fail because `IsAnyKeyPressed()` returns `true`

**Fix — use `Keyboard.Type()` instead of `Keyboard.Press()`:**

```csharp
// BAD — key-down only, DELETE stays pressed
Keyboard.Press(VirtualKeyShort.DELETE);

// GOOD — sends key-down + key-up, DELETE is properly released
Keyboard.Type(VirtualKeyShort.DELETE);
```

**FlaUI keyboard method comparison:**

| Method | Sends | Key state after |
|--------|-------|-----------------|
| `Keyboard.Press(key)` | key-down only | Key stays **pressed** |
| `Keyboard.Release(key)` | key-up only | Key released |
| `Keyboard.Type(key)` | key-down + key-up | Key released |
| `Keyboard.TypeSimultaneously(keys)` | all down, then all up (reverse order) | All released |

**Rule:** Always use `Keyboard.Type()` for single keystrokes. Reserve `Keyboard.Press()` + `Keyboard.Release()` only for held-key scenarios (e.g., Ctrl+Click).

## Problem 6: Canvas Coordinates Must Use UIA BoundingRectangle, Not Designer Bounds

When calculating positions on a NodifyEditor canvas for drag selection, using the editor control's screen bounds (`designer.BoundingRectangle`) as the origin is incorrect. NodifyEditor uses a virtual canvas with viewport offset — the first card at grid position (0,0) may not align with the editor's top-left screen coordinate.

**Symptoms:**
- Drag selection rectangle starts at the wrong position
- Selection rectangle covers the intended area visually but selects nothing
- The same test works when cards happen to be at the viewport origin

**Fix — calculate positions from actual node UIA bounding rectangles:**

```csharp
// BAD — assumes viewport origin equals editor top-left
var designerBounds = designer.BoundingRectangle;
var selectionStart = new Point(
    designerBounds.Left + CellWidth + 50,
    designerBounds.Top + CellPitchY * 2);

// GOOD — uses actual node screen positions (viewport-independent)
var nodes = FindNodesByNameSortedByPosition(designer, "InputMethod2DFile");
var lastNodeBounds = nodes[^1].BoundingRectangle;
var firstNodeBounds = nodes[0].BoundingRectangle;

// Start: below-right of last node (guaranteed empty canvas)
var selectionStart = new Point(
    lastNodeBounds.Right + 30,
    lastNodeBounds.Bottom + 30);

// End: center of first node
var selectionEnd = new Point(
    firstNodeBounds.Left + firstNodeBounds.Width / 2,
    firstNodeBounds.Top + firstNodeBounds.Height / 2);
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
| `ReleaseAllKeys` causes pan/drag side effect | Use `ReleaseModifierKeys()` (Ctrl/Shift/Alt only) to avoid non-modifier key-up noise |
| Key stays pressed after `Keyboard.Press()` | Use `Keyboard.Type()` (down+up) instead of `Keyboard.Press()` (down only) |
| Canvas drag selection at wrong position | Calculate coordinates from node `BoundingRectangle`, not editor control bounds |
| `ReleaseAllKeys` before gesture, need all keys cleared | `ReleaseAllKeys()` + `Thread.Sleep(500)` — longer delay for non-modifier key states to settle |

## Diagnostic Checklist

When a FlaUI mouse gesture fails silently on a WPF control, check these in order:

1. **Keyboard state** — Log `Keyboard.IsKeyDown()` in `PreviewMouseLeftButtonDown`. Any stuck key blocks gesture matching.
2. **`Mouse.Captured`** — If another element holds capture, `CaptureMouse()` silently fails.
3. **`ClickCount`** — Must be 1 for single-click gestures. Cross-process timing can produce 0 or 2.
4. **Hit test target** — `VisualTreeHelper.HitTest()` in `PreviewMouseDown` to verify the correct element is under the cursor.
5. **Correct binary** — When using git worktrees or separate build outputs, verify the test launches the exe that contains your changes.
6. **Press vs Type** — Audit all `Keyboard.Press()` calls. Each one leaves a key pressed. Use `Keyboard.Type()` for single keystrokes.
7. **Canvas coordinates** — Never assume viewport origin equals editor control top-left. Use node `BoundingRectangle` from UIA as position anchors.
8. **ReleaseAllKeys delay** — 200ms is enough after `ReleaseModifierKeys()`. `ReleaseAllKeys()` needs 500ms because non-modifier key-up events take longer to settle across processes.
