namespace ClaudeDesk.ViewModels;

/// <summary>
/// App-lifetime holder for the active <see cref="ChatSettings"/>.
/// The settings panel replaces <see cref="Current"/>; each send reads it.
/// </summary>
public sealed class ChatSettingsStore
{
    public ChatSettings Current { get; set; } = new() { Provider = Provider.Mock, ModelId = "mock" };
}
