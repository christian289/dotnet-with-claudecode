---
description: "Generates WPF service interface, implementation class, and DI registration. Use when adding a new service layer, creating a service with interface separation, or scaffolding a service class with DI wiring. Usage: /wpf-dev-pack:make-wpf-service <ServiceName> [--no-interface]"
argument-hint: [ServiceName]
---

# WPF Service Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the Service name (e.g., DataExport, FileManager)". Do NOT proceed until a valid name is provided. Use the response as the ServiceName for all subsequent steps.**

Generate a `$0Service` with interface + implementation class + DI registration.
Automatically places files in the correct project based on solution structure.
If `--no-interface` is appended, skip interface generation.

- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.

## Usage

```bash
# Interface + Implementation + DI registration
/wpf-dev-pack:make-wpf-service DataExport

# Implementation only (no interface) — for simple services
/wpf-dev-pack:make-wpf-service FileManager --no-interface
```

---

## Execution Procedure

### Step 1: Parse $0

- `$0` is the service name (without `Service` suffix — auto-appended)
  - e.g., `DataExport` → `IDataExportService.cs` + `DataExportService.cs`
- `--no-interface` flag: Skip interface generation (class-only)

### Step 2: Locate Target Projects

Search for solution file and identify projects by naming convention:

| Project Suffix | Purpose | Files Placed |
|----------------|---------|--------------|
| `.Abstractions` | Interfaces | `I$0Service.cs` |
| `.Core` | Business logic | `$0Service.cs` (UI-independent) |
| `.WpfServices` | WPF-dependent services | `$0Service.cs` (if needs WPF types) |
| `.WpfApp` | Application | DI registration in `App.xaml.cs` |

**Fallback rules (keep the interface visible to both the implementation and the app):**
- No `.Abstractions` project → place the interface in **`.Core`** (beside the implementation). Do NOT put it in the WPF app project: `.Core` cannot reference the app, so an interface placed there is unreachable from the implementation in `.Core`.
- No `.Core` or `.WpfServices` (no separate logic project) → place both interface and implementation in the app's `Services/` folder.
- Single project solution → both files in `Services/` folder.

### Step 3: Generate Interface (unless --no-interface)

Create `I$0Service.cs`:

```csharp
namespace {Namespace}.Services;

public interface I$0Service
{
    // TODO: Define service contract
    // TODO: 서비스 계약 정의
}
```

### Step 4: Generate Implementation

Create `$0Service.cs`:

```csharp
namespace {Namespace}.Services;

public sealed class $0Service : I$0Service
{
    // TODO: Implement service
    // TODO: 서비스 구현
}
```

If `--no-interface`:

```csharp
namespace {Namespace}.Services;

public sealed class $0Service
{
    // TODO: Implement service
    // TODO: 서비스 구현
}
```

### Step 5: Register in DI Container

#### CommunityToolkit.Mvvm (GenericHost)

Locate `App.xaml.cs` and add in `ConfigureServices`:

```csharp
// With interface
services.AddSingleton<I$0Service, $0Service>();

// Without interface (--no-interface)
services.AddSingleton<$0Service>();
```

#### Prism 9

Locate `App.xaml.cs` and add in `RegisterTypes`:

```csharp
// With interface
containerRegistry.RegisterSingleton<I$0Service, $0Service>();

// Without interface (--no-interface)
containerRegistry.RegisterSingleton<$0Service>();
```

> **GlobalUsings**: make sure `App.xaml.cs` can resolve the service type — add
> `global using {Namespace}.Services;` to the app's existing `GlobalUsings.cs`
> if absent (the `Services` namespace now exists, so the using resolves). Do not
> create a second `GlobalUsings.cs`.

### Step 6: Report Results

Output list of generated/modified files and next steps guidance.

---

## Generated File Structure

### With Interface (default)

```
{AbstractionsProject}/
└── Services/
    └── I$0Service.cs

{CoreProject}/
└── Services/
    └── $0Service.cs

{WpfAppProject}/
└── App.xaml.cs              (DI registration added)
```

### Without Interface (--no-interface)

```
{CoreProject}/
└── Services/
    └── $0Service.cs

{WpfAppProject}/
└── App.xaml.cs              (DI registration added)
```

> If there is **no `.Abstractions` project** (the default 3-project structure),
> place `I$0Service.cs` in **`$0.Core/Services/`** alongside `$0Service.cs` —
> never in the WPF app project, which `.Core` cannot reference.

---

## Error Handling

- Missing service name → output usage instructions
- No WPF project found → suggest `/wpf-dev-pack:make-wpf-project` first
- Duplicate service → warn and abort
- Multiple candidate projects → ask user which to use
