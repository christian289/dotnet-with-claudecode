namespace PolyLab3DStudio.ViewModels;

/// <summary>The settings screen; values live on the shell and persist immediately.</summary>
public sealed partial class SettingsViewModel(ShellViewModel shell) : ObservableObject
{
    public ShellViewModel Shell { get; } = shell;

    [RelayCommand]
    private void Back() => Shell.SettingsBack();

    [RelayCommand]
    private void ResetProgress() => Shell.ResetProgress();
}
