namespace PolyLab3DStudio.Views;

public sealed partial class StartView : UserControl
{
    public StartView()
    {
        InitializeComponent();

        // Border.CornerRadius does not clip children; clip the demo viewport manually.
        DemoClip.SizeChanged += (_, e) => DemoClip.Clip = new RectangleGeometry(
            new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), 18, 18);
    }
}
