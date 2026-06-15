using Anthropic.SDK;

namespace ClaudeDesk.Services;

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
            Provider.Mock => new MockChatClient(),                 // no HttpClient
            Provider.Anthropic => CreateAnthropic(s, apiKey),
            _ => throw new NotSupportedException($"Install and wire the {s.Provider} SDK first.")
        };
        return new ChatClientBuilder(inner)
            .UseFunctionInvocation(loggerFactory: null, configure: null)
            .Build();
    }

    // Anthropic.SDK: HttpClient is the 2nd ctor arg; endpoint override via ApiUrlFormat template.
    private IChatClient CreateAnthropic(ChatSettings s, string? apiKey)
    {
        var client = new AnthropicClient(new APIAuthentication(apiKey!), Pooled(), requestInterceptor: null);
        if (!string.IsNullOrWhiteSpace(s.BaseUrl))
        {
            client.ApiUrlFormat = $"{s.BaseUrl.TrimEnd('/')}/{{0}}/{{1}}"; // "{base}/{version}/{endpoint}"
        }
        return client.Messages; // .Messages implements IChatClient
    }
}
