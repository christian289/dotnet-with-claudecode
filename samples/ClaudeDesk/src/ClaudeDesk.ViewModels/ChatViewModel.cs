using ClaudeDesk.Abstractions;

namespace ClaudeDesk.ViewModels;

public sealed partial class ChatViewModel : ObservableObject
{
    private readonly IChatClientFactory _factory;
    private readonly ChatSettingsStore _settingsStore;
    private readonly IApiKeyStore _keyStore;
    private readonly IUiDispatcher _dispatcher;
    private readonly List<ChatMessage> _history = [];
    private CancellationTokenSource? _cts;
    private ChatTurnViewModel? _current;

    public ObservableCollection<ChatTurnViewModel> Turns { get; } = [];

    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    [ObservableProperty] private string _input = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [ObservableProperty] private bool _isStreaming;

    public ChatViewModel(
        IChatClientFactory factory,
        ChatSettingsStore settingsStore,
        IApiKeyStore keyStore,
        IUiDispatcher dispatcher)
    {
        _factory = factory;
        _settingsStore = settingsStore;
        _keyStore = keyStore;
        _dispatcher = dispatcher;
    }

    private bool CanSend() => !IsStreaming && !string.IsNullOrWhiteSpace(Input);

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        string prompt = Input.Trim();
        Input = string.Empty;

        Turns.Add(new ChatTurnViewModel { IsUser = true, Markdown = prompt });
        _history.Add(new ChatMessage(ChatRole.User, prompt));

        _current = new ChatTurnViewModel { IsUser = false, IsWaiting = true };
        Turns.Add(_current);

        IsStreaming = true;
        _cts = new CancellationTokenSource();
        try
        {
            // Per-send client creation is cheap: the singleton factory owns the
            // shared SocketsHttpHandler pool, so sockets are still reused.
            ChatSettings settings = _settingsStore.Current;
            string? apiKey = settings.Provider == Provider.Mock
                ? null
                : _keyStore.TryGet(CredentialNames.ApiKey);
            var orchestrator = new ChatOrchestrator(_factory.Create(settings, apiKey), tools: [], settings);

            await foreach (ChatEvent ev in orchestrator.SendAsync(_history, _cts.Token))
            {
                _dispatcher.Invoke(() => Apply(ev)); // marshal before touching bound state
            }
            _history.Add(new ChatMessage(ChatRole.Assistant, _current.Markdown));
        }
        catch (OperationCanceledException) { /* user pressed Stop — normal, not an error */ }
        finally
        {
            if (_current is not null) { _current.IsWaiting = false; }
            IsStreaming = false;
            _cts?.Dispose(); _cts = null;
        }
    }

    private bool CanStop() => IsStreaming;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _cts?.Cancel();

    private void Apply(ChatEvent ev)
    {
        switch (ev)
        {
            case ChatText t: _current!.AppendMarkdown(t.Text); _current.IsWaiting = false; break;
            case ToolStarted s: _current!.AppendMarkdown($"\n\n*Calling `{s.Name}`…*\n\n"); break;
            case ToolCompleted: break; // optionally surface the tool result
            case ChatFailed f: _current!.AppendMarkdown($"\n\n> {f.Error.Message}"); break;
        }
    }
}
