---
name: formatting-wpf-csharp-code
description: Formats WPF XAML and C# code using XamlStyler and dotnet format. Generates Settings.XamlStyler and .editorconfig files automatically. Use when code formatting or style cleanup is needed.
---

# WPF and C# Code Formatting

Applies consistent code style to XAML and C# files.

---

## 1. Required Tools

### PowerShell (pwsh)

All commands run in PowerShell environment for cross-platform compatibility.

```powershell
# Verify PowerShell is available
pwsh --version
```

### XamlStyler (XAML Formatting)

Run via `dnx` (dotnet tool runner) in PowerShell. No manual installation required.

### dotnet format (C# Formatting)

Included with .NET SDK by default.

---

## 2. Configuration Files

### Settings.XamlStyler

Copy from template to workspace root if `Settings.XamlStyler` doesn't exist.

**Template location**: `templates/Settings.XamlStyler`

**Key settings**:
- `AttributesTolerance: 1` - Allow up to 1 attribute on same line
- `KeepFirstAttributeOnSameLine: true` - Keep first attribute on element line

### .editorconfig

Copy from template to workspace root if `.editorconfig` doesn't exist.

**Template location**: `templates/.editorconfig`

**Key settings**:
- Indentation: 4 spaces
- Line ending: CRLF (Windows)
- Max line length: 120

---

## 3. Formatting Commands

### XAML Formatting

```powershell
# Format all XAML files in workspace
dnx -y XamlStyler.Console -- -d "{workspace}" -r -c "{workspace}/Settings.XamlStyler"

# Format single file
dnx -y XamlStyler.Console -- -f "{file.xaml}" -c "{workspace}/Settings.XamlStyler"
```

**dnx Options**:
- `-y`: Auto-accept confirmation prompt (dnx option)
- `--`: Separator between dnx options and tool arguments

**XamlStyler Options**:
- `-d`: Target directory
- `-f`: Target file
- `-r`: Recursive processing
- `-c`: Configuration file path

### C# Formatting

```powershell
# Format entire solution
dotnet format "{solution.sln}" --no-restore

# Format specific project only
dotnet format "{project.csproj}" --no-restore

# Format single file
dotnet format "{project.csproj}" --include "{file.cs}" --no-restore
```

**Options**:
- `--no-restore`: Skip NuGet restore (faster)
- `--include`: Target specific file

---

## 4. Workflow

### Full Formatting

```
Task Progress:
- [ ] Step 1: Check if Settings.XamlStyler exists, create if not
- [ ] Step 2: Check if .editorconfig exists, create if not
- [ ] Step 3: Run dnx XamlStyler.Console for XAML formatting
- [ ] Step 4: Run dotnet format for C# formatting
```

### Per-file Formatting (Hook Usage)

```
- When .xaml file modified: Run dnx XamlStyler.Console
- When .cs file modified: Run dotnet format
```

---

## 5. Notes

- **Check git status**: Verify uncommitted changes before formatting
- **Binary exclusion**: bin/, obj/ folders automatically excluded
- **Encoding**: UTF-8 with BOM maintained (Visual Studio compatible)

---

## 6. References

- [XamlStyler GitHub](https://github.com/Xavalon/XamlStyler)
- [dotnet format Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
- [.editorconfig Specification](https://editorconfig.org/)
