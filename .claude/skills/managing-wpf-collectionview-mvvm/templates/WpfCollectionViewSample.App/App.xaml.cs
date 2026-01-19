namespace WpfCollectionViewSample.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Service Layer (WPF 참조 가능)
                // Service Layer (can reference WPF)
                services.AddSingleton<IMemberCollectionService, MemberCollectionService>();

                // ViewModels (순수 BCL 타입만 사용)
                // ViewModels (uses pure BCL types only)
                services.AddTransient<MainViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
