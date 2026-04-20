---
description: Prevents WPF Dispatcher deadlocks caused by sync-over-async patterns in event handlers, Command callbacks, and virtual method overrides. Use when calling async methods from Button.Click, Window.Loaded, Window.Closing, Application.OnExit, ItemsControl.SelectionChanged, or any callback that runs on the Dispatcher thread. Covers the DispatcherSynchronizationContext capture mechanism that causes .GetAwaiter().GetResult(), .Wait(), and .Result to deadlock, explains why Task.Run wrapping is not a real fix, enforces async void with try/catch as the only safe event-handler pattern, and provides reentrancy guards plus CommunityToolkit.Mvvm and Prism 9 command alternatives. Apply whenever code calls an async method from WPF UI thread code, especially when the method signature cannot be changed to async Task (overrides, interface implementations, event handlers).
user-invocable: false
model: sonnet
---

# Preventing Dispatcher Deadlock in WPF Event Handlers

Every WPF event handler runs on the Dispatcher thread. Blocking an async method from such a handler using `.GetAwaiter().GetResult()`, `.Wait()`, or `.Result` causes a permanent deadlock.

## 1. Deadlock Mechanism

### 1.1 Role of the WPF SynchronizationContext

- `await` captures the current `SynchronizationContext` by default.
- On a WPF UI thread, that context is a `DispatcherSynchronizationContext`.
- The continuation scheduled by `await` is **posted** back to the captured Dispatcher.
- The continuation only runs when the Dispatcher drains its queue.

### 1.2 How sync-over-async produces a deadlock

```
[Dispatcher Thread]
    │ Event handler starts
    │ SomeAsyncMethod().GetAwaiter().GetResult()  ← blocks the Dispatcher
    │
    │   [Inside SomeAsyncMethod]
    │       await Task.Delay(10)
    │       │ captures DispatcherSynchronizationContext
    │       │ Task.Delay completes on a thread pool thread
    │       │ continuation posted back to the Dispatcher queue
    │       │ continuation can only run when the Dispatcher pumps
    │
    │ Dispatcher is blocked inside GetResult()
    │ → continuation never runs
    │ → GetResult() never returns
    └─→ DEADLOCK
```

### 1.3 Summary of the root cause

| Item | Detail |
|------|--------|
| Thread blocked | Dispatcher (UI) thread |
| Waiting for | The `await` continuation |
| Continuation requires | Dispatcher to pump its queue |
| Result | Circular wait → indefinite hang |

---

## 2. Prohibited Patterns

### 2.1 Banned in any WPF event handler

```csharp
// Bad: GetResult() inside a Button Click handler
private void OnSaveClick(object sender, RoutedEventArgs e)
{
    SaveAsync().GetAwaiter().GetResult(); // deadlock
}

// Bad: .Result inside a Loaded event
private void OnLoaded(object sender, RoutedEventArgs e)
{
    var data = LoadDataAsync().Result; // deadlock
}

// Bad: .Wait() inside a Closing event
private void OnClosing(object sender, CancelEventArgs e)
{
    FlushBufferAsync().Wait(); // deadlock
}

// Bad: GetResult() in an OnExit override
protected override void OnExit(ExitEventArgs e)
{
    ReleaseAsync().GetAwaiter().GetResult(); // deadlock
}
```

### 2.2 Why `Task.Run` wrapping is not a real fix

```csharp
// Warning: avoids the deadlock but introduces other problems
private void OnClick(object sender, RoutedEventArgs e)
{
    Task.Run(() => SaveAsync()).GetAwaiter().GetResult();
}
```

**Side effects**:
- The Dispatcher thread is still blocked, so the UI freezes during the call.
- Any UI access inside `SaveAsync` throws a cross-thread exception.
- Exceptions get wrapped in `AggregateException`, making diagnosis harder.
- The wall-clock cost is the same as awaiting, with worse ergonomics.

→ **`Task.Run` wrapping is not a root-cause fix.** It only hides the Dispatcher deadlock by moving the top of the call chain off the UI thread.

---

## 3. Recommended Pattern: `async void` Event Handler

### 3.1 Core rule

Declare the event handler as `async void` and use `await`. `async void` is tolerated **for event handlers only**.

```csharp
// Good: Button Click handler
private async void OnSaveClick(object sender, RoutedEventArgs e)
{
    try
    {
        await SaveAsync();
    }
    catch (Exception ex)
    {
        // Must handle here — exceptions from async void cannot be caught by the caller
        _logger.LogError(ex, "Error while saving");
        MessageBox.Show("Save failed.");
    }
}

// Good: Loaded event
private async void OnLoaded(object sender, RoutedEventArgs e)
{
    try
    {
        var data = await LoadDataAsync();
        ApplyData(data);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load data");
    }
}
```

### 3.2 Why `async void` is limited to event handlers

| Method kind | Return type | Reason |
|-------------|-------------|--------|
| Event handler | `async void` allowed | Event delegate signature requires `void` |
| Command method (`[RelayCommand]`) | `async Task` | Caller can await completion |
| Service method | `async Task` | Exception propagation and composition |
| Library API | `async Task` | Caller retains control |

### 3.3 `async void` caveats

```csharp
// try/catch is mandatory
private async void OnClick(object sender, RoutedEventArgs e)
{
    try
    {
        await DoWorkAsync();
    }
    catch (Exception ex)
    {
        // An uncaught exception in async void is posted to the SynchronizationContext
        // and typically crashes the process
        HandleError(ex);
    }
}
```

- Uncaught exceptions in `async void` are re-thrown on the `SynchronizationContext`, which usually terminates the process.
- `async void` is hard to unit-test → keep business logic in `async Task` methods and `await` them from the handler.

---

## 4. Preventing Reentrancy

When a handler is `async void`, the user can click the same button again while the async work is still running. Guard with a flag or disable the control.

```csharp
private bool _isBusy;

private async void OnSaveClick(object sender, RoutedEventArgs e)
{
    if (_isBusy) return;
    _isBusy = true;

    try
    {
        ((Button)sender).IsEnabled = false;
        await SaveAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during save");
    }
    finally
    {
        _isBusy = false;
        ((Button)sender).IsEnabled = true;
    }
}
```

In MVVM projects, `[RelayCommand]`'s `CanExecute` together with the generated `IsRunning` flag handles this automatically.

---

## 5. Why `ConfigureAwait(false)` Does Not Solve This

`ConfigureAwait(false)` tells an `await` inside library code to skip capturing the context. It is a **performance / decoupling** tool, not a fix for event-handler sync-over-async.

```csharp
// Still deadlocks
protected override void OnExit(ExitEventArgs e)
{
    SaveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    //          ^^^^^^^^^^^^^^^^^^^^
    // ConfigureAwait(false) only affects awaits inside SaveAsync.
    // The top-level GetAwaiter().GetResult() is what blocks the Dispatcher,
    // and it does so regardless of how the inner awaits are configured.
}
```

**Correct usage**: inside reusable library code, write `await SomethingAsync().ConfigureAwait(false)` to avoid forcing callers to marshal back to the UI thread. It is not a tool for fixing blocked handlers.

---

## 6. MVVM Alternatives

### 6.1 CommunityToolkit.Mvvm

```csharp
public sealed partial class EditorViewModel : ObservableObject
{
    // RelayCommand automatically wraps async Task in an async-safe command
    [RelayCommand]
    private async Task SaveAsync()
    {
        await _repository.SaveAsync();
    }
}
```

```xml
<Button Content="Save" Command="{Binding SaveCommand}" />
```

`[RelayCommand]` generates an internal `async void` wrapper that forwards exceptions through `TaskScheduler.UnobservedTaskException`, maintains an `IsRunning` flag, and disables `CanExecute` while running. No manual guard required.

### 6.2 Prism 9

```csharp
public class EditorViewModel : BindableBase
{
    private AsyncDelegateCommand? _saveCommand;
    public AsyncDelegateCommand SaveCommand =>
        _saveCommand ??= new AsyncDelegateCommand(ExecuteSaveAsync);

    private async Task ExecuteSaveAsync()
    {
        await _repository.SaveAsync();
    }
}
```

`AsyncDelegateCommand` blocks reentrancy while executing and routes exceptions to the registered error handler (`IContainerRegistry.RegisterGlobalExceptionHandler` in Prism 9).

---

## 7. Decision Flow

```
Do you need to call an async method from UI-thread code?
    │
    ├─ Yes
    │   ├─ Does the signature return void?
    │   │   ├─ Yes → async void + try/catch + await
    │   │   └─ No  → async Task + await
    │   │
    │   └─ Is the signature fixed by an override (e.g., OnExit)?
    │       └─ Yes → apply the shutting-down-wpf-gracefully skill
    │
    └─ No → use a synchronous method only
```

---

## 8. Checklist

- [ ] No `.GetAwaiter().GetResult()`, `.Wait()`, or `.Result` inside any event handler
- [ ] Event handlers that need async are declared `async void`
- [ ] Every `async void` handler has a top-level `try/catch`
- [ ] Business logic lives in `async Task` methods, not in the handler
- [ ] Reentrant handlers guard with a flag or disable the control
- [ ] MVVM code uses `[RelayCommand]` or `AsyncDelegateCommand` instead of raw handlers
- [ ] No `ConfigureAwait(false)` used as an attempted deadlock workaround
- [ ] Overrides whose signature forbids `async void` use the `shutting-down-wpf-gracefully` skill

---

## 9. Related Skills

| Skill | Relationship |
|-------|--------------|
| `shutting-down-wpf-gracefully` | Applies this skill to shutdown scenarios (`OnExit`, `Window.Closing`) |
| `threading-wpf-dispatcher` | Dispatcher priorities and scheduling |
| `implementing-communitytoolkit-mvvm` | `[RelayCommand]` usage details |

---

## 10. References

- [Don't Block on Async Code — Stephen Cleary](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)
- [Async/Await Best Practices — Microsoft Docs](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [SynchronizationContext — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.threading.synchronizationcontext)
- [DispatcherSynchronizationContext — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatchersynchronizationcontext)
- [Original discussion — dotnetdev.kr forum](https://forum.dotnetdev.kr/t/c-task-getawaiter-and-getresult-with-application-onexit-window-closing/14174)
