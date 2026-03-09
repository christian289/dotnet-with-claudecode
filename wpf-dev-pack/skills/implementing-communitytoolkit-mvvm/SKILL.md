---
name: implementing-communitytoolkit-mvvm
description: "Implements MVVM pattern using CommunityToolkit.Mvvm 8.4+ with ObservableProperty attributes and source generators. Use when building ViewModels, implementing commands, or setting up MVVM structure in WPF."
---

# CommunityToolkit.Mvvm Code Guidelines

CommunityToolkit.Mvvm 8.4+ 기반 MVVM 패턴 구현 가이드.

## Prerequisites

```xml
<ItemGroup>
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.*" />
</ItemGroup>
```

- **.NET 8+** 필수 (최소 지원 버전)

## Project Structure

```
MyApp.ViewModels/    ← ViewModel Class Library (UI framework independent)
├── UserViewModel.cs
├── GlobalUsings.cs
└── MyApp.ViewModels.csproj
```

- ViewModel 프로젝트에 `System.Windows` 참조 금지
- BCL 타입 + CommunityToolkit.Mvvm만 참조

## ObservableProperty Attribute Writing Rules

### Single Attribute — Write Inline

```csharp
// ✅ Good: Single attribute written inline
[ObservableProperty] private string _userName = string.Empty;

[ObservableProperty] private int _age;

[ObservableProperty] private bool _isActive;
```

### Multiple Attributes — ObservableProperty Always Inline

```csharp
// ✅ Good: Multiple attributes, ObservableProperty always inline
[NotifyPropertyChangedRecipients]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty] private string _email = string.Empty;

[NotifyDataErrorInfo]
[Required(ErrorMessage = "Name is required.")]
[MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
[ObservableProperty] private string _name = string.Empty;

[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
[NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
[ObservableProperty] private User? _selectedUser;
```

### Bad Example

```csharp
// ❌ Bad: ObservableProperty on separate line
[NotifyPropertyChangedRecipients]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty]
private string _email = string.Empty;
```

## Complete ViewModel Example

```csharp
namespace MyApp.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class UserViewModel : ObservableObject
{
    // Single attribute
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private int _age;

    // Multiple attributes — ObservableProperty inline
    [NotifyPropertyChangedRecipients]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty] private string _email = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    [ObservableProperty] private User? _selectedUser;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Save logic
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Email);
}
```

## Key Rules

- **Single Attribute**: `[ObservableProperty]`를 필드 선언과 같은 줄에 inline 작성
- **Multiple Attributes**: 다른 속성은 별도 줄, `[ObservableProperty]`는 **항상 마지막 줄에 inline**
- **Purpose**: 코드 가독성과 일관된 코딩 스타일 유지

## .NET 9+ Upgrade: Partial Property Syntax

.NET 9+ (C# 13)에서는 **partial property** 문법 사용 가능. field 기반보다 명시적이며 AOT 호환.

```csharp
// .NET 9+ (C# 13): Partial property syntax
[ObservableProperty] public partial string UserName { get; set; }

// Access modifiers 지원
[ObservableProperty] public partial string Status { get; private set; }
[ObservableProperty] public required partial string Id { get; set; }
[ObservableProperty] public override partial string DisplayName { get; set; }

// Multiple attributes
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty] public partial string Email { get; set; }
```

### MVVMTK0045 Warning

8.4.0에서 field 기반 `[ObservableProperty]` 사용 시 **MVVMTK0045** 경고 발생:
> "Using [ObservableProperty] on fields is not AOT compatible in WinRT scenarios"

- .NET 8: 경고 무시 가능 (WPF에서는 AOT 시나리오 해당 없음)
- .NET 9+: Visual Studio Code Fixer로 partial property 자동 변환 (Ctrl+. → "Use partial property")
