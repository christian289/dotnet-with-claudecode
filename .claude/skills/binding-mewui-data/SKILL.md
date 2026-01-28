---
name: binding-mewui-data
description: "Binds MewUI controls to data using ObservableValue and ValueBinding. Use when implementing reactive UI updates, building ViewModels, or connecting controls to data sources."
---

## ObservableValue<T>

Reactive value container:

```csharp
// Create
var name = new ObservableValue<string>("Initial");
var count = new ObservableValue<int>(0);
var enabled = new ObservableValue<bool>(true);

// Read/Write
string current = name.Value;
name.Value = "New";  // Triggers Changed event

// Subscribe via Changed event
name.Changed += () => Console.WriteLine($"Changed to: {name.Value}");

// With coercion (value constraint)
var percent = new ObservableValue<double>(50, coerce: v => Math.Clamp(v, 0, 100));
percent.Value = 150;  // Becomes 100
```

---

## Fluent Binding

```csharp
var vm = new MyViewModel();

new StackPanel().Children(
    // One-way (source → UI)
    new Label().BindText(vm.Message),

    // Two-way (source ↔ UI)
    new TextBox().BindText(vm.Name),
    new CheckBox().BindIsChecked(vm.IsEnabled),
    new Slider().BindValue(vm.Volume),

    // With converter
    new Label().BindText(vm.Count, c => $"Count: {c}"),

    // Common bindings
    new Button().BindIsEnabled(vm.CanSubmit).BindIsVisible(vm.ShowButton)
)
```

---

## ViewModel Pattern

```csharp
public class PersonViewModel
{
    public ObservableValue<string> FirstName { get; } = new("");
    public ObservableValue<string> LastName { get; } = new("");
    public ObservableValue<string> FullName { get; } = new("");
    public ObservableValue<bool> IsValid { get; } = new(false);

    public PersonViewModel()
    {
        FirstName.Changed += Update;
        LastName.Changed += Update;
    }

    private void Update()
    {
        FullName.Value = $"{FirstName.Value} {LastName.Value}".Trim();
        IsValid.Value = FirstName.Value.Length > 0 && LastName.Value.Length > 0;
    }
}
```

---

## ValueBinding<T> (Low-level)

```csharp
// Note: subscribe/unsubscribe use lambda pattern, not method group
var binding = new ValueBinding<string>(
    get: () => source.Value,
    set: v => source.Value = v,  // null for one-way
    subscribe: h => source.Changed += h,
    unsubscribe: h => source.Changed -= h,
    onSourceChanged: () => control.Text = source.Value
);
```

---

## Preventing Binding Loops

```csharp
private bool _suppressBindingSet;

public string Text
{
    get => _text;
    set {
        if (_text != value) {
            _text = value;
            if (!_suppressBindingSet) _binding?.Set(value);
            InvalidateMeasure();
        }
    }
}

private void SetTextFromSource(string value)
{
    _suppressBindingSet = true;
    try { Text = value; }
    finally { _suppressBindingSet = false; }
}
```

---

**Advanced patterns**: See [viewmodel-patterns.md](viewmodel-patterns.md)
