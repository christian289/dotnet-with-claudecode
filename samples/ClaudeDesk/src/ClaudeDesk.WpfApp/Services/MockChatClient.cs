namespace ClaudeDesk.Services;

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
