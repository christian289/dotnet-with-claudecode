---
description: "Designs WPF solution and project structures based on Clean Architecture. Use when creating new WPF solutions, organizing layers, or establishing project naming conventions."
---

# WPF Solution and Project Structure

A guide for designing WPF project solution and project structure based on Clean Architecture.

## Template Project

The templates folder contains a Clean Architecture WPF solution example (use latest .NET per version mapping).

```
templates/
├── GameDataTool.sln                    ← Solution file
├── Directory.Build.props               ← Common build settings
├── src/
│   ├── GameDataTool.Domain/            ← 🔵 Core - Pure domain models
│   ├── GameDataTool.Application/       ← 🟢 Core - Use Cases
│   ├── GameDataTool.Infrastructure/    ← 🟡 Infrastructure - External systems
│   ├── GameDataTool.ViewModels/        ← 🟠 Presentation - ViewModel
│   ├── GameDataTool.WpfServices/       ← 🟠 Presentation - WPF services
│   ├── GameDataTool.UI/                ← 🔴 Presentation - Custom Controls
│   └── GameDataTool.WpfApp/            ← 🔴 Composition Root
└── tests/
    ├── GameDataTool.Domain.Tests/
    ├── GameDataTool.Application.Tests/
    └── GameDataTool.ViewModels.Tests/
```

## Solution Structure Principles

**Solution name is the application name**
- Example: `GameDataTool` solution = executable .NET Assembly name

## Project Structure (Clean Architecture)

```
SolutionName/
├── src/
│   │
│   │  ══════════════ Core (No Dependencies) ══════════════
│   │
│   ├── SolutionName.Domain              // 🔵 Entities - Pure domain models
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/                  //    - Domain service interfaces only
│   │
│   ├── SolutionName.Application         // 🟢 Use Cases - Business logic coordination
│   │   ├── Interfaces/                  //    - IRepository, IExternalService, etc.
│   │   ├── Services/                    //    - Application services
│   │   └── DTOs/
│   │
│   │  ══════════════ Infrastructure ══════════════
│   │
│   ├── SolutionName.Infrastructure      // 🟡 External system implementation
│   │   ├── Persistence/                 //    - Data access implementation
│   │   ├── FileSystem/                  //    - File system access
│   │   └── ExternalServices/            //    - HTTP, API, etc.
│   │
│   │  ══════════════ Presentation (WPF) ══════════════
│   │
│   ├── SolutionName.ViewModels          // 🟠 ViewModels (Interface Adapter role)
│   │   └── (Depends on Application only)
│   │
│   ├── SolutionName.WpfServices         // 🟠 WPF-specific services
│   │   └── (Dialog, Navigation, etc.)
│   │
│   ├── SolutionName.UI                  // 🔴 Custom Controls & Styles
│   │
│   └── SolutionName.WpfApp              // 🔴 Composition Root (Entry point)
│       └── App.xaml.cs                  //    - DI setup, connect all implementations
│
└── tests/
    ├── SolutionName.Domain.Tests
    ├── SolutionName.Application.Tests
    └── SolutionName.ViewModels.Tests
```

## Roles by Project Type

### Core Layer (No Dependencies)

| Project | Role | Contents |
|---------|------|----------|
| `.Domain` | 🔵 Entities | Pure domain models, ValueObjects, domain interfaces |
| `.Application` | 🟢 Use Cases | Business logic coordination, IRepository/IService interfaces, DTOs |

### Infrastructure Layer

| Project | Role | Contents |
|---------|------|----------|
| `.Infrastructure` | 🟡 External Systems | Repository implementation, file system, HTTP/API clients |

### Presentation Layer (WPF)

| Project | Role | Contents |
|---------|------|----------|
| `.ViewModels` | 🟠 Interface Adapter | MVVM ViewModel (depends on Application only, no WPF reference) |
| `.WpfServices` | 🟠 WPF Services | DialogService, NavigationService, WindowService |
| `.UI` | 🔴 Custom Controls | ResourceDictionary, CustomControl, Themes |
| `.WpfApp` | 🔴 Composition Root | App.xaml, DI setup, Views, entry point |

## Project Dependency Hierarchy

```
                    ┌─────────────────────────────────────┐
                    │         SolutionName.WpfApp         │  ← Composition Root
                    │   (References all projects, DI setup)   │
                    └─────────────────────────────────────┘
                                      │
          ┌───────────────────────────┼───────────────────────────┐
          ▼                           ▼                           ▼
┌─────────────────┐       ┌─────────────────────┐       ┌─────────────────┐
│  .ViewModels    │       │   .Infrastructure   │       │  .WpfServices   │
│  (Application   │       │ (Application ref)   │       │  (Application   │
│     ref)        │       │                     │       │     ref)        │
└────────┬────────┘       └──────────┬──────────┘       └────────┬────────┘
         │                           │                           │
         └───────────────────────────┼───────────────────────────┘
                                     ▼
                    ┌─────────────────────────────────────┐
                    │       SolutionName.Application      │  ← Use Cases
                    │         (Domain ref only)           │
                    └─────────────────────────────────────┘
                                      │
                                      ▼
                    ┌─────────────────────────────────────┐
                    │         SolutionName.Domain         │  ← Core (No dependencies)
                    │           (No references)           │
                    └─────────────────────────────────────┘
```

## Detailed Layer Descriptions

### Domain Layer (Pure Domain)

```csharp
// Domain/Entities/User.cs
namespace GameDataTool.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;

    public void UpdateName(string name)
    {
        // Domain business rule validation
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        Name = name;
    }
}
```

```csharp
// Domain/ValueObjects/Email.cs
namespace GameDataTool.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValid(value))
            throw new DomainException("Invalid email format.");

        Value = value;
    }

    private static bool IsValid(string email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@');
}
```

### Application Layer (Use Cases)

```csharp
// Application/Interfaces/IUserRepository.cs
namespace GameDataTool.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
```

```csharp
// Application/Services/UserService.cs
namespace GameDataTool.Application.Services;

public sealed class UserService(IUserRepository userRepository)
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : new UserDto(user.Id, user.Name, user.Email.Value);
    }

    public async Task UpdateUserNameAsync(Guid id, string newName, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.UpdateName(newName);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
```

```csharp
// Application/DTOs/UserDto.cs
namespace GameDataTool.Application.DTOs;

public sealed record UserDto(Guid Id, string Name, string Email);
```

### Infrastructure Layer (External System Implementation)

```csharp
// Infrastructure/Persistence/UserRepository.cs
namespace GameDataTool.Infrastructure.Persistence;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Users.ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### ViewModels Layer (Presentation - Depends on Application Only)

```csharp
// ViewModels/UserViewModel.cs
namespace GameDataTool.ViewModels;

public sealed partial class UserViewModel(UserService userService) : ObservableObject
{
    private readonly UserService _userService = userService;

    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _userEmail = string.Empty;

    [RelayCommand]
    private async Task LoadUserAsync(Guid userId)
    {
        var user = await _userService.GetUserAsync(userId);
        if (user is null) return;

        UserName = user.Name;
        UserEmail = user.Email;
    }
}
```

### WpfApp Layer (Composition Root - DI Setup)

```csharp
// WpfApp/App.xaml.cs
namespace GameDataTool.WpfApp;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Domain - No registration needed (pure models)

                // Application Layer
                services.AddTransient<UserService>();

                // Infrastructure Layer
                services.AddDbContext<AppDbContext>();
                services.AddScoped<IUserRepository, UserRepository>();

                // Presentation Layer
                services.AddTransient<UserViewModel>();
                services.AddTransient<MainViewModel>();

                // WPF Services
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();
        _host.Services.GetRequiredService<MainWindow>().Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
        base.OnExit(e);
    }
}
```

## Actual Folder Structure Example

```
GameDataTool/
├── src/
│   ├── GameDataTool.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   └── GameData.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   └── GameVersion.cs
│   │   ├── Interfaces/
│   │   │   └── IDomainEventPublisher.cs
│   │   ├── Exceptions/
│   │   │   └── DomainException.cs
│   │   ├── GlobalUsings.cs
│   │   └── GameDataTool.Domain.csproj
│   │
│   ├── GameDataTool.Application/
│   │   ├── Interfaces/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── IGameDataRepository.cs
│   │   │   └── IFileExportService.cs
│   │   ├── Services/
│   │   │   ├── UserService.cs
│   │   │   └── GameDataService.cs
│   │   ├── DTOs/
│   │   │   ├── UserDto.cs
│   │   │   └── GameDataDto.cs
│   │   ├── Exceptions/
│   │   │   └── NotFoundException.cs
│   │   ├── GlobalUsings.cs
│   │   └── GameDataTool.Application.csproj
│   │
│   ├── GameDataTool.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── UserRepository.cs
│   │   │   └── GameDataRepository.cs
│   │   ├── FileSystem/
│   │   │   └── ExcelExportService.cs
│   │   ├── ExternalServices/
│   │   │   └── ApiClient.cs
│   │   ├── GlobalUsings.cs
│   │   └── GameDataTool.Infrastructure.csproj
│   │
│   ├── GameDataTool.ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── UserViewModel.cs
│   │   ├── GameDataViewModel.cs
│   │   ├── GlobalUsings.cs
│   │   └── GameDataTool.ViewModels.csproj
│   │
│   ├── GameDataTool.WpfServices/
│   │   ├── DialogService.cs
│   │   ├── NavigationService.cs
│   │   ├── WindowService.cs
│   │   ├── GlobalUsings.cs
│   │   └── GameDataTool.WpfServices.csproj
│   │
│   ├── GameDataTool.UI/
│   │   ├── Themes/
│   │   │   ├── Generic.xaml
│   │   │   └── CustomButton.xaml
│   │   ├── CustomControls/
│   │   │   └── CustomButton.cs
│   │   ├── Properties/
│   │   │   └── AssemblyInfo.cs
│   │   └── GameDataTool.UI.csproj
│   │
│   └── GameDataTool.WpfApp/
│       ├── Views/
│       │   ├── MainWindow.xaml
│       │   ├── MainWindow.xaml.cs
│       │   ├── UserView.xaml
│       │   └── UserView.xaml.cs
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── Mappings.xaml
│       ├── GlobalUsings.cs
│       └── GameDataTool.WpfApp.csproj
│
├── tests/
│   ├── GameDataTool.Domain.Tests/
│   │   ├── Entities/
│   │   │   └── UserTests.cs
│   │   └── GameDataTool.Domain.Tests.csproj
│   │
│   ├── GameDataTool.Application.Tests/
│   │   ├── Services/
│   │   │   └── UserServiceTests.cs
│   │   └── GameDataTool.Application.Tests.csproj
│   │
│   └── GameDataTool.ViewModels.Tests/
│       ├── UserViewModelTests.cs
│       └── GameDataTool.ViewModels.Tests.csproj
│
├── GameDataTool.sln
└── Directory.Build.props
```

## Assembly Reference Rules

### Domain Project
- ❌ Does not reference any project
- ✅ Uses pure C# BCL only

### Application Project
- ✅ References Domain only
- ❌ Cannot reference Infrastructure, Presentation

### Infrastructure Project
- ✅ References Domain, Application
- ✅ Can use external NuGet packages (EF Core, HttpClient, etc.)

### ViewModels Project
- ✅ References Application only
- ❌ Cannot reference WPF assemblies (WindowsBase, PresentationFramework, etc.)
- ✅ Can use CommunityToolkit.Mvvm

### WpfApp Project (Composition Root)
- ✅ References all projects
- ✅ Connects all implementations in DI container

## Clean Architecture Advantages

1. **Independence**: Core layer is independent from external frameworks
2. **Testability**: Each layer can be tested independently
3. **Maintainability**: Clear scope of change impact
4. **Flexibility**: Easy to replace Infrastructure (DB, API, etc.)

