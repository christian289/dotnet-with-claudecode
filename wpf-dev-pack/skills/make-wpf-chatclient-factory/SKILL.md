---
description: "Generates a provider-agnostic IChatClient factory that shares one SocketsHttpHandler across LLM SDKs (OpenAI, Ollama, Anthropic, Gemini) and includes a MockChatClient for account-less development. Use when wiring multiple LLM providers behind Microsoft.Extensions.AI in a WPF app. Usage: /wpf-dev-pack:make-wpf-chatclient-factory <Name>"
argument-hint: [Name]
---

# WPF ChatClient Factory Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the factory class name (e.g., ChatClientFactory)". Do NOT proceed until a valid name is provided. Use the response as the factory name for all subsequent steps.**

Generate `$0` (implementing `I$0`): a provider-agnostic `IChatClient` factory that owns **one shared `SocketsHttpHandler`** (a single connection pool) reused by every LLM SDK, plus a `MockChatClient` so the chat UI runs with no provider or API key.

- Replace `{Namespace}` with the project's root namespace.
- Register the factory as a **DI singleton** — see "Required: singleton lifetime".

> **Full rationale** in the `sharing-httpclient-across-llm-sdks` knowledge topic
> (fetch via `WpfDevPackMcp get_wpf_topic`): the socket-exhaustion anti-pattern,
> `disposeHandler:false`, the per-SDK injection cheat sheet, and the
> version-floor caveat. The mock is covered in
> `hosting-extensions-ai-chatclient-in-wpf-mvvm`.

## ⚠️ Verify SDK signatures with HandMirror first

Each LLM SDK exposes a **different** `HttpClient` injection point and these APIs
are version-sensitive. Before emitting, verify the exact signatures with
HandMirror (`inspect_nuget_package` / `inspect_nuget_package_type`) for whichever
providers you include: `OpenAI`, `OllamaSharp`, `Anthropic.SDK`, `Google.GenAI`,
`System.ClientModel`, `Microsoft.Extensions.AI`. Do not ship a provider arm you
have not verified against the pinned package version.

## Required Packages

```xml
<PackageReference Include="Microsoft.Extensions.AI" Version="9.*" />
<!-- Include only the providers you need: -->
<PackageReference Include="OpenAI" Version="2.*" />
<PackageReference Include="OllamaSharp" Version="5.*" />
<PackageReference Include="Anthropic.SDK" Version="5.*" />
<PackageReference Include="Google.GenAI" Version="1.*" />
```

> **Version-floor caveat**: a native SDK may pin `Microsoft.Extensions.AI.Abstractions`
> to a higher version than other consumers (e.g. an MCP client library). The bump is
> safe only when those other consumers depend on a `[x, )` **floor** that accepts the
> higher version — verify the dependency is a floor, not an exact pin, before bumping.

## Generated Code

### Supporting types

```csharp
namespace {Namespace}.Services;

public enum Provider { Mock, Ollama, OpenAI, AzureOpenAI, OpenAICompatible, Anthropic, Gemini }

public sealed record ChatSettings
{
    public Provider Provider { get; init; }
    public string ModelId { get; init; } = string.Empty;
    public string? BaseUrl { get; init; }
}

public interface I$0
{
    IChatClient Create(ChatSettings s, string? apiKey);
}
```

### $0.cs

Every provider path reuses the same `_shared` handler. Connection pooling lives
in `SocketsHttpHandler` (the pool), not in the per-call `HttpClient` wrapper —
so calling `Pooled()` fresh on each `Create()` still reuses sockets. Only the
*injection shape* differs per SDK (instance vs `Func<HttpClient>`).

```csharp
namespace {Namespace}.Services;

public sealed class $0 : I$0
{
    // One handler == one connection pool shared by every provider for the app's lifetime.
    private readonly SocketsHttpHandler _shared = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    };

    // Lightweight wrapper; disposeHandler:false keeps the shared pool alive when the
    // wrapper (or a chat client that owns it) is disposed.
    private HttpClient Pooled() => new(_shared, disposeHandler: false);

    public IChatClient Create(ChatSettings s, string? apiKey)
    {
        IChatClient inner = s.Provider switch
        {
            Provider.Mock              => new MockChatClient(),                 // no HttpClient
            Provider.Ollama            => CreateOllama(s),
            Provider.OpenAI
              or Provider.AzureOpenAI
              or Provider.OpenAICompatible => CreateOpenAiFamily(s, apiKey),
            Provider.Anthropic         => CreateAnthropic(s, apiKey),
            Provider.Gemini            => CreateGemini(s, apiKey),
            _ => throw new InvalidOperationException($"Unsupported provider: {s.Provider}")
        };
        return new ChatClientBuilder(inner)
            .UseFunctionInvocation(loggerFactory: null, configure: null)
            .Build();
    }

    // OpenAI family (incl. Azure & OpenAI-compatible): handler via Transport on the options.
    private IChatClient CreateOpenAiFamily(ChatSettings s, string? apiKey)
    {
        var options = new OpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(Pooled()) // System.ClientModel.Primitives
        };
        if (!string.IsNullOrWhiteSpace(s.BaseUrl)) { options.Endpoint = new Uri(s.BaseUrl); }
        return new OpenAIClient(new ApiKeyCredential(apiKey!), options)
            .GetChatClient(s.ModelId)
            .AsIChatClient();
    }

    // OllamaSharp: HttpClient is a direct ctor arg; it implements IChatClient explicitly.
    private IChatClient CreateOllama(ChatSettings s)
    {
        HttpClient http = Pooled();
        http.BaseAddress = new Uri(string.IsNullOrWhiteSpace(s.BaseUrl) ? "http://localhost:11434" : s.BaseUrl);
        return new OllamaApiClient(http, s.ModelId, null!); // JsonSerializerContext may be null
    }

    // Anthropic.SDK: HttpClient is the 2nd ctor arg; endpoint override via ApiUrlFormat template.
    private IChatClient CreateAnthropic(ChatSettings s, string? apiKey)
    {
        var client = new AnthropicClient(new APIAuthentication(apiKey!), Pooled(), requestInterceptor: null);
        if (!string.IsNullOrWhiteSpace(s.BaseUrl))
        {
            client.ApiUrlFormat = $"{s.BaseUrl.TrimEnd('/')}/{{0}}/{{1}}"; // "{base}/{version}/{endpoint}"
        }
        return client.Messages; // .Messages is the IChatClient
    }

    // Google.GenAI (official): HttpClient via a FACTORY on ClientOptions, not a direct param.
    private IChatClient CreateGemini(ChatSettings s, string? apiKey)
    {
        var clientOptions = new ClientOptions { HttpClientFactory = Pooled }; // Func<HttpClient>
        HttpOptions? http = string.IsNullOrWhiteSpace(s.BaseUrl) ? null : new HttpOptions { BaseUrl = s.BaseUrl };
        var client = new Client(
            enterprise: null, vertexAI: null, apiKey: apiKey,
            credential: null, project: null, location: null,
            httpOptions: http, clientOptions: clientOptions);
        return client.AsIChatClient(s.ModelId); // GoogleGenAIExtensions, in the Microsoft.Extensions.AI namespace
    }
}
```

### Per-SDK injection cheat sheet

| SDK | Where the shared `HttpClient` goes | Endpoint override |
|-----|-----------------------------------|-------------------|
| OpenAI / Azure / compatible | `OpenAIClientOptions.Transport = new HttpClientPipelineTransport(Pooled())` | `OpenAIClientOptions.Endpoint` |
| OllamaSharp | `new OllamaApiClient(Pooled(), model, jsonCtx)` (direct ctor arg) | `HttpClient.BaseAddress` |
| Anthropic.SDK | `new AnthropicClient(auth, Pooled(), interceptor)` (2nd arg) | `client.ApiUrlFormat` template |
| Google.GenAI | `new ClientOptions { HttpClientFactory = Pooled }` (a `Func<HttpClient>`) | `HttpOptions.BaseUrl` |

### MockChatClient.cs (account-less development)

```csharp
namespace {Namespace}.Services;

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

> Pair `Provider.Mock` with an orchestrator that **skips MCP connection** (empty
> tools) so the UI runs with no server at all.

## Required: singleton lifetime

The shared handler is an instance field of the factory, so reuse depends on the
**same factory instance** living for the app. Registering the factory
transient/scoped creates a fresh `SocketsHttpHandler` (a fresh pool) on every
resolve, silently defeating the pattern — no compile or runtime error.

```csharp
// REQUIRED: one factory instance for the app's lifetime, so _shared is truly shared.
services.AddSingleton<I$0, $0>(); // GenericHost (CommunityToolkit.Mvvm)
// Prism: containerRegistry.RegisterSingleton<I$0, $0>();
// Transient/Scoped => new SocketsHttpHandler per resolve => no shared pool.
```

## File Location

```
{Project}/
└── Services/
    ├── $0.cs
    ├── I$0.cs            (interface — or fold into $0.cs per project convention)
    ├── ChatSettings.cs
    ├── Provider.cs
    └── MockChatClient.cs
```

## Related

- Knowledge (fetch via `WpfDevPackMcp get_wpf_topic`): `sharing-httpclient-across-llm-sdks`,
  `hosting-extensions-ai-chatclient-in-wpf-mvvm`, `configuring-dependency-injection`.
- Sibling scaffolders: feeds `/wpf-dev-pack:make-wpf-chat-orchestrator` (which
  consumes the produced `IChatClient`). Both are emitted by the one-button
  `/wpf-dev-pack:make-wpf-chatclient`.
