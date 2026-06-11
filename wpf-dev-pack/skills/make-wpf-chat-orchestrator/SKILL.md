---
description: "Generates a streaming chat orchestrator that decomposes IChatClient.GetStreamingResponseAsync into UI-agnostic events (text, tool-started, tool-completed, failed), plus MCP tool acquisition via McpClient.CreateAsync and StdioClientTransport. Use when wiring a streaming Microsoft.Extensions.AI chat loop with MCP tools into a WPF app. Usage: /wpf-dev-pack:make-wpf-chat-orchestrator <Name>"
argument-hint: [Name]
---

# WPF Chat Orchestrator Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the orchestrator class name (e.g., ChatOrchestrator)". Do NOT proceed until a valid name is provided. Use the response as the orchestrator name for all subsequent steps.**

Generate `$0`: a streaming chat orchestrator that decomposes `IChatClient.GetStreamingResponseAsync` into a stream of UI-agnostic `ChatEvent`s, plus the MCP tool acquisition (`McpClient.CreateAsync` + `StdioClientTransport`) that feeds `ChatOptions.Tools`.

- Replace `{Namespace}` with the project's root namespace.
- The `IChatClient` is produced by `/wpf-dev-pack:make-wpf-chatclient-factory`
  (already wrapped with `UseFunctionInvocation`, so the tool loop runs automatically).

> **Full rationale** in two knowledge topics (fetch via `WpfDevPackMcp get_wpf_topic`):
> `hosting-extensions-ai-chatclient-in-wpf-mvvm` (Dispatcher marshaling, the
> `yield`-illegal-in-`catch` (CS1631) restructuring, cancellation) and
> `consuming-mcp-tools-in-extensions-ai` (there is **no** `McpClientFactory`;
> `McpClient` is abstract — use the static `CreateAsync`; off-Dispatcher exit dispose).

## ⚠️ Verify signatures with HandMirror first

`Microsoft.Extensions.AI` and the MCP SDK evolve quickly. Before emitting, verify
with HandMirror (`inspect_nuget_package` / `inspect_nuget_package_type`):
`Microsoft.Extensions.AI` (`IChatClient`, `ChatResponseUpdate`, `ChatOptions`,
`FunctionCallContent`, `FunctionResultContent`) and `ModelContextProtocol`
(`McpClient.CreateAsync`, `StdioClientTransport`, `McpClientTool`).

## Required Packages

```xml
<PackageReference Include="Microsoft.Extensions.AI" Version="9.*" />
<PackageReference Include="ModelContextProtocol" Version="0.*" />
```

## Generated Code

### ChatEvent.cs (UI-agnostic stream events)

```csharp
namespace {Namespace}.Services;

public abstract record ChatEvent;
public sealed record ChatText(string Text) : ChatEvent;
public sealed record ToolStarted(string Name, IDictionary<string, object?>? Arguments) : ChatEvent;
public sealed record ToolCompleted(string CallId, object? Result) : ChatEvent;
public sealed record ChatFailed(Exception Error) : ChatEvent;
```

### $0.cs (stream decomposition)

C# forbids `yield` inside a `catch` (**CS1631**). So the loop catches a fault
into a flag and yields the failure event **after** the `try/catch` closes. A
`OperationCanceledException` is a normal stop (`yield break`), not a failure.

```csharp
namespace {Namespace}.Services;

public sealed class $0(IChatClient client, IList<AITool> tools, ChatSettings settings)
{
    private readonly IChatClient _client = client;
    private readonly IList<AITool> _tools = tools;
    private readonly ChatSettings _settings = settings;

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
}
```

> Emit text via `update.Text` (robust across stream/SDK shapes) rather than
> walking `TextContent`.

### McpToolService.cs (tool acquisition + stdio lifecycle)

There is **no** `McpClientFactory`. `McpClient` is abstract — use the static
`CreateAsync`. Connect once and cache (re-connecting per send re-spawns the
child process); dispose **off** the Dispatcher on app exit.

```csharp
namespace {Namespace}.Services;

public sealed class McpToolService : IAsyncDisposable
{
    private McpClient? _mcp;
    public IList<McpClientTool> Tools { get; private set; } = [];

    // For a framework-dependent dll: Command="dotnet", Arguments=[dllPath].
    // For a self-contained exe: Command=exePath.
    public async Task ConnectAsync(string serverCommand, IList<string> serverArgs,
        ILoggerFactory? loggerFactory, CancellationToken ct)
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "tools",
            Command = serverCommand,
            Arguments = serverArgs,
        }, loggerFactory);

        // NOTE: there is NO McpClientFactory. McpClient is abstract — use the static CreateAsync.
        _mcp = await McpClient.CreateAsync(transport, clientOptions: null, loggerFactory: null, ct);
        Tools = await _mcp.ListToolsAsync(cancellationToken: ct); // McpClientTool : AIFunction
    }

    public async ValueTask DisposeAsync()
    {
        if (_mcp is not null) { await _mcp.DisposeAsync(); }
    }
}
```

`McpClientTool : AIFunction`, so the acquired tools drop straight into the
orchestrator's `IList<AITool>`:

```csharp
var options = new ChatOptions { Tools = [.. mcpToolService.Tools], ModelId = modelId };
```

### App exit — dispose the stdio client OFF the Dispatcher

Disposing the stdio client ends its child process. From `App.OnExit` (UI thread)
you must **not** block sync-over-async, which deadlocks the Dispatcher:

```csharp
// App.OnExit — bounded, OFF the Dispatcher; never .GetAwaiter().GetResult()/.Wait() on the UI thread.
Task.Run(async () => await _mcpService.DisposeAsync()).Wait(TimeSpan.FromSeconds(3));
```

## How the ViewModel consumes it (Dispatcher marshaling + cancellation)

Async stream continuations are not Dispatcher-affine — marshal each event onto
the UI Dispatcher before touching bound state, gate Send with a re-entrancy
guard, and treat `OperationCanceledException` as a normal stop:

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
            Application.Current.Dispatcher.Invoke(() => Apply(ev)); // marshal before touching bound state
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

> The full ViewModel (turns collection, `IsWaiting`, `Apply(ev)` switch) is
> emitted by the one-button `/wpf-dev-pack:make-wpf-chatclient`.

## File Location

```
{Project}/
└── Services/
    ├── ChatEvent.cs
    ├── $0.cs
    └── McpToolService.cs
```

## Related

- Knowledge (fetch via `WpfDevPackMcp get_wpf_topic`): `hosting-extensions-ai-chatclient-in-wpf-mvvm`,
  `consuming-mcp-tools-in-extensions-ai`, `threading-wpf-dispatcher`,
  `preventing-dispatcher-deadlock`.
- Sibling scaffolders: consumes the `IChatClient` from
  `/wpf-dev-pack:make-wpf-chatclient-factory`. Emitted as part of the one-button
  `/wpf-dev-pack:make-wpf-chatclient`.
