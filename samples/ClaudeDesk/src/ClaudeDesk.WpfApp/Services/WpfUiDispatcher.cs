namespace ClaudeDesk.Services;

public sealed class WpfUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action) => Application.Current.Dispatcher.Invoke(action);
}
