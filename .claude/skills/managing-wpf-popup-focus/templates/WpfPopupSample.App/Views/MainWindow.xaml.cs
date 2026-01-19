namespace WpfPopupSample.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Popup이 정상 동작하도록 포커스 관리
        // Manage focus to ensure Popup works correctly
        PreviewMouseDown += MainWindow_PreviewMouseDown;
    }

    private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // 키보드 포커스가 없으면 윈도우 활성화
        // Activate window if keyboard focus is lost
        if (!IsKeyboardFocused)
        {
            Activate();
        }
    }

    private void ShowPopupButton_Click(object sender, RoutedEventArgs e)
    {
        popup.IsOpen = true;
    }

    private void ClosePopup_Click(object sender, RoutedEventArgs e)
    {
        popup.IsOpen = false;
    }
}
