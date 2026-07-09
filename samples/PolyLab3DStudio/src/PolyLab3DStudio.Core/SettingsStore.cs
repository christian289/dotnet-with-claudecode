namespace PolyLab3DStudio.Core;

/// <summary>
/// JSON settings persistence under %APPDATA%/PolyLab3DStudio (the design used
/// localStorage). Load/save are best-effort: a broken or missing file just
/// yields defaults, mirroring the design's try/catch behavior.
/// </summary>
public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PolyLab3DStudio",
        "settings.json");

    public UserSettings Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                return JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(_path)) ?? new UserSettings();
            }
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
        }

        return new UserSettings();
    }

    public void Save(UserSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}
