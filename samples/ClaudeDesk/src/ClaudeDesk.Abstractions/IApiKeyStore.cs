namespace ClaudeDesk.Abstractions;

/// <summary>
/// Secrets live in the OS credential store — never in settings/config files.
/// </summary>
public interface IApiKeyStore
{
    void Save(string key, string secret);
    string? TryGet(string key);
    void Delete(string key);
}
