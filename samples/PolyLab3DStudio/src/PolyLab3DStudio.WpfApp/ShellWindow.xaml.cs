using PolyLab3DStudio.Core;
using PolyLab3DStudio.ViewModels;

namespace PolyLab3DStudio;

public sealed partial class ShellWindow : Window
{
    private readonly ShellViewModel _shell;

    public ShellWindow(ShellViewModel shell)
    {
        InitializeComponent();
        _shell = shell;
        DataContext = shell;
    }

    /// <summary>Design's global key map: Ctrl+Z/Ctrl+Shift+Z anywhere, tools in the studio.</summary>
    private void OnShellPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.OriginalSource is TextBoxBase or PasswordBox)
        {
            return;
        }

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Z)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                _shell.Studio.RedoCommand.Execute(null);
            }
            else
            {
                _shell.Studio.UndoCommand.Execute(null);
            }

            e.Handled = true;
            return;
        }

        if (!_shell.IsStudioActive)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Q:
                _shell.Studio.SetToolCommand.Execute(ToolCatalog.Select);
                break;
            case Key.W:
                _shell.Studio.SetToolCommand.Execute(ToolCatalog.Move);
                break;
            case Key.E:
                _shell.Studio.SetToolCommand.Execute(ToolCatalog.Rotate);
                break;
            case Key.R:
                _shell.Studio.SetToolCommand.Execute(ToolCatalog.Scale);
                break;
            case Key.Delete:
            case Key.Back:
                _shell.Studio.DeleteSelectedCommand.Execute(null);
                break;
            case Key.Escape:
                _shell.Studio.ClearSelection();
                break;
        }
    }
}
