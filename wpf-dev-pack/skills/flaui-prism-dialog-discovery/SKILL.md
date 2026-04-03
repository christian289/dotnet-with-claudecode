---
description: "Discovers and interacts with Prism IDialogAware modal windows in FlaUI UI automation. Use when testing Prism dialogs that use TitlelessWindow style, CommonWindow with empty Title, modal Progress dialogs without title bar, or when ModalWindows search returns unexpected results. Covers child-control-based dialog identification, IsCancel button ESC key verification, and Status Bar message assertion patterns."
user-invocable: false
model: sonnet
---

# FlaUI Prism Dialog Discovery

Prism's `IDialogAware` dialogs in WPF-UI applications often use custom window styles (`TitlelessWindow`, `CommonWindow`) that make standard FlaUI modal discovery unreliable. This skill covers patterns for finding and verifying these dialogs.

## Problem: TitlelessWindow Modals Can't Be Found by Title

Prism dialogs using `TitlelessWindow` style have no `Title` property. Standard modal search by title returns nothing.

**Symptoms:**
- `_mainWindow.ModalWindows` returns windows but `modal.Title` is empty
- Dialog is visually present but can't be distinguished from other titleless modals
- Multiple titleless modals may exist simultaneously

**Fix — identify by child control type:**

```csharp
/// <summary>
/// Finds a Progress dialog by looking for a modal with a ProgressBar control.
/// TitlelessWindow style means Title is empty, so we identify by content.
/// </summary>
private AutomationElement? FindProgressDialog()
{
    var modalWindows = _mainWindow.ModalWindows;
    foreach (var modal in modalWindows)
    {
        var progressBar = modal.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.ProgressBar));

        if (progressBar is not null)
        {
            return modal;
        }
    }

    return null;
}
```

**For titled dialogs (CommonWindow with Title):**

```csharp
/// <summary>
/// Finds a modal dialog with a non-empty Title (e.g., warning, confirmation).
/// </summary>
private AutomationElement? FindTitledModal()
{
    var modalWindows = _mainWindow.ModalWindows;
    foreach (var modal in modalWindows)
    {
        if (!string.IsNullOrEmpty(modal.Title))
        {
            return modal;
        }
    }

    return null;
}
```

**General pattern — identify modal by unique child:**

```csharp
private AutomationElement? FindModalByChild(
    Func<AutomationElement, bool> childMatcher)
{
    foreach (var modal in _mainWindow.ModalWindows)
    {
        var descendants = modal.FindAllDescendants();
        if (descendants.Any(childMatcher))
        {
            return modal;
        }
    }

    return null;
}

// Usage: find modal containing a specific button text
var dialog = FindModalByChild(d =>
    d.Name?.Contains("Cancel", StringComparison.OrdinalIgnoreCase) == true);
```

## Problem: Verifying ESC Key Cancellation

WPF buttons with `IsCancel="True"` close the dialog on ESC press. In Prism dialogs, this triggers `IDialogAware.OnDialogClosed()`. Testing this requires verifying both dialog closure and the application's response.

**Pattern — ESC cancel with Status Bar verification:**

```csharp
// 1. Wait for dialog to appear
var dialog = RetryHelper.WaitForElement(
    () => FindProgressDialog(),
    TimeSpan.FromSeconds(10));
Assert.NotNull(dialog);

// 2. Press ESC (triggers IsCancel="True" button → CancelCommand)
Thread.Sleep(500); // Allow dialog to fully render
Keyboard.Press(VirtualKeyShort.ESCAPE);

// 3. Verify dialog closed
var closed = RetryHelper.WaitUntil(
    () => FindProgressDialog() is null,
    TimeSpan.FromSeconds(10));
Assert.True(closed, "ESC should close the dialog");

// 4. Verify cancellation via Status Bar message
// This distinguishes cancel from normal completion
var statusVerified = RetryHelper.WaitUntil(
    () =>
    {
        var texts = _mainWindow.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.Text));
        return texts.Any(t =>
            t.Name?.Contains("canceled", StringComparison.OrdinalIgnoreCase) == true);
    },
    TimeSpan.FromSeconds(5));
Assert.True(statusVerified, "Status Bar should show 'canceled' message");
```

**Why Status Bar verification matters:**
- Dialog closes on Success, Failure, AND Cancel — closure alone doesn't prove cancellation
- ProgressBar value < 100% is unreliable (fast tasks may complete before ESC)
- Status Bar message is set by `VisionTaskCompleted(TaskEventReason.Canceled)` event — unique to cancellation

## Problem: File Open Dialog Interaction

Loading test data via Ctrl+O requires interacting with the Windows file dialog, which is a separate process.

**Pattern:**

```csharp
// Ctrl+O to open file dialog
DragHelper.ForceSetForeground(_mainWindow);
Thread.Sleep(300);
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_O);
Thread.Sleep(2000); // File dialog takes time to open

// Type file path and confirm
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
Thread.Sleep(100);
Keyboard.Type(taskConfigPath);
Thread.Sleep(500);
Keyboard.Type(VirtualKeyShort.ENTER);
Thread.Sleep(3000); // Loading takes time

// Handle potential warning dialog (e.g., path validation)
var warningDialog = FindTitledModal();
if (warningDialog is not null)
{
    Keyboard.Type(VirtualKeyShort.ENTER); // Dismiss warning
    Thread.Sleep(500);
}
```

## Prism Dialog Window Styles Reference

| Style | Title | How to Find |
|-------|-------|-------------|
| `CommonWindow` (default) | Set via `prism:Dialog.WindowStyle` Setter | `modal.Title` |
| `TitlelessWindow` | Empty | Child control type (ProgressBar, specific Button, etc.) |
| `CommonWindow` with no Title Setter | Empty | Same as TitlelessWindow |

## Key Timing Considerations

- `Thread.Sleep(2000)` after Ctrl+O — Windows file dialog is cross-process, needs extra time
- `Thread.Sleep(3000)` after ENTER in file dialog — task config loading + FunctionBlock instantiation
- `Thread.Sleep(500)` before ESC — allow dialog to fully render and receive keyboard focus
- Use `RetryHelper.WaitUntil` for state verification instead of fixed sleep
