# Hosting a Streaming Microsoft.Extensions.AI IChatClient in WPF MVVM

> Consumes a streaming `Microsoft.Extensions.AI` `IChatClient.GetStreamingResponseAsync` inside a WPF MVVM application and marshals every `ChatResponseUpdate` onto the Dispatcher before touching bound state, because async stream continuations resume on thread-pool threads and corrupt or throw on bound `ObservableCollection`/property writes. Covers letting `ChatClientBuilder.UseFunctionInvocation()` (FunctionInvokingChatClient) run the model-to-tool-call-to-tool-result loop automatically instead of hand-rolling an agent loop, dropping MCP `AIFunction` tools straight into `ChatOptions.Tools`, the C# rule that `yield` is illegal inside a `catch` clause (CS1631) and how to catch into a flag and yield the failure after the catch, a mock `IChatClient` that drives the whole UI offline with no provider or API key, and Stop/cancellation/re-entrancy handling that treats `OperationCanceledException` as a normal stop rather than an error.

Streaming a chat response from `Microsoft.Extensions.AI` into a WPF UI involves three orthogonal concerns — thread affinity, the tool-call loop, and a C# iterator restriction — plus offline development and cancellation. This topic addresses each in turn.

---

## 1. Decompose the Stream into UI-Agnostic Events (yield the failure AFTER the catch)

Do not write `ChatResponseUpdate` chunks directly into bound state from the orchestrator. Project the raw stream into a sequence of small, UI-agnostic events (`ChatText`, `ToolStarted`, `ToolCompleted`, `ChatFailed`) so the ViewModel decides how to render each one.

The compiler restriction that shapes this method: C# forbids `yield return` and `yield break` inside the body of a `catch` clause (**CS1631** — "Cannot yield a value in the body of a catch clause"). So you cannot catch a stream exception and immediately `yield` a failure event. Instead, catch into a local flag, then yield the failure event **after** the `try/catch` block has closed.

Emit text via `update.Text` (robust across stream/SDK shapes) rather than walking `TextContent`. A `OperationCanceledException` from the stream is a normal stop — `yield break` out of it, do not surface it as a failure.

```csharp
public async IAsyncEnumerable<ChatEvent> SendAsync(
    IList<ChatMessage> history, [EnumeratorCancellation] CancellationToken ct)
{
    var options = new ChatOptions { Tools = _tools, ModelId = _settings.ModelId };
    await using var e = _client.GetStreamingResponseAsync(history, options, ct).GetAsyncEnumerator(ct);

    while (true)
    {
        ChatResponseUpdate? update = null;
        bool faulted = false; Exception? error = null;
        try
        {
            if (!await e.MoveNextAsync()) { break; }
            update = e.Current;
        }
        catch (OperationCanceledException) { yield break; }
        catch (Exception ex) { faulted = true; error = ex; }   // CS1631: cannot yield here

        if (faulted) { yield return new ChatFailed(error!); yield break; }

        if (!string.IsNullOrEmpty(update!.Text)) { yield return new ChatText(update.Text); }
        foreach (AIContent c in update.Contents)
        {
            if (c is FunctionCallContent call)  { yield return new ToolStarted(call.Name, call.Arguments); }
            if (c is FunctionResultContent res) { yield return new ToolCompleted(res.CallId, res.Result); }
        }
    }
}
```

---

## 2. Let `UseFunctionInvocation()` Run the Tool Loop — Don't Hand-Roll It

Hand-rolling the model → tool-call → tool-result → model cycle is error-prone and unnecessary. `Microsoft.Extensions.AI` ships `FunctionInvokingChatClient` as a middleware that runs the tool loop automatically when tools are supplied. Wrap the provider client once via `ChatClientBuilder.UseFunctionInvocation()` and the loop becomes automatic.

MCP client tools surface as `AIFunction` (specifically `McpClientTool : AIFunction`), so they drop straight into `ChatOptions.Tools` with no adapter.

```csharp
IChatClient client = new ChatClientBuilder(inner)
    .UseFunctionInvocation(loggerFactory: null, configure: null)
    .Build();
// tools: IList<McpClientTool> from ModelContextProtocol's McpClient.ListToolsAsync() — McpClientTool : AIFunction
```

---

## 3. Marshal Every Event onto the Dispatcher in the ViewModel

Async stream continuations are not Dispatcher-affine — `await foreach` can resume on a thread-pool thread. Writing to a bound `ObservableCollection` or bound property from that thread throws a cross-thread exception or produces binding errors. Marshal each event onto the UI Dispatcher with `Application.Current.Dispatcher.Invoke(...)` before touching bound state.

A "waiting …" indicator bound to `IsWaiting` (flipped `false` on the first `ChatText`) covers the visual gap before the first token arrives.

```csharp
var cts = new CancellationTokenSource(); // Stop button cancels
await foreach (ChatEvent ev in _orchestrator.SendAsync(_history, cts.Token))
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        switch (ev)
        {
            case ChatText t:        _current.AppendMarkdown(t.Text); _current.IsWaiting = false; break;
            case ToolStarted s:     _current.ToolCalls.Add(new ToolCallVm(s)); break;
            case ToolCompleted d:   _current.CompleteTool(d); break;
            case ChatFailed f:      _current.AppendMarkdown($"\n\n> {f.Error.Message}"); break;
        }
    });
}
```

See [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/TOPIC.md) for why each WPF object has thread affinity. When a long-lived async transport (e.g. a child-process stdio MCP client) must be disposed during `OnExit`, do not `.GetAwaiter().GetResult()` on the UI thread — see [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/TOPIC.md).

---

## 4. A Mock `IChatClient` for Account-Less Development

`IChatClient` is a small interface. A fake that yields scripted `ChatResponseUpdate`s drives the entire orchestrator and UI path — streaming text, bubbles, markdown, tool display — with no network and no API key, so all UI work can proceed before a provider account is procured. It also simulates token cadence, so the streaming feel is exercisable offline.

```csharp
internal sealed class MockChatClient : IChatClient
{
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        const string reply = "This is a **mock** streamed reply.\n\n```csharp\nvar x = 1;\n```";
        foreach (string token in reply.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return new ChatResponseUpdate(ChatRole.Assistant, token + " ");
            await Task.Delay(40, ct); // simulate token cadence
        }
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options, CancellationToken ct = default)
        => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "mock")));

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
```

Pair it with a `Provider.Mock` that **skips MCP connection** (empty tools) so the UI runs with no server at all. Because the mock satisfies the same streaming contract that the production path consumes, the same fake also feeds ViewModel tests — see [`testing-wpf-viewmodels`](../testing-wpf-viewmodels/TOPIC.md).

---

## 5. Cancellation, Stop, and Re-entrancy (with `IsWaiting`)

A Stop button must cancel an in-flight response; pressing Enter again mid-stream must not start an overlapping send that corrupts the turn list; and cancelling must not paint a scary error bubble. Three pieces cover this: a per-send `CancellationTokenSource`, a re-entrancy guard on Send's `CanExecute` while streaming, and `OperationCanceledException` caught and treated as a normal stop.

```csharp
private CancellationTokenSource? _cts;
public bool IsStreaming { get; private set; }

private async Task SendAsync()
{
    if (IsStreaming || string.IsNullOrWhiteSpace(Input)) { return; } // re-entrancy + empty guard
    IsStreaming = true;
    _cts = new CancellationTokenSource();
    RaiseCommandStates(); // Send disabled, Stop enabled
    try
    {
        await foreach (ChatEvent ev in _orchestrator.SendAsync(_history, _cts.Token))
        {
            Application.Current.Dispatcher.Invoke(() => Apply(ev)); // Dispatcher-marshaled — see §3
        }
    }
    catch (OperationCanceledException) { /* user pressed Stop — normal, not an error */ }
    finally
    {
        IsStreaming = false;
        _cts.Dispose(); _cts = null;
        RaiseCommandStates();
    }
}

private void Stop() => _cts?.Cancel();
// SendCommand.CanExecute => !IsStreaming && !string.IsNullOrWhiteSpace(Input)
// StopCommand.CanExecute => IsStreaming
```

---

## References

- [Microsoft.Extensions.AI libraries — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- [IChatClient Interface — Microsoft Learn](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai.ichatclient)
- [Build an AI chat app with .NET — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/quickstarts/build-chat-app)
- [Function calling with Microsoft.Extensions.AI — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/advanced/function-invocation)
- [Iterators (C# yield) — Microsoft Learn](https://learn.microsoft.com/dotnet/csharp/iterators)
- [Dispatcher.Invoke Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.threading.dispatcher.invoke)
- [CancellationTokenSource Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtokensource)

### Related topics

- [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/TOPIC.md) — why each `ChatResponseUpdate` must be marshaled onto the UI Dispatcher before touching bound state.
- [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/TOPIC.md) — the shutdown trap when disposing a long-lived stdio MCP transport during `OnExit`.
- [`sharing-httpclient-across-llm-sdks`](../sharing-httpclient-across-llm-sdks/TOPIC.md) — pooling the `HttpClient` behind the provider `IChatClient`.
- [`consuming-mcp-tools-in-extensions-ai`](../consuming-mcp-tools-in-extensions-ai/TOPIC.md) — turning `McpClientTool` (`AIFunction`) instances into `ChatOptions.Tools`.
- [`testing-wpf-viewmodels`](../testing-wpf-viewmodels/TOPIC.md) — reusing the mock `IChatClient` to drive ViewModel tests.
