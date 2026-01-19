namespace GameDataTool.WpfApp;

using Microsoft.EntityFrameworkCore;
using GameDataTool.WpfApp.Views;

/// <summary>
/// Composition Root - DI 컨테이너 설정 및 애플리케이션 진입점
/// Composition Root - DI container configuration and application entry point
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ══════════════ Domain Layer ══════════════
                // 등록 불필요 (순수 모델)
                // No registration needed (pure models)

                // ══════════════ Application Layer ══════════════
                services.AddTransient<UserService>();

                // ══════════════ Infrastructure Layer ══════════════
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite("Data Source=GameDataTool.db"));
                services.AddScoped<IUserRepository, UserRepository>();

                // ══════════════ Presentation Layer ══════════════
                // ViewModels
                services.AddTransient<MainViewModel>();

                // WPF Services
                services.AddSingleton<IDialogService, DialogService>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // 데이터베이스 마이그레이션 (개발용)
        // Database migration (for development)
        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        // 메인 윈도우 표시
        // Show main window
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
