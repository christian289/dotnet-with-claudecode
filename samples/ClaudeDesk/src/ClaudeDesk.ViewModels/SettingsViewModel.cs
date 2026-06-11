using ClaudeDesk.Abstractions;

namespace ClaudeDesk.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ChatSettingsStore _settingsStore;
    private readonly IApiKeyStore _keyStore;

    public IReadOnlyList<Provider> Providers { get; } = [Provider.Mock, Provider.Anthropic];

    [NotifyPropertyChangedFor(nameof(ShowApiKey))]
    [NotifyPropertyChangedFor(nameof(ShowBaseUrl))]
    [ObservableProperty] private Provider _provider;

    [ObservableProperty] private string _modelId = string.Empty;
    [ObservableProperty] private string? _baseUrl;

    // Pushed in by the PasswordBox attached behavior (OneWayToSource);
    // written to the OS credential store on Save, never to config files.
    [ObservableProperty] private string _apiKey = string.Empty;

    [ObservableProperty] private string _status = string.Empty;

    public bool ShowApiKey => Provider is not Provider.Mock;
    public bool ShowBaseUrl => Provider is not Provider.Mock;

    public SettingsViewModel(ChatSettingsStore settingsStore, IApiKeyStore keyStore)
    {
        _settingsStore = settingsStore;
        _keyStore = keyStore;

        ChatSettings current = settingsStore.Current;
        Provider = current.Provider;
        ModelId = current.ModelId;
        BaseUrl = current.BaseUrl;
        // The stored API key is intentionally NOT loaded back into the UI.
    }

    [RelayCommand]
    private void Save()
    {
        _settingsStore.Current = new ChatSettings
        {
            Provider = Provider,
            ModelId = ModelId,
            BaseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? null : BaseUrl,
        };

        if (!string.IsNullOrEmpty(ApiKey))
        {
            _keyStore.Save(CredentialNames.ApiKey, ApiKey);
            ApiKey = string.Empty; // do not keep the secret in bound state
        }

        Status = "Saved.";
    }
}
