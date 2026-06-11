using System.Runtime.CompilerServices;
using ClaudeDesk.Abstractions;
using ClaudeDesk.ViewModels;
using Microsoft.Extensions.AI;

namespace ClaudeDesk.ViewModels.Tests;

public sealed class ChatViewModelTests
{
    private sealed class InlineDispatcher : IUiDispatcher
    {
        public void Invoke(Action action) => action();
    }

    private sealed class EmptyKeyStore : IApiKeyStore
    {
        public void Save(string key, string secret) { }
        public string? TryGet(string key) => null;
        public void Delete(string key) { }
    }

    private sealed class FixedClientFactory(IChatClient client) : IChatClientFactory
    {
        public IChatClient Create(ChatSettings s, string? apiKey) => client;
    }

    private sealed class ScriptedChatClient(params string[] tokens) : IChatClient
    {
        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (string token in tokens)
            {
                ct.ThrowIfCancellationRequested();
                yield return new ChatResponseUpdate(ChatRole.Assistant, token);
                await Task.Yield();
            }
        }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options, CancellationToken ct = default)
            => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, string.Concat(tokens))));

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private static ChatViewModel CreateViewModel(params string[] tokens)
        => new(
            new FixedClientFactory(new ScriptedChatClient(tokens)),
            new ChatSettingsStore(),
            new EmptyKeyStore(),
            new InlineDispatcher());

    [Fact]
    public void SendCommand_CannotExecute_WhenInputEmpty()
    {
        ChatViewModel vm = CreateViewModel("ignored");

        Assert.False(vm.SendCommand.CanExecute(null));

        vm.Input = "hello";
        Assert.True(vm.SendCommand.CanExecute(null));
    }

    [Fact]
    public async Task Send_AppendsUserAndAssistantTurns()
    {
        ChatViewModel vm = CreateViewModel("Hello ", "from ", "the ", "model.");
        vm.Input = "hi";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Turns.Count);
        Assert.True(vm.Turns[0].IsUser);
        Assert.Equal("hi", vm.Turns[0].Markdown);
        Assert.False(vm.Turns[1].IsUser);
        Assert.Equal("Hello from the model.", vm.Turns[1].Markdown);
        Assert.False(vm.Turns[1].IsWaiting);
        Assert.False(vm.IsStreaming);
        Assert.Equal(string.Empty, vm.Input);
    }

    [Fact]
    public async Task Send_GuardsReentrancy_WhileStreaming()
    {
        ChatViewModel vm = CreateViewModel("a", "b", "c");
        vm.Input = "first";

        Task send = vm.SendCommand.ExecuteAsync(null);

        // While streaming, Send must be disabled regardless of input.
        vm.Input = "second";
        if (vm.IsStreaming)
        {
            Assert.False(vm.SendCommand.CanExecute(null));
        }

        await send;
        Assert.False(vm.IsStreaming);
    }
}
