# Window Management

## Window Creation

```csharp
var window = new Window()
    .Title("Main Window")
    .Width(800).Height(600)
    .MinWidth(400).MinHeight(300)
    .Content(mainContent);

window.Show();
```

Note: `WindowStartupLocation` and `WindowState` do not exist in MewUI.

## Window Sizing Helpers

```csharp
// Fixed size
window.Fixed(800, 600);

// Resizable with initial size
window.Resizable(800, 600);

// Fit to content
window.FitContentWidth(maxWidth: 800, fixedHeight: 600);
window.FitContentHeight(fixedWidth: 800, maxHeight: 600);
window.FitContentSize(maxWidth: 800, maxHeight: 600);
```

## Window Events

Events use `Action` delegates, not `EventHandler` pattern:

```csharp
// Note: No (sender, args) - just Action delegates
window.Loaded += () => { /* Window visible and rendered */ };
window.Closed += () => { /* Cleanup after close */ };
window.Activated += () => { /* Window gained focus */ };
window.Deactivated += () => { /* Window lost focus */ };

// Size changed (note: ClientSizeChanged, not SizeChanged)
window.ClientSizeChanged += (Size newSize) => { /* Resize handling */ };

// DPI changed
window.DpiChanged += (uint newDpi) => { /* DPI change handling */ };

// Theme changed
window.ThemeChanged += () => { /* Theme change handling */ };

// Frame rendering
window.FirstFrameRendered += () => { /* First frame done */ };
window.FrameRendered += () => { /* Each frame done */ };

// Preview keyboard events (before child handling)
window.PreviewKeyDown += (KeyEventArgs e) => { };
window.PreviewKeyUp += (KeyEventArgs e) => { };
window.PreviewTextInput += (TextInputEventArgs e) => { };
```

Note: There is NO `Closing` event that allows cancellation. Only `Closed` exists.

## Multiple Windows

```csharp
// Open secondary window
var secondWindow = new Window().Title("Details").Content(...);
secondWindow.Show();

// Note: ShowDialog() does NOT exist in MewUI
// Windows are always non-modal
```

## Application Shutdown

```csharp
// Quit the application (static method)
Application.Quit();

// Note: No exit code parameter, no ShutdownMode
```

## Dispatcher (UI Thread)

Access via `Application.Current.Dispatcher`:

```csharp
// Post to UI thread (async)
Application.Current.Dispatcher?.Post(() => RefreshList());

// Post with priority
Application.Current.Dispatcher?.Post(() => DoWork(), UiDispatcherPriority.Normal);

// Send (synchronous)
Application.Current.Dispatcher?.Send(() => UpdateDirectly());

// Schedule for later
Application.Current.Dispatcher?.Schedule(TimeSpan.FromSeconds(1), () => DelayedAction());

// Check thread (property, not method)
if (Application.Current.Dispatcher?.IsOnUIThread == true)
    UpdateDirectly();
else
    Application.Current.Dispatcher?.Post(UpdateDirectly);
```

Note: No `InvokeAsync()` or `CheckAccess()` methods. Use `Post`/`Send` and `IsOnUIThread`.

## DPI Handling

```csharp
// Access DPI
uint dpi = window.Dpi;           // Raw DPI value (96 = 100%)
double scale = window.DpiScale;  // Scale factor (1.0 = 96 DPI, 1.5 = 144 DPI)

// React to DPI changes (note: uint parameters, not double)
protected override void OnDpiChanged(uint oldDpi, uint newDpi)
{
    base.OnDpiChanged(oldDpi, newDpi);
    _cachedResources = null;
    InvalidateMeasure();
}
```

## Window Properties

| Property | Type | Description |
|----------|------|-------------|
| `Title` | `string` | Window title |
| `Handle` | `nint` | Native window handle |
| `IsActive` | `bool` | Window has focus |
| `Dpi` | `uint` | Raw DPI value |
| `DpiScale` | `double` | DPI scale factor |
| `Icon` | `IconSource?` | Window icon |
| `UseLayoutRounding` | `bool` | Enable pixel snapping |

## Mouse Capture

```csharp
// Capture mouse to element (call on Window)
window.CaptureMouse(myElement);

// Release capture
window.ReleaseMouseCapture();
```

## Layout and Rendering

```csharp
// Force layout update
window.PerformLayout();

// Force visual refresh
window.Invalidate();

// Request command state refresh
window.RequerySuggested();
```
