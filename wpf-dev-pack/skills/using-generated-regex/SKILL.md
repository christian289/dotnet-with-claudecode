---
name: using-generated-regex
description: Implements compile-time regex using GeneratedRegexAttribute Source Generator. Use when working with Regex patterns for better performance and AOT compatibility.
---

# Using GeneratedRegex (Source Generator)

Use `GeneratedRegexAttribute` for compile-time regex generation instead of runtime `new Regex()`.

## Why GeneratedRegex?

| Aspect | Runtime Regex | GeneratedRegex |
|--------|--------------|----------------|
| Compilation | Runtime | Compile-time |
| Performance | Slower first match | Pre-compiled, faster |
| AOT Support | Limited | Full support |
| Memory | Allocates at runtime | No runtime allocation |
| .NET Version | All | .NET 7+ |

---

## Basic Pattern

```csharp
public partial class EmailValidator
{
    [GeneratedRegex(@"^[\w\.-]+@[\w\.-]+\.\w+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();

    public bool IsValidEmail(string email)
    {
        return EmailPattern().IsMatch(email);
    }
}
```

**Requirements:**
- Class must be `partial`
- Method must be `static partial` returning `Regex`
- .NET 7 or later

---

## Common Patterns

### Email Validation

```csharp
public partial class ValidationPatterns
{
    [GeneratedRegex(@"^[\w\.-]+@[\w\.-]+\.\w+$", RegexOptions.IgnoreCase)]
    public static partial Regex Email();
}
```

### Phone Number

```csharp
[GeneratedRegex(@"^\d{3}-\d{3,4}-\d{4}$")]
public static partial Regex PhoneNumber();
```

### URL Pattern

```csharp
[GeneratedRegex(@"^https?://[\w\.-]+(?:/[\w\.-]*)*$", RegexOptions.IgnoreCase)]
public static partial Regex Url();
```

### Whitespace Normalization

```csharp
[GeneratedRegex(@"\s+")]
private static partial Regex WhitespacePattern();

public string NormalizeWhitespace(string input)
{
    return WhitespacePattern().Replace(input, " ");
}
```

---

## Migration from Runtime Regex

### Before (Runtime)

```csharp
// Anti-pattern: Runtime compilation
public class Validator
{
    private static readonly Regex _emailRegex =
        new(@"^[\w\.-]+@[\w\.-]+\.\w+$", RegexOptions.Compiled);

    public bool IsValid(string email)
    {
        return _emailRegex.IsMatch(email);
    }
}
```

### After (Source Generator)

```csharp
// Best practice: Compile-time generation
public partial class Validator
{
    [GeneratedRegex(@"^[\w\.-]+@[\w\.-]+\.\w+$")]
    private static partial Regex EmailPattern();

    public bool IsValid(string email)
    {
        return EmailPattern().IsMatch(email);
    }
}
```

---

## RegexOptions

```csharp
// Multiple options
[GeneratedRegex(@"pattern", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
private static partial Regex MultiOptionPattern();

// With timeout (for untrusted input)
[GeneratedRegex(@"complex.*pattern", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
private static partial Regex SafePattern();
```

---

## WPF Validation Example

```csharp
public partial class FormViewModel : ObservableValidator
{
    [GeneratedRegex(@"^[\w\.-]+@[\w\.-]+\.\w+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"^\d{3}-\d{3,4}-\d{4}$")]
    private static partial Regex PhonePattern();

    [CustomValidation(typeof(FormViewModel), nameof(ValidateEmail))]
    [ObservableProperty] private string _email = string.Empty;

    [CustomValidation(typeof(FormViewModel), nameof(ValidatePhone))]
    [ObservableProperty] private string _phone = string.Empty;

    public static ValidationResult? ValidateEmail(string email, ValidationContext context)
    {
        if (string.IsNullOrEmpty(email))
            return new ValidationResult("Email is required");

        if (!EmailPattern().IsMatch(email))
            return new ValidationResult("Invalid email format");

        return ValidationResult.Success;
    }

    public static ValidationResult? ValidatePhone(string phone, ValidationContext context)
    {
        if (string.IsNullOrEmpty(phone))
            return ValidationResult.Success; // Optional field

        if (!PhonePattern().IsMatch(phone))
            return new ValidationResult("Format: 000-0000-0000");

        return ValidationResult.Success;
    }
}
```

---

## GlobalUsings.cs

```csharp
global using System.Text.RegularExpressions;
```

---

## Checklist

- [ ] Class is marked as `partial`
- [ ] Method is `static partial` returning `Regex`
- [ ] Pattern string is valid regex
- [ ] Target framework is .NET 7+
- [ ] Consider `RegexOptions` for case sensitivity
- [ ] Add timeout for untrusted input patterns
