namespace PolyLab3DStudio.Views;

public sealed partial class GuideView : UserControl
{
    public GuideView()
    {
        InitializeComponent();
    }

    private void OnPointCloudLinkClick(object sender, RoutedEventArgs e) =>
        PointCloudSection.BringIntoView();
}
