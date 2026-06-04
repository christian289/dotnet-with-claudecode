# WPF Memory Optimization

> Covers WPF memory optimization including Freezable patterns, common memory leak causes, and diagnostic techniques. Use when experiencing memory growth, implementing resource caching, or debugging memory issues.

## 1. Freezable Pattern

### Why Freeze?

| Benefit | Description |
|---------|-------------|
| Thread-safe | Can be used across threads |
| No change tracking | Reduces overhead |
| Renderer optimization | Better GPU utilization |

### Basic Usage
```csharp
// Always freeze static resources
var brush = new SolidColorBrush(Colors.Red);
brush.Freeze();

var pen = new Pen(Brushes.Black, 1);
pen.Freeze();
```

### XAML Freeze
```xml
<Window xmlns:po="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options"
        mc:Ignorable="po">
    <Window.Resources>
        <SolidColorBrush x:Key="FrozenBrush" Color="Red" po:Freeze="True"/>
    </Window.Resources>
</Window>
```

### When Freeze Fails
```csharp
if (brush.CanFreeze)
    brush.Freeze();
else
    // Has bindings or animations - cannot freeze
```

### Modifying Frozen Objects
```csharp
var clone = frozenBrush.Clone(); // Creates unfrozen copy
clone.Color = Colors.Blue;
clone.Freeze(); // Freeze again if needed
```

## 2. Common Memory Leaks

### Event Handler Leaks
```csharp
// ❌ LEAK: Static event holds reference
SomeStaticClass.StaticEvent += OnEvent;

// ✅ FIX: Unsubscribe in Unloaded
Unloaded += (s, e) => SomeStaticClass.StaticEvent -= OnEvent;
```

### CompositionTarget.Rendering Leak
```csharp
// ❌ LEAK: Never unsubscribed
CompositionTarget.Rendering += OnRendering;

// ✅ FIX: Always unsubscribe
Loaded += (s, e) => CompositionTarget.Rendering += OnRendering;
Unloaded += (s, e) => CompositionTarget.Rendering -= OnRendering;
```

### Binding Without INotifyPropertyChanged
```csharp
// ❌ LEAK: PropertyDescriptor retained
public string Name { get; set; } // No INPC

// ✅ FIX: Implement INPC
public string Name
{
    get => _name;
    set { _name = value; OnPropertyChanged(); }
}
```

### DispatcherTimer Leak
```csharp
// ❌ LEAK: Timer keeps running
_timer = new DispatcherTimer();
_timer.Tick += OnTick;
_timer.Start();

// ✅ FIX: Stop and cleanup
Unloaded += (s, e) =>
{
    _timer.Stop();
    _timer.Tick -= OnTick;
};
```

### Image Resource Leak
```csharp
// ❌ LEAK: Stream kept open
var bitmap = new BitmapImage(new Uri(path));

// ✅ FIX: Load immediately and release stream
var bitmap = new BitmapImage();
bitmap.BeginInit();
bitmap.CacheOption = BitmapCacheOption.OnLoad;
bitmap.UriSource = new Uri(path);
bitmap.EndInit();
bitmap.Freeze();
```

## 3. Diagnostic Checklist

### Code Review Points

- [ ] All static event subscriptions have unsubscribe logic
- [ ] CompositionTarget.Rendering unsubscribed in Unloaded
- [ ] DispatcherTimer stopped in Unloaded
- [ ] ViewModels implement INotifyPropertyChanged
- [ ] Images use `BitmapCacheOption.OnLoad`
- [ ] Static resources are Frozen

### Diagnostic Tools

| Tool | Purpose |
|------|---------|
| VS Diagnostic Tools | Real-time memory snapshots |
| dotMemory | Detailed retention paths |
| PerfView | GC and allocation analysis |

### Memory Monitor
```csharp
public static class MemoryMonitor
{
    public static void LogMemory(string context)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var mem = GC.GetTotalMemory(true) / 1024.0 / 1024.0;
        Debug.WriteLine($"[{context}] Memory: {mem:F2} MB");
    }
}
```

### Native Resources Already Copied During Conversion

When a native or large unmanaged resource is **converted by copying** into
a different representation (e.g. `OpenCvSharp.Mat` → `BitmapSource` via
pixel copy, frozen for thread-safe sharing), do NOT keep the source
alive for the control's lifetime. The copy already owns the pixels.

```csharp
// ❌ Anti-pattern: hold Mat as a field, dispose in Unloaded.
// Wastes memory because the BitmapSource already owns the pixels.
private Mat? _rampMat;

private void RebuildColormapBrush()
{
    _rampMat?.Dispose();
    _rampMat = BuildColormapMat(_colormap);
    var source = _rampMat.ToBitmapSource();
    source.Freeze();
    ColormapBrush = new ImageBrush(source);
}

// In ctor:
Unloaded += (_, _) => _rampMat?.Dispose();
```

```csharp
// ✅ Correct: convert and dispose inside one local using.
// No field, no Unloaded subscription to manage.
private void RebuildColormapBrush()
{
    using var mat = BuildColormapMat(_colormap);   // owned locally
    var source = mat.ToBitmapSource();             // pixels are copied
    source.Freeze();                               // safe to share / cross threads
    ColormapBrush = new ImageBrush(source);
}                                                   // mat disposed here
```

**Rule of thumb:** if the converted representation owns its own copy and
can be frozen, the native source's lifetime should be the converting
method's lifetime — not the control's. The "field + Unloaded.Dispose"
pattern is correct only when the source is **read repeatedly** between
events; for one-shot conversions it is a memory leak waiting to happen.

## 4. Resource Factory Pattern
```csharp
public static class FrozenResources
{
    public static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public static Pen CreatePen(Color color, double thickness)
    {
        var pen = new Pen(CreateBrush(color), thickness);
        pen.Freeze();
        return pen;
    }
}
```
