using System.Diagnostics;
using System.Windows.Documents;

namespace PolyLab3DStudio.Views;

public sealed partial class DictView : UserControl
{
    public DictView()
    {
        InitializeComponent();
    }

    private void OnDocLinkClick(object sender, RoutedEventArgs e)
    {
        if (sender is Hyperlink { Tag: string url } && url.Length > 0)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
