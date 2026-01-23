---
name: implementing-wpf-validation
description: "Implements WPF data validation using ValidationRule, IDataErrorInfo, and INotifyDataErrorInfo. Use when building forms, validating user input, or displaying validation errors in UI."
---

# WPF Data Validation

## 1. Validation Approaches

| Approach | Location | Pros | Cons |
|----------|----------|------|------|
| `ValidationRule` | XAML (Binding) | Simple, declarative XAML | Hard to separate from ViewModel |
| `IDataErrorInfo` | ViewModel | ViewModel integration | Synchronous validation only |
| `INotifyDataErrorInfo` | ViewModel | Async support, multiple errors | Complex implementation |
| `ExceptionValidationRule` | XAML | Exception-based | Potential performance impact |

---

## 2. ValidationRule

### 2.1 Custom ValidationRule

```csharp
public sealed partial class EmailValidationRule : ValidationRule
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string email || string.IsNullOrWhiteSpace(email))
        {
            return new ValidationResult(false, "Please enter an email address.");
        }

        if (!EmailPattern().IsMatch(email))
        {
            return new ValidationResult(false, "Invalid email format.");
        }

        return ValidationResult.ValidResult;
    }
}
```

> **Note**: Uses `GeneratedRegexAttribute` for compile-time regex. See `using-generated-regex` skill.

### 2.2 XAML Usage

```xml
<TextBox>
    <TextBox.Text>
        <Binding Path="Email" UpdateSourceTrigger="PropertyChanged">
            <Binding.ValidationRules>
                <local:EmailValidationRule ValidatesOnTargetUpdated="True"/>
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```

### 2.3 Error Template

```xml
<Style TargetType="TextBox">
    <Setter Property="Validation.ErrorTemplate">
        <Setter.Value>
            <ControlTemplate>
                <DockPanel>
                    <TextBlock DockPanel.Dock="Right" Foreground="Red" Text="!"
                               FontWeight="Bold" Margin="5,0"/>
                    <Border BorderBrush="Red" BorderThickness="1">
                        <AdornedElementPlaceholder/>
                    </Border>
                </DockPanel>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    <Style.Triggers>
        <Trigger Property="Validation.HasError" Value="True">
            <Setter Property="ToolTip"
                    Value="{Binding RelativeSource={RelativeSource Self},
                           Path=(Validation.Errors)[0].ErrorContent}"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

---

## 3. IDataErrorInfo

### 3.1 Implementation

```csharp
public partial class UserViewModel : ObservableObject, IDataErrorInfo
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _age;

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(Name) when string.IsNullOrWhiteSpace(Name) =>
                    "Please enter a name.",
                nameof(Name) when Name.Length < 2 =>
                    "Name must be at least 2 characters.",
                nameof(Age) when Age < 0 || Age > 150 =>
                    "Please enter a valid age.",
                _ => string.Empty
            };
        }
    }
}
```

### 3.2 XAML Binding

```xml
<TextBox Text="{Binding Name,
                        UpdateSourceTrigger=PropertyChanged,
                        ValidatesOnDataErrors=True}"/>
```

---

## 4. INotifyDataErrorInfo (Recommended)

### 4.1 Base Implementation

```csharp
public abstract partial class ValidatableViewModelBase : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.SelectMany(e => e.Value);
        }

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
        {
            _errors[propertyName] = [];
        }

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    protected void ClearAllErrors()
    {
        var properties = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var prop in properties)
        {
            OnErrorsChanged(prop);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }
}
```

### 4.2 ViewModel with Validation

```csharp
public partial class RegistrationViewModel : ValidatableViewModelBase
{
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    partial void OnEmailChanged(string value)
    {
        ValidateEmail();
    }

    partial void OnPasswordChanged(string value)
    {
        ValidatePassword();
        ValidateConfirmPassword();
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        ValidateConfirmPassword();
    }

    private void ValidateEmail()
    {
        ClearErrors(nameof(Email));

        if (string.IsNullOrWhiteSpace(Email))
        {
            AddError(nameof(Email), "Please enter an email address.");
        }
        else if (!Email.Contains('@'))
        {
            AddError(nameof(Email), "Invalid email format.");
        }
    }

    private void ValidatePassword()
    {
        ClearErrors(nameof(Password));

        if (Password.Length < 8)
        {
            AddError(nameof(Password), "Password must be at least 8 characters.");
        }

        if (!Password.Any(char.IsDigit))
        {
            AddError(nameof(Password), "Password must contain a digit.");
        }
    }

    private void ValidateConfirmPassword()
    {
        ClearErrors(nameof(ConfirmPassword));

        if (ConfirmPassword != Password)
        {
            AddError(nameof(ConfirmPassword), "Passwords do not match.");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        ValidateAll();
        if (!HasErrors)
        {
            // Submit logic
        }
    }

    private bool CanSubmit() => !HasErrors && !string.IsNullOrEmpty(Email);

    private void ValidateAll()
    {
        ValidateEmail();
        ValidatePassword();
        ValidateConfirmPassword();
    }
}
```

### 4.3 XAML Binding

```xml
<TextBox Text="{Binding Email,
                        UpdateSourceTrigger=PropertyChanged,
                        ValidatesOnNotifyDataErrors=True}"/>

<!-- Error list display -->
<ItemsControl ItemsSource="{Binding (Validation.Errors),
              RelativeSource={RelativeSource Self}}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding ErrorContent}" Foreground="Red"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

---

## 5. CommunityToolkit.Mvvm Integration

CommunityToolkit.Mvvm 8.0+ provides `ObservableValidator`.

```csharp
public partial class UserViewModel : ObservableValidator
{
    [Required(ErrorMessage = "Please enter a name.")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
    [ObservableProperty] private string _name = string.Empty;

    [Required]
    [Range(1, 150, ErrorMessage = "Please enter a valid age.")]
    [ObservableProperty] private int _age;

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [ObservableProperty] private string _email = string.Empty;

    partial void OnNameChanged(string value) => ValidateProperty(value, nameof(Name));
    partial void OnAgeChanged(int value) => ValidateProperty(value, nameof(Age));
    partial void OnEmailChanged(string value) => ValidateProperty(value, nameof(Email));

    [RelayCommand]
    private void Submit()
    {
        ValidateAllProperties();
        if (!HasErrors)
        {
            // Submit logic
        }
    }
}
```

---

## 6. Summary

| Requirement | Recommended Approach |
|-------------|---------------------|
| Simple XAML validation | ValidationRule |
| ViewModel-based validation | INotifyDataErrorInfo |
| DataAnnotations usage | ObservableValidator (CommunityToolkit) |
| Async validation | INotifyDataErrorInfo |
| Legacy compatibility | IDataErrorInfo |
