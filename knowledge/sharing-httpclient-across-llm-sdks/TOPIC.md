# Sharing One HttpClient Across LLM SDKs in a WPF App

> A provider-agnostic chat client that `new`s an `HttpClient` per request (per provider, per send) risks socket exhaustion under sustained use — the classic anti-pattern. The fix is ONE shared `SocketsHttpHandler` (a single connection pool, with `PooledConnectionLifetime` guarding against stale DNS) wrapped by lightweight `new HttpClient(handler, disposeHandler: false)` instances. Each LLM SDK injects the `HttpClient` at a DIFFERENT point: OpenAI/Azure via `HttpClientPipelineTransport` on `Transport`, OllamaSharp and Anthropic.SDK via a constructor argument, and Google.GenAI via a `Func<HttpClient>` factory on `ClientOptions`. Crucially, reuse only actually happens when the factory owning the handler is registered as a DI SINGLETON — lifetime is part of the pattern's correctness. Includes a version-floor caveat for `Microsoft.Extensions.AI.Abstractions`.

These clients feed the same `ChatClientBuilder` consumed in [`hosting-extensions-ai-chatclient-in-wpf-mvvm`](../hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md); the singleton-lifetime requirement is an application of [`configuring-dependency-injection`](../configuring-dependency-injection/TOPIC.md).

---

## 1. The Shared-Handler Factory

A provider-agnostic chat client that `new`s an `HttpClient` per request risks socket exhaustion under sustained use. Connection pooling lives in `SocketsHttpHandler`, so the fix is to own **one** handler for the app's lifetime and wrap it in cheap per-use `HttpClient` instances. The friction is that each LLM SDK exposes a *different* `HttpClient` injection point.

The full reuse-oriented factory (placeholders for project types):

```csharp
public sealed class ChatClientFactory : IChatClientFactory
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

**Every provider path reuses the same `_shared` handler.** Connection pooling lives in `SocketsHttpHandler` (the pool), not in the per-call `HttpClient` wrapper — so calling `Pooled()` fresh on each `Create()` still reuses sockets, because every wrapper wraps the one `_shared` handler. The only difference is the *injection shape*: OpenAI / Ollama / Anthropic.SDK accept an `HttpClient` **instance** (`Pooled()` invoked), while Google.GenAI accepts a `Func<HttpClient>` **factory** (`HttpClientFactory = Pooled`, a method group). The literal name `HttpClientFactory` appears only for Gemini for that reason — it is **not** a sign that the other SDKs skip reuse.

---

## 2. Why `disposeHandler: false`

The shared `SocketsHttpHandler` owns the connection pool and must outlive every individual `HttpClient` wrapper. A chat client's `Dispose` (for example, an orchestrator `using` block, or disposing a per-send `IChatClient`) must **not** tear down the shared pool.

- `new HttpClient(handler, disposeHandler: false)` produces a wrapper that, when disposed, leaves `_shared` untouched.
- The wrapper is cheap to create and dispose; the pool it wraps persists for the app's lifetime.
- If you instead let the wrapper dispose the handler (`disposeHandler: true`, the default for the single-arg constructor), the first `Dispose` of any chat client would dispose the shared pool, defeating reuse for every subsequent provider.

---

## 3. Per-SDK Injection-Point Cheat Sheet

Each SDK accepts the shared `HttpClient` (or a factory that returns it) at a different place. Reproduce these injection points exactly:

| SDK | Where the shared `HttpClient` goes | Endpoint override |
|-----|-----------------------------------|-------------------|
| OpenAI / Azure / compatible | `OpenAIClientOptions.Transport = new HttpClientPipelineTransport(Pooled())` | `OpenAIClientOptions.Endpoint` |
| OllamaSharp | `new OllamaApiClient(Pooled(), model, jsonCtx)` (direct ctor arg) | `HttpClient.BaseAddress` |
| Anthropic.SDK | `new AnthropicClient(auth, Pooled(), interceptor)` (2nd arg) | `client.ApiUrlFormat` template |
| Google.GenAI | `new ClientOptions { HttpClientFactory = Pooled }` (a `Func<HttpClient>`) | `HttpOptions.BaseUrl` |

OpenAI / Ollama / Anthropic.SDK take an `HttpClient` **instance** (invoke `Pooled()`); Google.GenAI takes a `Func<HttpClient>` **factory** (pass `Pooled` as a method group). All four paths ultimately draw on the one `_shared` handler.

---

## 4. Version-Floor Caveat (`Microsoft.Extensions.AI.Abstractions`)

A native SDK may pin `Microsoft.Extensions.AI.Abstractions` to a higher version than other consumers (for example, an MCP client library). The bump is safe only when those other consumers depend on a `[x, )` **floor** that accepts the higher version — verify the dependency is a floor, not an exact pin, before bumping.

- Inspect the transitive dependency: a floor like `[9.0.0, )` accepts the higher version; an exact pin like `[9.0.0]` does not.
- If a consumer exact-pins the lower version, the higher SDK and that consumer cannot coexist on the same `Microsoft.Extensions.AI.Abstractions` version — resolve the conflict before adding the SDK.

---

## 5. The Factory MUST Be a DI Singleton (lifetime is correctness)

> **Critical callout.** The shared `SocketsHttpHandler` only shares if the factory is a DI **singleton**. After adopting the shared-handler pattern, connection reuse still doesn't happen — sockets churn / exhaust under sustained use — with no compile or runtime error. The handler is an instance field of the factory; reuse depends on the **same factory instance** living for the app. Registering the factory transient or scoped creates a fresh factory (and a fresh `SocketsHttpHandler`, i.e. a fresh pool) on every resolve, silently nullifying the optimization. The lifetime is part of the pattern's correctness — not an afterthought.

```csharp
// REQUIRED: one factory instance for the app's lifetime, so _shared is truly shared.
container.RegisterSingleton<IChatClientFactory, ChatClientFactory>();
// Transient/Scoped => new SocketsHttpHandler per resolve => no shared pool.

// Per-send you may still Create() a fresh IChatClient wrapper and dispose it (cheap);
// disposeHandler:false (section 2) keeps the singleton-owned pool alive across sends.
```

---

## 6. References

- [Guidelines for using HttpClient — Microsoft Learn](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [SocketsHttpHandler Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.net.http.socketshttphandler)
- [SocketsHttpHandler.PooledConnectionLifetime Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.net.http.socketshttphandler.pooledconnectionlifetime)
- [HttpClient Constructor (HttpMessageHandler, Boolean) — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient.-ctor#system-net-http-httpclient-ctor(system-net-http-httpmessagehandler-system-boolean))
- [IHttpClientFactory with .NET — Microsoft Learn](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)
- [Dependency injection in .NET — service lifetimes — Microsoft Learn](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes)

### Related topics

- [`hosting-extensions-ai-chatclient-in-wpf-mvvm`](../hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md) — these per-provider clients feed the same `ChatClientBuilder` that surfaces an `IChatClient` to a WPF MVVM chat view.
- [`configuring-dependency-injection`](../configuring-dependency-injection/TOPIC.md) — service lifetimes; the singleton registration that makes the shared handler actually shared.
