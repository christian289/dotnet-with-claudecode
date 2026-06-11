namespace ClaudeDesk.ViewModels;

public interface IChatClientFactory
{
    IChatClient Create(ChatSettings s, string? apiKey);
}
