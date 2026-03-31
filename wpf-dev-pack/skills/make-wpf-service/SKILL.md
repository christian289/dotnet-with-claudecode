---
description: "Generates WPF service interface, implementation class, and DI registration. Use when adding a new service layer, creating a service with interface separation, or scaffolding a service class with DI wiring. Usage: /wpf-dev-pack:make-wpf-service <ServiceName> [--no-interface]"
---

# WPF Service Generator

Generates service interface + implementation class + DI registration in a single command.
Automatically places files in the correct project based on solution structure.

## Usage

```bash
# Interface + Implementation + DI registration
/wpf-dev-pack:make-wpf-service DataExport

# Implementation only (no interface) — for simple services
/wpf-dev-pack:make-wpf-service FileManager --no-interface
```

---

## Execution Procedure

### Step 1: Argument Parsing

- 1st argument (required): Service name (without `Service` suffix — auto-appended)
  - `DataExport` → `IDataExportService.cs` + `DataExportService.cs`
- `--no-interface`: Skip interface generation (class-only)

### Step 2: Locate Target Projects

Search for solution file and identify projects by naming convention:

| Project Suffix | Purpose | Files Placed |
|----------------|---------|--------------|
| `.Abstractions` | Interfaces | `I{Name}Service.cs` |
| `.Core` | Business logic | `{Name}Service.cs` (UI-independent) |
| `.WpfServices` | WPF-dependent services | `{Name}Service.cs` (if needs WPF types) |
| `.WpfApp` | Application | DI registration in `App.xaml.cs` |

**Fallback rules:**
- No `.Abstractions` project → place interface in `Services/` folder of main project
- No `.Core` or `.WpfServices` → place implementation in `Services/` folder of main project
- Single project solution → all files in `Services/` folder

### Step 3: Generate Interface (unless --no-interface)

Create `I{Name}Service.cs`:

```csharp
namespace {Namespace}.Services;

public interface I{Name}Service
{
    // TODO: Define service contract
    // TODO: 서비스 계약 정의
}
```

### Step 4: Generate Implementation

Create `{Name}Service.cs`:

```csharp
namespace {Namespace}.Services;

public sealed class {Name}Service : I{Name}Service
{
    // TODO: Implement service
    // TODO: 서비스 구현
}
```

If `--no-interface`:

```csharp
namespace {Namespace}.Services;

public sealed class {Name}Service
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
services.AddSingleton<I{Name}Service, {Name}Service>();

// Without interface (--no-interface)
services.AddSingleton<{Name}Service>();
```

#### Prism 9

Locate `App.xaml.cs` and add in `RegisterTypes`:

```csharp
// With interface
containerRegistry.RegisterSingleton<I{Name}Service, {Name}Service>();

// Without interface (--no-interface)
containerRegistry.RegisterSingleton<{Name}Service>();
```

### Step 6: Report Results

Output list of generated/modified files and next steps guidance.

---

## Generated File Structure

### With Interface (default)

```
{AbstractionsProject}/
└── Services/
    └── I{Name}Service.cs

{CoreProject}/
└── Services/
    └── {Name}Service.cs

{WpfAppProject}/
└── App.xaml.cs              (DI registration added)
```

### Without Interface (--no-interface)

```
{CoreProject}/
└── Services/
    └── {Name}Service.cs

{WpfAppProject}/
└── App.xaml.cs              (DI registration added)
```

---

## Error Handling

- Missing service name → output usage instructions
- No WPF project found → suggest `/wpf-dev-pack:make-wpf-project` first
- Duplicate service → warn and abort
- Multiple candidate projects → ask user which to use
