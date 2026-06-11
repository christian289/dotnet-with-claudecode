namespace ClaudeDesk.ViewModels;

public interface IUiDispatcher
{
    void Invoke(Action action);
}
