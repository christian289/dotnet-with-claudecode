---
description: "Manages literal strings by pre-defining them as const string in C#. Use when organizing string constants, log messages, exception messages, or UI texts across the codebase."
user-invocable: false
---

# Literal String Handling

A guide on handling literal strings in C# code.

## Project Structure

The templates folder contains a Console Application example (use latest .NET per version mapping).

```
templates/
└── LiteralStringSample/                ← Console Application
    ├── Constants/
    │   ├── Messages.cs                 ← General message constants
    │   └── LogMessages.cs              ← Log message constants
    ├── Program.cs                      ← Top-Level Statement entry point
    ├── GlobalUsings.cs
    └── LiteralStringSample.csproj
```

## Rule

**Literal strings should preferably be pre-defined as `const string`**

## Examples

### Good Example

```csharp
// Good example
const string ErrorMessage = "An error has occurred.";

if (condition)
    throw new Exception(ErrorMessage);
```

### Bad Example

```csharp
// Bad example
if (condition)
    throw new Exception("An error has occurred.");
```

## Constants Class Structure

Manage by separating into static classes by message type:

```csharp
// Constants/Messages.cs
namespace LiteralStringSample.Constants;

public static class Messages
{
    // Error messages
    public const string ErrorOccurred = "An error has occurred.";

    public const string InvalidInput = "Invalid input.";

    // Success messages
    public const string OperationSuccess = "Operation completed successfully.";
}
```

```csharp
// Constants/LogMessages.cs
namespace LiteralStringSample.Constants;

public static class LogMessages
{
    // Information logs
    public const string ApplicationStarted = "Application started.";

    // Format strings
    public const string UserLoggedIn = "User logged in: {0}";
}
```

## Usage Example

```csharp
using LiteralStringSample.Constants;

try
{
    if (string.IsNullOrEmpty(input))
    {
        throw new ArgumentException(Messages.InvalidInput);
    }

    Console.WriteLine(Messages.OperationSuccess);
}
catch (Exception)
{
    Console.WriteLine(Messages.ErrorOccurred);
}

// Using format strings
Console.WriteLine(string.Format(LogMessages.UserLoggedIn, userName));
```

## Reasons

1. **Maintainability**: Only one place to modify when changing messages
2. **Reusability**: Same messages can be used in multiple places
3. **Type safety**: Typos can be caught at compile time
4. **Performance**: Eliminates string literal duplication
5. **Consistency**: Messages can be managed in pairs (e.g., Korean/English)

## WPF-Specific Case: VisualState Names

WPF's `VisualStateManager` is a name-based contract between C# and XAML
where `[TemplateVisualState(Name = …)]` and `VisualStateManager.GoToState`
accept `const string`, but XAML `<VisualState x:Name="…">` cannot reference
a C# `const` — so the XAML half is unavoidably a literal that must match
the C# constant exactly. A mismatch is a silent runtime no-op (no
exception, no compiler error). See
[`authoring-wpf-controls` §3.4 "Visual State Naming Contract"](../authoring-wpf-controls/SKILL.md)
for the consolidation pattern and pitfalls.

