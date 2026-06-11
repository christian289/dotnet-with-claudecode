using ClaudeDesk.Abstractions;
using ClaudeDesk.Core;
using ClaudeDesk.Services;

namespace ClaudeDesk;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Shared HttpClient pool — singleton is part of the pattern's correctness.
                services.AddSingleton<IChatClientFactory, ChatClientFactory>();
                services.AddSingleton<IUiDispatcher, WpfUiDispatcher>();
                services.AddSingleton<IApiKeyStore, CredentialManagerApiKeyStore>();
                services.AddSingleton<ChatSettingsStore>(); // Provider.Mock by default: runs keyless

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<ChatViewModel>();
                services.AddSingleton<SettingsViewModel>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
