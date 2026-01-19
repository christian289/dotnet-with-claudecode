namespace GameDataTool.WpfApp.Views;

/// <summary>
/// 메인 윈도우 - Constructor Injection을 통한 ViewModel 주입
/// Main window - ViewModel injection through Constructor Injection
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // 윈도우 로드 시 데이터 로드
        // Load data when window loads
        Loaded += async (_, _) => await viewModel.LoadUsersCommand.ExecuteAsync(null);
    }
}
