namespace ClaudeDesk.ViewModels;

public sealed partial class ChatTurnViewModel : ObservableObject
{
    [ObservableProperty] private bool _isUser;
    [ObservableProperty] private bool _isWaiting;          // "waiting…" until the first token
    [ObservableProperty] private string _markdown = string.Empty;

    public void AppendMarkdown(string delta) => Markdown += delta;
}
