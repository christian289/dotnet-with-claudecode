using PolyLab3DStudio.Core;
using PolyLab3DStudio.ViewModels;

namespace PolyLab3DStudio;

public sealed partial class App : Application
{
    private readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddSingleton<SettingsStore>();
            services.AddSingleton<ProgressStore>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<ShellWindow>();
        })
        .Build();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host.Start();

        MainWindow = _host.Services.GetRequiredService<ShellWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();

        base.OnExit(e);
    }
}
