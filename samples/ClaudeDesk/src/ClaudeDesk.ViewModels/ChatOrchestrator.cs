namespace ClaudeDesk.ViewModels;

public sealed class ChatOrchestrator(IChatClient client, IList<AITool> tools, ChatSettings settings)
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
                if (c is FunctionCallContent call) { yield return new ToolStarted(call.Name, call.Arguments); }
                if (c is FunctionResultContent res) { yield return new ToolCompleted(res.CallId, res.Result); }
            }
        }
    }
}
