using System.Diagnostics;
using Microsoft.Win32;
using PolyLab3DStudio.Controls;
using PolyLab3DStudio.ViewModels;

namespace PolyLab3DStudio.Views;

public sealed partial class StudioView : UserControl
{
    private StudioViewModel? _vm;

    public StudioView()
    {
        InitializeComponent();

        Viewport.SelectRequested += id => _vm?.OnViewportSelect(id);
        Viewport.TransformCommitted += OnTransformCommitted;
        Viewport.ActionPerformed += name => _vm?.OnViewportAction(name);

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.ViewPresetRequested -= OnViewPresetRequested;
            _vm.CopyToClipboardRequested -= OnCopyToClipboardRequested;
            _vm.OpenFolderRequested -= OnOpenFolderRequested;
            _vm.PickFolderRequested = null;
        }

        _vm = DataContext as StudioViewModel;

        if (_vm is not null)
        {
            _vm.ViewPresetRequested += OnViewPresetRequested;
            _vm.CopyToClipboardRequested += OnCopyToClipboardRequested;
            _vm.OpenFolderRequested += OnOpenFolderRequested;
            _vm.PickFolderRequested = OnPickFolderRequested;
        }
    }

    private void OnTransformCommitted(TransformCommit commit) =>
        _vm?.OnViewportTransform(commit.Id, commit.X, commit.Y, commit.Z, commit.RotationYDeg, commit.Scale, commit.Mode, commit.Shift);

    private void OnViewPresetRequested(string preset) => Viewport.SetView(preset);

    private static void OnCopyToClipboardRequested(string text) => Clipboard.SetText(text);

    private static string? OnPickFolderRequested()
    {
        var dialog = new OpenFolderDialog { Title = "프로젝트를 만들 폴더 선택" };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private static void OnOpenFolderRequested(string path) => Process.Start("explorer.exe", path);
}
