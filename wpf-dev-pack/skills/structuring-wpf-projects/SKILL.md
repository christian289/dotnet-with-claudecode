---
description: "Designs WPF solution and project structures based on Clean Architecture. Use when creating new WPF solutions, organizing layers, or establishing project naming conventions."
---

# WPF Solution and Project Structure

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/mvvm-framework.md` 설정에 따라 코드 스타일이 결정됩니다.
> Prism 9 사용 시 → [PRISM.md](PRISM.md) 참조

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

> **Advanced**: See [ADVANCED.md](ADVANCED.md) for detailed layer implementation examples (Domain, Application, Infrastructure, ViewModels, WpfApp) and actual folder structure.

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

