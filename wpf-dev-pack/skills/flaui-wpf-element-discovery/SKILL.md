---
description: "Solves FlaUI element discovery failures in WPF UI automation. Use when FindAllDescendants returns empty or missing elements, AutomationId is set but not visible in UIA tree, ByAutomationId finds nothing, connector/node elements can't be located, Shape-based controls (Connection, Path) don't appear in automation tree, or native Common Dialog (OpenFileDialog/SaveFileDialog) can't be found via a single search path. Covers AutomationId placement on ItemContainer vs content elements, FindAllDescendants depth limitations, Shape/AutomationPeer visibility rules, and 3-tier fallback search for Microsoft.Win32 common dialogs with ProcessId filtering."
user-invocable: false
model: sonnet
---

# FlaUI WPF Element Discovery

When automating WPF applications with FlaUI, elements you expect to find may be missing from the UIA automation tree. This skill covers the most common causes and their fixes, based on how WPF exposes elements to UI Automation.

## Problem 1: AutomationId Set But Not Visible in UIA

Setting `AutomationProperties.AutomationId` on a content element inside an `ItemsControl` may not expose it in the UIA tree. The UIA tree reflects the **items container**, not the content template.

**Symptoms:**
- `designer.FindAllDescendants(cf => cf.ByAutomationId("MyNode"))` returns empty
- The AutomationId is correctly set in XAML via `AutomationProperties.AutomationId="{Binding Name}"`
- The element is visually present and interactive

**Root cause:** In WPF's ItemsControl pattern (e.g., Nodify's `NodifyEditor`), items are wrapped in containers (`ItemContainer`, `ListBoxItem`, `TreeViewItem`). The UIA tree exposes these containers, not the content templates inside them. Setting AutomationId on a `DataTemplate` child (e.g., `nodify:Node`) places it below the container level where UIA may not propagate it.

**Fix — set AutomationId on the ItemContainerStyle, not the DataTemplate:**

```xml
<!-- WRONG: AutomationId on content element inside DataTemplate -->
<DataTemplate DataType="{x:Type local:MyItem}">
    <nodify:Node AutomationProperties.AutomationId="{Binding Name}" .../>
</DataTemplate>

<!-- RIGHT: AutomationId on the items container via Style -->
<Style x:Key="NodeStyle" TargetType="{x:Type nodify:ItemContainer}">
    <Setter Property="AutomationProperties.AutomationId" Value="{Binding Name}"/>
    <Setter Property="Location" Value="{Binding PixelPosition, Mode=TwoWay}"/>
</Style>
```

This applies to any `ItemsControl`-based pattern: `ListView`, `ListBox`, `TreeView`, `NodifyEditor`, etc.

## Problem 2: FindAllDescendants Missing Deeply Nested Children

`node.FindAllDescendants()` may not return all elements in the visual tree. TextBlocks or other elements inside nested `ItemsControl` containers can be omitted.

**Symptoms:**
- A node element is found, but searching its descendants for a specific child returns null
- The child element IS visible when searching from the designer (root) level
- The child's parent `ItemsControlItem` IS found as a descendant, but the child itself is not

**Example:** Nodify output connector structure:
```
ItemContainer (node)                  ← FindAllDescendants() starts here
  └── ItemsControl (PART_Output)     ← found
       └── ItemsControlItem          ← found
            └── TextBlock "ZMap3D"   ← NOT found in node.FindAllDescendants()
```

**Fix — search children of the specific container directly:**

```csharp
// FindAllDescendants misses deeply nested TextBlocks
AutomationElement? socketTextBlock = null;

// First try: search all descendants (works for shallow elements)
foreach (var desc in node.FindAllDescendants())
{
    if (desc.Name?.StartsWith(socketName + "\n") == true)
    {
        socketTextBlock = desc;
        break;
    }
}

// If not found: search children of each ItemsControlItem directly
if (socketTextBlock is null)
{
    foreach (var desc in node.FindAllDescendants())
    {
        if (desc.Properties.ClassName.ValueOrDefault != "ItemsControlItem")
        {
            continue;
        }

        // FindAllChildren goes one level deep — catches elements
        // that FindAllDescendants missed
        var children = desc.FindAllChildren();
        foreach (var child in children)
        {
            if (child.Name?.StartsWith(socketName + "\n") == true)
            {
                socketTextBlock = child;
                break;
            }
        }

        if (socketTextBlock is not null)
        {
            break;
        }
    }
}
```

## Problem 3: Shape-Based Controls Not Visible in UIA Tree

WPF controls inheriting from `Shape` (`Path`, `Ellipse`, `Rectangle`, `Line`, and custom shapes) do not provide an `AutomationPeer` by default. They are invisible to FlaUI's `FindAllDescendants()`.

**Affected controls:**
- Nodify's `BaseConnection` / `MidpointConnection` (extends `Shape`)
- Custom drawing elements extending `Shape`
- `Path` elements used for connection lines

**Symptoms:**
- After creating connections between nodes, no Connection elements appear in `FindAllDescendants()`
- `ClassName.Contains("Connection")` or `ClassName.Contains("Path")` finds nothing
- The connections are visually present and interactive via mouse

**Impact on testing:**
- Connection existence cannot be verified through UIA element inspection
- `FindAllDescendants().Length` may **decrease** after connecting (connector structure changes)
- Do not rely on element count comparison for connection verification

**Workarounds:**

1. **Accept that connections can't be verified via UIA.** Document this limitation and verify connections indirectly (e.g., the drag operation completed without error).

2. **Add a custom AutomationPeer** to the connection control if you control the source code:
```csharp
public class MidpointConnection : BaseConnection
{
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new FrameworkElementAutomationPeer(this);
    }
}
```

3. **Expose connection count as an AutomationProperty** on a parent element that IS visible in UIA.

## Quick Reference

| Situation | Solution |
|-----------|----------|
| `ByAutomationId` finds nothing | Set AutomationId on the `ItemContainerStyle`, not on DataTemplate content |
| TextBlock missing from `FindAllDescendants` | Use `FindAllChildren()` on the parent `ItemsControlItem` directly |
| Connection/Path not in UIA tree | `Shape`-based controls have no `AutomationPeer` — invisible to UIA |
| Element count decreases after action | Connector structure can change on connection — don't rely on count comparison |
| Need to distinguish input/output connectors | Filter `ItemsControlItem` by position (left half = input, right half = output) |

## Diagnostic: Dump UIA Tree

When elements aren't found, dump the full UIA tree to see what's actually exposed:

```csharp
var allDescs = designer.FindAllDescendants();
Console.WriteLine($"Total elements: {allDescs.Length}");
foreach (var desc in allDescs)
{
    var cls = desc.Properties.ClassName.ValueOrDefault ?? "(null)";
    var name = desc.Name ?? "(null)";
    var aid = desc.Properties.AutomationId.ValueOrDefault ?? "";
    var b = desc.BoundingRectangle;
    Console.WriteLine(
        $"  [{cls}] Name=\"{name}\" AutomationId=\"{aid}\" " +
        $"Bounds=({b.Left},{b.Top},{b.Width}x{b.Height})");
}
```

Check for:
- **AutomationId** — is it on the expected element or its container?
- **ClassName** — `ItemsControlItem` vs the control type you expect
- **Missing elements** — compare with what you see visually

## Problem 4: Native Common Dialogs (OpenFileDialog / SaveFileDialog) Fail Single-Path Search

`Microsoft.Win32.OpenFileDialog` and `SaveFileDialog` use the Vista+ `IFileDialog` COM (Windows Common File Dialog). Their UIA tree placement is **inconsistent** — sometimes they appear as a modal child of the owner window, sometimes as a top-level window of the host process, and sometimes under the desktop root without a clear process affinity. A single-path lookup (e.g., only `Application.GetAllTopLevelWindows`) often misses them even when the dialog is clearly visible to the user.

**Symptoms:**
- The file dialog opens visually but `Application.GetAllTopLevelWindows(...)` returns nothing matching
- `_mainWindow.ModalWindows` is empty while the dialog is showing
- Substring title match (`title.Contains("Save")`) accidentally returns the Visual Studio / IDE window (e.g. `"SomeTest.cs - Microsoft Visual Studio"`) because the IDE happens to have "Save" in its title

**Fix — 3-tier fallback search with PID filter and exact title match:**

```csharp
private AutomationElement? WaitForFileDialog(string exactTitle, TimeSpan? timeout = null)
{
    var hostProcessId = _fixture.Application.ProcessId;

    return RetryHelper.WaitForElement(
        () =>
        {
            // 1) Modal children of the main window — most common placement
            foreach (var modal in _mainWindow.ModalWindows)
            {
                if (string.Equals(modal.Title, exactTitle, StringComparison.Ordinal))
                    return modal;
            }

            // 2) Top-level windows of the automated process
            foreach (var win in _fixture.Application.GetAllTopLevelWindows(_fixture.Automation))
            {
                if (string.Equals(win.Title, exactTitle, StringComparison.Ordinal))
                    return win;
            }

            // 3) UIA desktop root — native common dialogs sometimes land here.
            //    Filter by ProcessId to avoid matching other processes (e.g., Visual Studio).
            var desktop = _fixture.Automation.GetDesktop();
            var rootWindows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));
            foreach (var win in rootWindows)
            {
                if (win.Properties.ProcessId.ValueOrDefault != hostProcessId)
                    continue;
                if (string.Equals(win.Properties.Name.ValueOrDefault, exactTitle, StringComparison.Ordinal))
                    return win;
            }

            return null;
        },
        timeout ?? TimeSpan.FromSeconds(5));
}
```

**Rules:**
- Set a specific `Title` on the dialog from your app code (e.g., `fileDialog.Title = "Save Task Config";`) and match that exact string — don't rely on Windows-provided default titles which vary by locale.
- Avoid `Contains()` — it false-matches the IDE window that hosts the test runner (VS, Rider).
- Use all 3 tiers; any single one is insufficient depending on how the COM dialog attaches to its owner.
- Always filter tier 3 by `ProcessId` to stay within the automated app's process.
