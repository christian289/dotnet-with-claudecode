namespace PolyLab3DStudio.Core;

/// <summary>
/// Writes a complete, immediately runnable WPF project (csproj + App +
/// MainWindow pair) for the exported scene into a fresh
/// <c>PolyLabScene</c> subdirectory of the chosen folder.
/// </summary>
public static class WpfProjectExporter
{
    public static string Export(
        string baseDirectory,
        string targetFramework,
        string mainWindowXaml,
        string mainWindowCs)
    {
        string directory = Path.Combine(baseDirectory, "PolyLabScene");
        for (int suffix = 2; Directory.Exists(directory); suffix++)
        {
            directory = Path.Combine(baseDirectory, $"PolyLabScene{suffix}");
        }

        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "PolyLabScene.csproj"), WpfSceneCodeGenerator.GenerateProjectFile(targetFramework));
        File.WriteAllText(Path.Combine(directory, "App.xaml"), WpfSceneCodeGenerator.GenerateAppXaml());
        File.WriteAllText(Path.Combine(directory, "App.xaml.cs"), WpfSceneCodeGenerator.GenerateAppCs());
        File.WriteAllText(Path.Combine(directory, "MainWindow.xaml"), mainWindowXaml);
        File.WriteAllText(Path.Combine(directory, "MainWindow.xaml.cs"), mainWindowCs);
        File.WriteAllText(Path.Combine(directory, "PolyLabScene.slnx"), WpfSceneCodeGenerator.GenerateSolutionFile());
        return directory;
    }
}
