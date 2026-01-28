# ViewModel Patterns

## Computed Properties

```csharp
public class OrderViewModel
{
    public ObservableValue<decimal> Price { get; } = new(0);
    public ObservableValue<int> Quantity { get; } = new(1);
    public ObservableValue<decimal> Subtotal { get; } = new(0);
    public ObservableValue<decimal> Tax { get; } = new(0);
    public ObservableValue<decimal> Total { get; } = new(0);
    public ObservableValue<decimal> TaxRate { get; } = new(0.08m);

    public OrderViewModel()
    {
        // Subscribe via Changed event
        Price.Changed += UpdateSubtotal;
        Quantity.Changed += UpdateSubtotal;
        Subtotal.Changed += UpdateTax;
        TaxRate.Changed += UpdateTax;
        Subtotal.Changed += UpdateTotal;
        Tax.Changed += UpdateTotal;
    }

    private void UpdateSubtotal() => Subtotal.Value = Price.Value * Quantity.Value;
    private void UpdateTax() => Tax.Value = Subtotal.Value * TaxRate.Value;
    private void UpdateTotal() => Total.Value = Subtotal.Value + Tax.Value;
}
```

## Form Validation

```csharp
public class FormViewModel
{
    public ObservableValue<string> Email { get; } = new("");
    public ObservableValue<string> Password { get; } = new("");
    public ObservableValue<bool> IsValid { get; } = new(false);
    public ObservableValue<string> ErrorMessage { get; } = new("");
    public ObservableValue<bool> CanSubmit { get; } = new(false);
    public ObservableValue<bool> IsLoading { get; } = new(false);

    public FormViewModel()
    {
        Email.Changed += Validate;
        Password.Changed += Validate;
        IsValid.Changed += UpdateCanSubmit;
        IsLoading.Changed += UpdateCanSubmit;
    }

    private void Validate()
    {
        var errors = new List<string>();
        if (!Email.Value.Contains("@")) errors.Add("Invalid email");
        if (Password.Value.Length < 8) errors.Add("Password too short");

        ErrorMessage.Value = string.Join(", ", errors);
        IsValid.Value = errors.Count == 0;
    }

    private void UpdateCanSubmit() => CanSubmit.Value = IsValid.Value && !IsLoading.Value;

    public async Task SubmitAsync()
    {
        if (!CanSubmit.Value) return;
        IsLoading.Value = true;
        try { await DoSubmit(); }
        finally { IsLoading.Value = false; }
    }
}
```

## Manual Collection Management

Note: MewUI does NOT have `ObservableCollection` or `ItemsSource`. Manage lists manually:

```csharp
public class ListViewModel
{
    private readonly List<string> _items = new();
    public ObservableValue<int> SelectedIndex { get; } = new(-1);
    public ObservableValue<string?> SelectedItem { get; } = new(null);

    public ListViewModel()
    {
        SelectedIndex.Changed += () => {
            var idx = SelectedIndex.Value;
            SelectedItem.Value = idx >= 0 && idx < _items.Count ? _items[idx] : null;
        };
    }

    public void Add(string item) => _items.Add(item);
    public void RemoveAt(int index) => _items.RemoveAt(index);
}

// Usage - use .Items() with strings, not ItemsSource
new ListBox()
    .Items("Item 1", "Item 2", "Item 3")
    .BindSelectedIndex(vm.SelectedIndex)

// For dynamic updates, clear and re-add:
listBox.ClearItems();
foreach (var item in newItems)
    listBox.AddItem(item);
```

## Implementing Custom Binding

```csharp
public class MyControl : Control
{
    private ValueBinding<double>? _valueBinding;  // Note: match actual control type
    private bool _updating;
    private double _value;

    public double Value
    {
        get => _value;
        set {
            if (_value != value) {
                _value = value;
                if (!_updating) _valueBinding?.Set(value);
                InvalidateVisual();
            }
        }
    }

    public void SetValueBinding(Func<double> get, Action<double> set,
        Action<Action>? subscribe, Action<Action>? unsubscribe)
    {
        _valueBinding?.Dispose();
        _valueBinding = new ValueBinding<double>(get, set, subscribe, unsubscribe,
            () => { _updating = true; Value = get(); _updating = false; });
        // Note: RegisterBinding is INTERNAL - manage disposal in OnDispose() instead
        Value = get();
    }

    protected override void OnDispose()
    {
        _valueBinding?.Dispose();
        base.OnDispose();
    }
}

// Extension - use lambda pattern for subscribe/unsubscribe
public static MyControl BindValue(this MyControl c, ObservableValue<double> src)
{
    c.SetValueBinding(
        () => src.Value,
        v => src.Value = v,
        h => src.Changed += h,    // Lambda pattern
        h => src.Changed -= h);   // Lambda pattern
    return c;
}
```
