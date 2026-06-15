# Building a Provider Settings Panel in WPF

> Builds a settings surface that lets the user choose an LLM provider, model id, optional base URL, system prompt, and API key, where the fields differ per provider — the base URL is only meaningful for some providers, and the example model ids differ. Driven by live configuration through `IOptionsMonitor<T>`, per-provider conditional visibility, and converter-driven per-provider placeholder examples, while the API key is held by the OS credential store rather than in the settings file. The same value-precedence discipline that governs role styling applies to derived UI: raise `PropertyChanged` for derived properties (for example `ShowBaseUrl`) inside the provider setter so the panel re-evaluates which fields and placeholders to show the instant the provider switches.

A real chat or agent application rarely talks to a single provider. The settings panel is where the user picks the provider and supplies the per-provider details that the rest of the app depends on. The challenge is that those details are not uniform: a hosted provider needs only a model id and a key, a self-hosted or proxy provider also needs a base URL, and a mock provider needs almost nothing. The panel must show only the fields that make sense for the selected provider and must offer per-provider example text so the user is never left guessing what a valid value looks like.

---

## 1. `IOptionsMonitor<T>` Consumption

Bind the panel to live configuration so external edits (an `appsettings.json` change, a hot reload, a settings import) flow into the UI without a restart. The view model takes `IOptionsMonitor<T>` from DI, reads `CurrentValue` to seed its editable fields, and subscribes to `OnChange` to refresh them when the underlying configuration changes.

- Read `IOptionsMonitor<T>.CurrentValue` once at construction to populate the editable properties.
- Subscribe to `OnChange` to re-seed the editable properties when configuration is reloaded externally.
- Keep the API key out of the options object — it is read from and written to the OS credential store, never persisted alongside the rest of the settings (see §4).

The panel edits its own observable copy of the values rather than mutating `CurrentValue` directly; persisting the edited values back is a separate save step.

---

## 2. Per-Provider Derived Visibility

Whether a field is shown is a property *derived* from the selected provider, not a stored flag. Because the derived property has no backing field of its own, the provider setter must explicitly notify that the derived property changed — otherwise the UI keeps the visibility it computed for the previous provider.

```csharp
// VM: derived visibility + per-provider placeholder, re-raised on provider change.
public Provider Provider
{
    get => _provider;
    set { if (SetProperty(ref _provider, value)) { OnPropertyChanged(nameof(ShowBaseUrl)); } }
}
public bool ShowBaseUrl => Provider is not Provider.Mock; // or a per-provider rule
```

`SetProperty` raises `PropertyChanged` for `Provider` itself; the extra `OnPropertyChanged(nameof(ShowBaseUrl))` inside the setter is what forces the UI to re-evaluate the derived visibility when the provider switches. Add one `OnPropertyChanged` line per derived property that depends on the provider.

---

## 3. Converter-Driven Per-Provider Placeholders (XAML)

Conditional visibility is the first half; per-provider example text is the second. A converter turns the selected provider into the placeholder string that fits it, so the base URL field shows the example base URL for the current provider and the model id field shows an example model id for the current provider. Binding `Provider` through the converter means the placeholders refresh automatically on the same `PropertyChanged` notification that drives visibility.

```xml
<!-- Base URL section only where meaningful; placeholder is a per-provider example via a converter -->
<StackPanel Visibility="{Binding ShowBaseUrl, Converter={StaticResource BoolToVisibility}}">
    <ui:TextBox Text="{Binding BaseUrl, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="{Binding Provider, Converter={StaticResource ProviderBaseUrlExampleConverter}}"/>
</StackPanel>
<!-- Model id field uses an analogous ProviderModelExampleConverter for its placeholder. -->
```

`BoolToVisibility` drives the section's visibility from `ShowBaseUrl`; `ProviderBaseUrlExampleConverter` maps the selected `Provider` to a sample base URL for the `PlaceholderText`. The model id field uses an analogous `ProviderModelExampleConverter` for its own placeholder. For the converter authoring pattern (`IValueConverter` plus the `MarkupExtension` self-provision), see [`using-converter-markup-extension`](../using-converter-markup-extension/TOPIC.md).

---

## 4. The Credential-Store Key Field

The API key is the one field that does not belong in the options object or the settings file. It is read from and written to the OS credential store, and the entry control is a `PasswordBox` rather than a plain `TextBox` so the secret never sits in a bindable plain-text property. The key field still lives in the same panel, but its load and save path is the credential store — see [`storing-api-keys-and-binding-passwordbox-in-wpf`](../storing-api-keys-and-binding-passwordbox-in-wpf/TOPIC.md) for the full pattern.

Per-field validation (required model id, well-formed base URL when shown) belongs to the same panel; see [`implementing-wpf-validation`](../implementing-wpf-validation/TOPIC.md).

---

## References

- [Options pattern in .NET — Microsoft Learn](https://learn.microsoft.com/dotnet/core/extensions/options)
- [IOptionsMonitor&lt;TOptions&gt; Interface — Microsoft Learn](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.ioptionsmonitor-1)
- [Options pattern guidance for .NET library authors — Microsoft Learn](https://learn.microsoft.com/dotnet/core/extensions/options-library-authors)

### Related topics

- [`using-converter-markup-extension`](../using-converter-markup-extension/TOPIC.md) — the per-provider placeholder converters (`ProviderBaseUrlExampleConverter`, `ProviderModelExampleConverter`) and `BoolToVisibility`
- [`implementing-wpf-validation`](../implementing-wpf-validation/TOPIC.md) — per-field validation for the model id and base URL fields
- [`storing-api-keys-and-binding-passwordbox-in-wpf`](../storing-api-keys-and-binding-passwordbox-in-wpf/TOPIC.md) — the OS credential-store-backed API key field
