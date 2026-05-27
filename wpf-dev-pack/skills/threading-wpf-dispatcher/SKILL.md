---
description: Explains WPF Dispatcher priority system and threading patterns. Use when implementing background operations, maintaining UI responsiveness, or understanding task scheduling order.
user-invocable: false
---

# WPF Dispatcher Threading

## Priority Levels (High to Low)

| Priority | Value | Use Case |
|----------|-------|----------|
| Send | 10 | Synchronous (avoid - deadlock risk) |
| Normal | 9 | Standard operations |
| DataBind | 8 | Binding updates |
| Render | 7 | Rendering operations |
| Loaded | 6 | Loaded event handlers |
| Input | 5 | User input processing |
| Background | 4 | Background tasks (UI stays responsive) |
| ContextIdle | 3 | After context operations |
| ApplicationIdle | 2 | App idle (cache cleanup) |
| SystemIdle | 1 | System idle |
| Inactive | 0 | Disabled |

## Common Patterns

### Background Work (UI Responsive)
```csharp
await Dispatcher.InvokeAsync(() =>
{
    // This runs between input processing
    ProcessNextChunk();
}, DispatcherPriority.Background);
```

### After Render Complete
```csharp
await Dispatcher.InvokeAsync(() =>
{
    // Runs after layout/render
    ScrollIntoView(lastItem);
}, DispatcherPriority.Loaded);
```

### Idle Cleanup
```csharp
Dispatcher.InvokeAsync(() =>
{
    // Low priority cleanup
    ClearUnusedCache();
}, DispatcherPriority.ApplicationIdle);
```

## Yielding Pattern

Keep UI responsive during long operations:
```csharp
public async Task ProcessLargeDataAsync(IList<Item> items)
{
    for (int i = 0; i < items.Count; i++)
    {
        Process(items[i]);

        // Yield every 100 items to process pending input
        if (i % 100 == 0)
        {
            await Dispatcher.Yield(DispatcherPriority.Background);
            UpdateProgress(i, items.Count);
        }
    }
}
```

## Threading Rules
```csharp
// Check if on UI thread
if (!Dispatcher.CheckAccess())
{
    Dispatcher.Invoke(() => UpdateUI());
    return;
}

// From background thread
await Task.Run(() =>
{
    var result = HeavyComputation();

    // Marshal back to UI
    Dispatcher.Invoke(() => DisplayResult(result));
});
```

## Avoid
```csharp
// ❌ Send priority - can cause deadlock
Dispatcher.Invoke(() => {}, DispatcherPriority.Send);

// ❌ Blocking UI thread
Thread.Sleep(1000);

// ✅ Use async/await instead
await Task.Delay(1000);
```

## Related Skills

| Skill | Relationship |
|---|---|
| [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/SKILL.md) | Sync-over-async deadlocks during normal operation |
| [`shutting-down-wpf-gracefully`](../shutting-down-wpf-gracefully/SKILL.md) | Cross-thread `Invoke` racing main-Dispatcher shutdown; guard with `HasShutdownStarted` |
| [`implementing-wpf-splash-screen`](../implementing-wpf-splash-screen/SKILL.md) | Hosting a `Window` on a dedicated STA thread with `Dispatcher.Run()` |