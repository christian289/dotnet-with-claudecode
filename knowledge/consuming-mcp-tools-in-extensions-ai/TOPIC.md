# Consuming MCP Tools in Microsoft.Extensions.AI (WPF clients)

> Acquires Model Context Protocol (MCP) tools and feeds them into `ChatOptions.Tools` from a .NET / WPF chat client. The entry point is the **static** `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)` — `McpClient` is abstract and there is **no** `McpClientFactory` (reaching for one will not compile). A local server process is reached through `StdioClientTransport`: a framework-dependent dll uses `Command="dotnet"` plus `Arguments=[dllPath]`, while a self-contained exe uses `Command=exePath`. `ListToolsAsync()` returns `McpClientTool`, which derives from `AIFunction`, so the tools drop straight into `ChatOptions.Tools`. Connect once and cache the client (re-connecting per send re-spawns the child process), aggregate tools across multiple servers, and dispose the stdio client **off** the Dispatcher on app exit — never sync-over-async on the UI thread.

This topic covers MCP *client* acquisition for a chat client, not authoring an MCP server.

---

## 1. The `CreateAsync` Shape (there is NO `McpClientFactory`)

The MCP .NET client entry point is the **static** `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)`. `McpClient` is **abstract** (no public constructor), and there is no `McpClientFactory` type — reaching for one will not compile. A local server process is reached via `StdioClientTransport`.

For a framework-dependent dll, set `Command="dotnet"` and `Arguments=[dllPath]`. For a self-contained exe, set `Command=exePath` (no extra arguments needed).

**Package layout (ModelContextProtocol 1.x, verified against 1.4.0):** the
client types above (`McpClient`, `StdioClientTransport`, `McpClientTool`) ship
in the `ModelContextProtocol.Core` assembly. The `ModelContextProtocol` package
itself carries only server/DI types but depends on Core, so a single
`<PackageReference Include="ModelContextProtocol" Version="1.*" />` still
compiles all client code, and the `ModelContextProtocol.Client` namespace is
unchanged. The full `CreateAsync` parameter list as of 1.4.0:
`CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)`.

```csharp
// Connect to a local stdio MCP server process. For a framework-dependent dll use
// Command="dotnet", Arguments=[dllPath]; for a self-contained exe, Command=exePath.
var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "tools",
    Command = serverCommand,
    Arguments = serverArgs,
}, loggerFactory);

// NOTE: there is NO McpClientFactory. McpClient is abstract — use the static CreateAsync.
McpClient mcp = await McpClient.CreateAsync(transport, clientOptions: null, loggerFactory: null, ct);
IList<McpClientTool> tools = await mcp.ListToolsAsync(cancellationToken: ct); // McpClientTool : AIFunction
```

---

## 2. `ListToolsAsync` → `ChatOptions.Tools`

`ListToolsAsync()` returns `IList<McpClientTool>`, and `McpClientTool : AIFunction`. Because `ChatOptions.Tools` is a collection of `AITool`, the MCP tools drop straight in with a spread element — no adapter or wrapper layer is required.

```csharp
var options = new ChatOptions { Tools = [.. tools], ModelId = modelId }; // straight into the chat call
```

---

## 3. Connect Once and Cache; Aggregating Multiple Servers

Connect once and cache the `McpClient` (re-connecting per send re-spawns the child process). If several servers are connected, aggregate their tools into one list before building `ChatOptions`.

- Create the transport and `McpClient` once during initialization, not on every send.
- Hold the connected client (and its tool list) for the lifetime of the chat session.
- When more than one server is connected, concatenate each server's `ListToolsAsync()` result, then pass the combined list to `ChatOptions.Tools`.

---

## 4. Off-Dispatcher Exit Disposal

Disposing the stdio client ends its child process. From `App.OnExit` (which runs on the UI thread) you must **not** block sync-over-async — calling `.GetAwaiter().GetResult()` or `.Wait()` directly on the async dispose deadlocks the Dispatcher. Marshal the async dispose onto a thread-pool thread with a bounded wait instead.

```csharp
// App.OnExit — bounded, OFF the Dispatcher; never .GetAwaiter().GetResult()/.Wait() the async dispose on the UI thread.
Task.Run(async () => await _mcpService.DisposeAsync()).Wait(TimeSpan.FromSeconds(3));
```

See [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/TOPIC.md) for the broader sync-over-async deadlock pattern.

---

## References

- [ModelContextProtocol C# SDK — GitHub](https://github.com/modelcontextprotocol/csharp-sdk)
- [Microsoft.Extensions.AI libraries — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- [Build an MCP client in .NET — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-client)

### Related topics

- [`hosting-extensions-ai-chatclient-in-wpf-mvvm`](../hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md) — where these MCP tools are consumed in the chat call.
- [`preventing-dispatcher-deadlock`](../preventing-dispatcher-deadlock/TOPIC.md) — the off-Dispatcher exit-disposal trap.
