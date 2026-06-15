# Building Runtime-Swappable WPF Themes

> Builds a complete, self-contained, hot-swappable WPF theme as one `ResourceDictionary` that restyles every stock control, plus the shell-brush-key contract and the runtime dictionary swap. Use when building an application theme, shipping multiple themes / a theme switcher / a theme gallery, or when a swapped theme only partially applies. This is the theme-dictionary layer on top of per-control animated templates — for the animated `ControlTemplate` bodies and animation-safety rules see `animating-wpf-controltemplates`, and for the swap mechanism see `managing-styles-resourcedictionary` §4.3.

A "theme" here is a single `ResourceDictionary` the app swaps at runtime to re-skin the whole UI. This topic covers the **theme-level contract** (self-containment, the shell-brush keys, the full implicit-style roster, the swap). It does not repeat how to author or animate an individual control template — that is `animating-wpf-controltemplates`. `samples/ThemeGallery` (10 themes) is the reference implementation; its `THEME-CONTRACT.md` is the authoring checklist.

---

## 1. A theme is ONE self-contained ResourceDictionary

Because the gallery/app hot-swaps the **entire** dictionary at runtime, each theme file must stand alone:

1. **No `MergedDictionaries`** inside a theme file — it is the unit that gets added/removed; merging others breaks clean removal.
2. **No external assets** — no image/font files, no code-behind, no converters, no custom markup extensions. Draw every visual with XAML primitives (`LinearGradientBrush`/`RadialGradientBrush`, `Geometry`, `Path`, shapes inside templates). This keeps a theme a single portable file with no load-order or packaging dependencies.
3. **Root element**: `<ResourceDictionary>` with only the standard `presentation` + `x` namespaces.
4. **Resource order is top-down** (palette `Color`s → shared brushes → keyed helper styles → control styles) so no `{StaticResource}` is a forward reference (see `animating-wpf-controltemplates` Essential rule 3 — a forward ref compiles then throws at instantiation).

---

## 2. Shell-brush key contract

Every theme MUST define the same set of shell brushes so app chrome can bind them uniformly and they re-resolve on swap. Consume them via **`{DynamicResource}`** (never `{StaticResource}` — a static reference freezes the first theme and won't follow a swap).

| Key | Role |
|-----|------|
| `ThemeWindowBackgroundBrush` | window / root background (gradients encouraged) |
| `ThemeTextBrush` | primary foreground |
| `ThemeSubtleTextBrush` | secondary / muted foreground |
| `ThemeAccentBrush` | accent / highlight |
| `ThemeCardBrush` | panel / surface background |
| `ThemeBorderBrush` | borders / dividers |

```xml
<!-- App chrome binds the contract via DynamicResource so a swap updates it live -->
<Window Background="{DynamicResource ThemeWindowBackgroundBrush}" ... />
<TextBlock Foreground="{DynamicResource ThemeTextBrush}" ... />
```

A theme adds whatever extra palette/brush keys it needs, but the six above are the fixed surface every theme provides.

---

## 3. Full implicit-style roster

A theme provides **implicit** (keyless, `TargetType`-only) styles — full `ControlTemplate`s, not mere property setters — for every stock control it re-skins, so controls restyle automatically on swap with no per-instance bindings. The roster the reference themes cover:

`Button`, `ToggleButton`, `CheckBox`, `RadioButton`, `TextBox`, `ComboBox`, `ComboBoxItem`, `Slider`, `ProgressBar`, `ScrollBar`, `TabControl`, `TabItem`, `ListBox`, `ListBoxItem`, `Expander`.

Each interactive control:
- reproduces WPF's required named parts (`PART_ContentHost`, `PART_Track`/`PART_Indicator`, the `Track` + `RepeatButton`s + `Thumb`, `PART_Popup` + `IsDropDownOpen`-bound toggle, `IsExpanded`-bound header, `ContentPresenter ContentSource="SelectedContent"`) — catalog in `animating-wpf-controltemplates` §4;
- animates at least its hover AND its press/check/select/focus transition with eased Storyboards, plus one looping **signature ambient** animation that gives the theme its identity;
- handles `IsEnabled="False"` visually and keeps a keyboard focus cue.

Author these bodies per `animating-wpf-controltemplates` (Triggers + Storyboards, the animation-safety rules, the runtime-only pitfalls). Factor `Thumb`/`RepeatButton` into keyed helper styles reused by `Slider` and `ScrollBar`.

---

## 4. Runtime swapping

The swap itself (remove the previous theme dictionary, add the new one to `Application.Current.Resources.MergedDictionaries`) is owned by `managing-styles-resourcedictionary` §4.3. Keep a P-002-clean abstraction so a WPF-free ViewModel can request a switch:

```csharp
// IThemeSwitcher lives in the ViewModels layer (no System.Windows); the WPF
// implementation lives in the app layer.
public void Apply(string themeName)
{
    var dict = new ResourceDictionary
    {
        Source = new Uri($"pack://application:,,,/MyApp.Themes;component/{themeName}.xaml", UriKind.Absolute),
    };
    var merged = Application.Current.Resources.MergedDictionaries;
    if (_current is not null) { merged.Remove(_current); }
    merged.Add(dict);
    _current = dict;
}
```

Implicit styles re-apply automatically on swap; `DynamicResource` shell brushes re-resolve. For an animated cross-fade instead of an instant swap, see `animating-wpf-controltemplates` §7 (and its caveat: a held animation pins a property across a swap).

---

## 5. Verify by switching against live controls

A theme can compile (`dotnet build` → BAML) and even load in isolation, yet crash on swap, because neither step instantiates the control templates. Build a throwaway window with one of every templated control and cycle all themes through the real switch path, capturing sync + `DispatcherUnhandledException` errors — this surfaces the runtime-only failures (`StaticResourceHolder` forward refs, `Setter`-on-Freezable, `(UIElement.Children)[n]` paths). Full harness in `animating-wpf-controltemplates` §8.

---

## References

- [Merged resource dictionaries — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/systems/xaml-resources-merged-dictionaries)
- [XAML resources overview — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/systems/xaml-resources-overview)

### Related topics

- [`animating-wpf-controltemplates`](../animating-wpf-controltemplates/TOPIC.md) — the per-control animated `ControlTemplate` bodies, required `PART_` catalog, animation-safety rules, runtime-only pitfalls, and the verify-at-runtime harness this contract relies on.
- [`managing-styles-resourcedictionary`](../managing-styles-resourcedictionary/TOPIC.md) — `ResourceDictionary` layout, `StaticResource` vs `DynamicResource`, and the canonical MergedDictionaries theme-swap (§4.3).
- [`designing-wpf-customcontrol-architecture`](../designing-wpf-customcontrol-architecture/TOPIC.md) — when a theme also ships custom controls (Generic.xaml hub, per-control files).
- [`containing-control-decorative-overflow`](../containing-control-decorative-overflow/TOPIC.md) — keeping a theme's animated glow / focus ring from being clipped.
- [`setting-up-flaui-ui-tests`](../setting-up-flaui-ui-tests/TOPIC.md) — UI-automating a themed app; AutomationId discovery survives restyling.
