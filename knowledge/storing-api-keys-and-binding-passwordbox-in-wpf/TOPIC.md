# Storing API Keys and Binding PasswordBox in WPF

> Stores API keys and secrets in the OS credential store (e.g. Windows Credential Manager via `CredWrite`/`CredRead`/`CredDelete`) keyed per provider, never written to `settings.json` or any config file in plaintext, and surfaces a `PasswordBox` for key entry through an attached behavior because `PasswordBox.Password` is deliberately NOT a `DependencyProperty`. Use when a WPF app accepts an API key, token, or other secret, when a `{Binding}` against `PasswordBox.Password` silently does nothing, or when designing the `IApiKeyStore` contract behind a provider settings panel. Covers the per-provider credential-store contract, the not-a-DP fact, the `PasswordBoxAssistant` one-way-out attached behavior with idempotent re-attach, and the bindable-password kit-control alternative — but never round-trips the secret through config.

Storing API keys securely in the OS credential store and surfacing a non-bindable `PasswordBox` for secret entry in WPF.

---

## 1. Storage: the `IApiKeyStore` Contract (OS Credential Store)

API keys and secrets belong in the OS credential store, not in app settings. Writing a key to `settings.json` or any config file persists it to disk in plaintext, where any user or process with file access can read it. Keep the secret in the OS credential store — on Windows, that is Windows Credential Manager, reachable through the Win32 `CredWrite` / `CredRead` / `CredDelete` APIs.

Abstract the store behind a contract, keyed per provider, so callers never touch a config file:

```csharp
// Storage: keyed per provider, never settings.json.
public interface IApiKeyStore
{
    void Save(string key, string secret);  // e.g. Win32 CredWrite
    string? TryGet(string key);             // CredRead
    void Delete(string key);                // CredDelete
}
```

- `Save` upserts the secret under a per-provider `key` (e.g. `"MyApp:OpenAI"`).
- `TryGet` returns `null` when no credential exists for that key — callers branch on the absence instead of catching.
- `Delete` removes the credential when the user clears or rotates the key.

The `key` identifies the credential slot; the `secret` is the value that must never reach disk in plaintext.

---

## 2. Binding: `PasswordBox.Password` Is Not a DependencyProperty

`PasswordBox.Password` is deliberately **not** a `DependencyProperty`. WPF makes this choice on purpose — exposing the secret as a DP would let it live inside the binding / DP system (and its diagnostic surfaces) in memory. The practical consequence: `PasswordBox.Password` cannot be a binding target, so `{Binding}` against it silently does nothing — no exception, no warning, just a key field that never propagates the entered value.

To surface the entered value, use an attached behavior that pushes it out one-way (out via `PasswordChanged`):

```csharp
// Binding: PasswordBox.Password is NOT a DP. Attached behavior to surface it (one-way out):
public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
        "BoundPassword", typeof(string), typeof(PasswordBoxAssistant),
        new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));
    public static string GetBoundPassword(DependencyObject o) => (string)o.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject o, string v) => o.SetValue(BoundPasswordProperty, v);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) { return; }
        pb.PasswordChanged -= Handler; // idempotent re-attach (guards re-entrancy)
        pb.PasswordChanged += Handler;
        static void Handler(object s, RoutedEventArgs _)
        {
            var box = (PasswordBox)s;
            SetBoundPassword(box, box.Password); // push entered value back to the bound property
        }
    }
}
```

- The attached `BoundPassword` property *is* a `DependencyProperty` (via `RegisterAttached`), so it can be the binding target the `Password` property cannot be.
- The re-attach `pb.PasswordChanged -= Handler; pb.PasswordChanged += Handler;` is idempotent — detaching before attaching guards against double subscription and re-entrancy when the property changes more than once.
- `Handler` is a static local function so it is a stable delegate target, which is what makes the `-=` actually remove the prior subscription.
- This is a one-way-out flow: the user's keystrokes raise `PasswordChanged`, which writes back to `BoundPassword`. Do not try to round-trip the secret in (and never push it back through config).

---

## 3. Kit-Control Alternative

If the UI kit in use provides a `PasswordBox` whose `Password` *is* bindable, prefer it and skip the attached behavior entirely — let the control's own bindable password property carry the value.

> If the UI kit provides a `PasswordBox` whose `Password` *is* bindable, prefer it and skip the behavior — but never round-trip the secret through `settings.json`.

Either way — attached behavior or kit control — the bound value flows into the `IApiKeyStore` from Section 1; it does not flow into config.

---

## References

- [PasswordBox Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.controls.passwordbox)
- [PasswordBox.Password Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.controls.passwordbox.password)
- [PasswordBox.PasswordChanged Event — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.controls.passwordbox.passwordchanged)
- [DependencyProperty.RegisterAttached Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.dependencyproperty.registerattached)
- [CredWrite function (wincred.h) — Microsoft Learn](https://learn.microsoft.com/windows/win32/api/wincred/nf-wincred-credwritew)
- [CredRead function (wincred.h) — Microsoft Learn](https://learn.microsoft.com/windows/win32/api/wincred/nf-wincred-credreadw)
- [CredDelete function (wincred.h) — Microsoft Learn](https://learn.microsoft.com/windows/win32/api/wincred/nf-wincred-creddeletew)

### Related topics

- [`implementing-wpf-validation`](../implementing-wpf-validation/TOPIC.md) — validating the API-key field as form input
- [`building-a-provider-settings-panel`](../building-a-provider-settings-panel/TOPIC.md) — the settings panel that hosts the key field
