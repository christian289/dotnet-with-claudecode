# Setting Up FlaUI UI Tests for WPF (.NET project configuration + streaming caveats)

> Configures an xUnit + FlaUI UI-automation test project for a modern .NET WPF app and works around the failure modes hit on first run. Use when creating a FlaUI test project, when `UIA3Automation` throws `FileNotFoundException` for the `Accessibility` assembly, when `Path`/`HttpClient` suddenly fail to resolve after adding `UseWPF`, when keyboard `TextBox.Enter` flakes or `SendInput` is blocked, when UI tests interfere with each other under xUnit parallelization, or when elements that ARE visually present never materialize in the UIA tree while the app is streaming/animating. Covers the `netX.0-windows` + `UseWPF` requirement, implicit-using removals, `CollectionBehavior(DisableTestParallelization = true)`, ValuePattern input instead of keyboard input, `TextPattern.GetText(-1)`, and the wait-for-the-burst-to-settle rule for streaming UIs.

This topic covers test-project *setup and run-loop robustness*. Element discovery itself (AutomationId placement, `FindAllDescendants` depth, Shape peers, common dialogs) is covered by `flaui-wpf-element-discovery`.

---

## 1. Test Project Configuration

FlaUI's UIA3 wrapper needs the **WindowsDesktop shared framework** at runtime
(`Accessibility.dll` for `LegacyIAccessiblePattern`). A plain `net10.0` xUnit
project launches fine and then throws on `new UIA3Automation()`:

```
System.IO.FileNotFoundException: Could not load file or assembly 'Accessibility, Version=4.0.0.0'
```

Fix — target the Windows TFM and reference the desktop framework:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>   <!-- pulls Microsoft.WindowsDesktop.App incl. Accessibility.dll -->
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="FlaUI.UIA3" Version="5.*" />
</ItemGroup>
```

Two side effects to expect:

- **`UseWPF` removes the `System.IO` and `System.Net.Http` implicit usings**
  (they collide with `System.Windows.Shapes.Path` etc.). A previously-compiling
  `Path.Combine(...)` in the test file becomes CS0103 — add `using System.IO;`
  per file.
- FlaUI 5.0 restores its net4x assets under NU1701 ("not fully compatible").
  The warning is benign on Windows; UIA3 works at runtime.

## 2. Disable xUnit Parallelization for UI Automation

Each test class is its own xUnit collection and collections run **in
parallel** — two UI test classes therefore launch two app instances whose
windows, focus, and UIA traffic interfere, producing flaky failures that pass
in isolation. UI automation must be serial:

```csharp
// AssemblyInfo.cs in the UI test project
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

## 3. Prefer UIA Patterns over Synthesized Input

`textBox.Enter("...")` and `Keyboard.Type` synthesize real keystrokes
(`SendInput`): they require the window to be foreground/focused, can be blocked
by the environment (RDP, sandboxes, CI agents), and can throw from
`PressVirtualKeyCode`. The patterns API has none of those constraints:

```csharp
input.Text = "Hello";   // FlaUI TextBox.Text setter = UIA ValuePattern.SetValue
send.Invoke();          // InvokePattern instead of a synthesized click
```

A WPF `TwoWay` binding with `UpdateSourceTrigger=PropertyChanged` still fires —
ValuePattern goes through the normal `TextBox.Text` property path. Gate the
click on the command actually becoming executable, and assert it:

```csharp
var enabled = Retry.WhileFalse(() => send.IsEnabled, timeout: TimeSpan.FromSeconds(5));
Assert.True(enabled.Result, "Send should enable once the binding pushed the text.");
send.Invoke(); // invoking a disabled element throws ElementNotEnabledException (COM 0x80040200)
```

Reserve synthesized keyboard/mouse for what patterns cannot express (keyboard
*gestures* like Enter-to-send / Shift+Enter, drag-and-drop) — see
`flaui-cross-process-input`.

## 4. Streaming UIs: Let the Burst Settle Before Polling

The hard-won rule: **continuously polling UIA while the app is processing a
streaming burst (tokens appending, items materializing, debounced re-renders)
can keep the new elements from ever appearing in the UIA tree** — the same
window dumped after a quiet `Thread.Sleep` shows them all. Symptoms: item
containers exist as `DataItem`s but their rendered content (e.g. a
`RichTextBox` bubble) is missing for the entire polling window, even though the
UI is visibly fine.

```csharp
send.Invoke();
Thread.Sleep(3000);                  // let the stream + debounced render finish quietly
var found = Retry.WhileFalse(() =>   // only now start polling
{
    // Structural witness FIRST (cheap, robust)...
    bool copySeen = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
        .Any(b => b.Name == "Copy");
    if (copySeen) { return true; }
    // ...then the text witness, isolated so one flaky read cannot
    // mask the structural check on the next iteration.
    try
    {
        return window.FindAllDescendants(cf => cf.ByControlType(ControlType.Document))
            .Any(d => d.Patterns.Text.PatternOrDefault?.DocumentRange.GetText(-1)
                .Contains("expected text") == true);
    }
    catch { return false; }
}, timeout: TimeSpan.FromSeconds(20), interval: TimeSpan.FromMilliseconds(250), ignoreException: true);
```

Rules distilled:

1. **Wait quietly through the burst, poll after.** A fixed settle delay sized
   to the stream (or an app-side "idle" signal) before the first UIA query.
2. **`GetText(-1)`** for "unlimited" in UIA `TextPattern` — some providers
   reject `int.MaxValue`.
3. **Assert on a structural witness** (a known child control appearing) in
   addition to text — text reads are the flakier half.
4. **Isolate flaky sub-checks** inside the retry lambda with their own
   `try/catch`; one throwing check otherwise hides every later check on that
   iteration, and `ignoreException: true` silently retries forever.
5. App-side defense: give debounced render timers an explicit
   `DispatcherPriority.Normal` (the parameterless `DispatcherTimer` runs at
   `Background`, the first priority starved under automation/load). This alone
   did not eliminate the materialization stall in testing — keep rule 1.

## 5. Launch/Teardown Skeleton

```csharp
public sealed class ChatSmokeTests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation = new();

    public ChatSmokeTests()
    {
        string exe = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src", "MyApp.WpfApp", "bin", "Debug",
            "net10.0-windows", "MyApp.WpfApp.exe"));
        _app = Application.Launch(exe);
    }

    // tests: _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10)) ...

    public void Dispose()
    {
        _app.Close();
        _automation.Dispose();
        _app.Dispose();
    }
}
```

Give every interactable control an `AutomationProperties.AutomationId` in XAML
up front — discovery by id is the only naming that survives localization and
restyling (`flaui-wpf-element-discovery` covers where the id must go for
`ItemsControl` content).

### Related topics

- [`flaui-wpf-element-discovery`](../flaui-wpf-element-discovery/TOPIC.md) — finding the elements once the project runs (AutomationId placement, descendant-depth limits, Shape peers, common dialogs).
- [`flaui-cross-process-input`](../flaui-cross-process-input/TOPIC.md) — when synthesized input IS required (drag, gestures): SendInput pitfalls, stuck keys, DPI.
- [`flaui-capture-resize-robustness`](../flaui-capture-resize-robustness/TOPIC.md) — robust screenshots and coordinates under window resize.
- [`hosting-extensions-ai-chatclient-in-wpf-mvvm`](../hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md) — the streaming chat surface whose token bursts motivate §4.
