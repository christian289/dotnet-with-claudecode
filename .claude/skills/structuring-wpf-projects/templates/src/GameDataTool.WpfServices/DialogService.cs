namespace GameDataTool.WpfServices;

/// <summary>
/// 다이얼로그 서비스 인터페이스
/// Dialog service interface
/// </summary>
public interface IDialogService
{
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
}

/// <summary>
/// WPF 다이얼로그 서비스 구현
/// WPF dialog service implementation
/// </summary>
public sealed class DialogService : IDialogService
{
    public Task ShowMessageAsync(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task ShowErrorAsync(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }
}
