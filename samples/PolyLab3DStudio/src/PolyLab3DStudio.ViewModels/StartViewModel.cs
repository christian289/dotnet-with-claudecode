using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>The start screen: hero copy, CTA buttons, and the auto-spinning demo scene.</summary>
public sealed partial class StartViewModel(ShellViewModel shell) : ObservableObject
{
    public ShellViewModel Shell { get; } = shell;

    /// <summary>Snowman demo scene rendered by the right-hand viewport.</summary>
    public IReadOnlyList<SceneObjectViewModel> DemoObjects { get; } =
        [.. SceneSeeds.Demo.Select(s => new SceneObjectViewModel(s))];

    [RelayCommand]
    private void Continue() => Shell.StartContinue();

    [RelayCommand]
    private void GoCourses() => Shell.GoCourses();

    [RelayCommand]
    private void GoStudioFree() => Shell.GoStudioFree();

    [RelayCommand]
    private void GoSettings() => Shell.GoSettings();

    [RelayCommand]
    private void GoGuide() => Shell.GoGuide();

    [RelayCommand]
    private void GoPipeline() => Shell.GoPipeline();

    [RelayCommand]
    private void GoDict() => Shell.GoDict();

    [RelayCommand]
    private void GoToolMap() => Shell.GoToolMap();
}
