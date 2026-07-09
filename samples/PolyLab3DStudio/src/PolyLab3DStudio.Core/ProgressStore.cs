namespace PolyLab3DStudio.Core;

/// <summary>
/// Lesson completion persistence (lesson key → done) under
/// %APPDATA%/PolyLab3DStudio, best-effort like <see cref="SettingsStore"/>.
/// </summary>
public sealed class ProgressStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PolyLab3DStudio",
        "progress.json");

    public Dictionary<string, bool> Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                return JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(_path)) ?? [];
            }
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
        }

        return [];
    }

    public void Save(IReadOnlyDictionary<string, bool> progress)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(progress, Options));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    public void Reset()
    {
        try
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}
