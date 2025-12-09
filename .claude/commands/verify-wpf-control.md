---
description: Verify WPF CustomControl generation command functionality
allowed-tools: Bash(find:*), Bash(ls:*), Bash(mkdir:*), Bash(rm:*), Bash(cat:*), Bash(dotnet:*), Bash(grep:*), Bash(test:*), Bash(cd:*), Bash(pwd:*), Read, Write, Glob
argument-hint: [test-case] (all|case-a|case-b|case-c|case-d|error-handling)
---

# WPF Custom Control Command Verification

## Argument Information

- Received arguments: $ARGUMENTS
- Default: `all` (run all test cases)
- Available options:
  - `all`: Run all verification tests
  - `case-a`: Test new solution with Gallery creation
  - `case-b`: Test adding UI library to existing solution
  - `case-c`: Test single CustomControl library scenario
  - `case-d`: Test multiple CustomControl libraries scenario
  - `error-handling`: Test error handling scenarios

## Test Environment Setup

### Create Isolated Test Directory

```bash
TEST_ROOT="/tmp/wpf-control-test-$(date +%Y%m%d%H%M%S)"
mkdir -p "$TEST_ROOT"
cd "$TEST_ROOT"
echo "Test root: $TEST_ROOT"
```

## Test Cases

### Case A: No Solution File (New Solution with Gallery)

**Purpose:** Verify complete solution creation with Gallery application

**Setup:**

```bash
TEST_DIR="$TEST_ROOT/case-a"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"
```

**Execute Command:**

```bash
# Simulate: /project:wpf-custom-control TestButton "A custom button with rounded corners"
```

**Verification Checklist:**

1. **Solution Structure Verification:**

```bash
# Check solution file exists
test -f "TestButton.Wpf.slnx" && echo "✓ Solution file exists" || echo "✗ Solution file missing"

# Check Gallery project
test -d "TestButton.Wpf.Gallery" && echo "✓ Gallery project directory exists" || echo "✗ Gallery project missing"
test -f "TestButton.Wpf.Gallery/TestButton.Wpf.Gallery.csproj" && echo "✓ Gallery csproj exists" || echo "✗ Gallery csproj missing"
test -f "TestButton.Wpf.Gallery/Properties/AssemblyInfo.cs" && echo "✓ Gallery AssemblyInfo.cs in Properties" || echo "✗ Gallery AssemblyInfo.cs location wrong"

# Check UI project
test -d "TestButton.Wpf.UI" && echo "✓ UI project directory exists" || echo "✗ UI project missing"
test -f "TestButton.Wpf.UI/TestButton.Wpf.UI.csproj" && echo "✓ UI csproj exists" || echo "✗ UI csproj missing"
```

2. **UI Project Structure Verification:**

```bash
# Controls directory and file
test -d "TestButton.Wpf.UI/Controls" && echo "✓ Controls directory exists" || echo "✗ Controls directory missing"
test -f "TestButton.Wpf.UI/Controls/TestButton.cs" && echo "✓ Control class file exists" || echo "✗ Control class file missing"

# Themes directory and files
test -d "TestButton.Wpf.UI/Themes" && echo "✓ Themes directory exists" || echo "✗ Themes directory missing"
test -f "TestButton.Wpf.UI/Themes/Generic.xaml" && echo "✓ Generic.xaml exists" || echo "✗ Generic.xaml missing"
test -f "TestButton.Wpf.UI/Themes/TestButton.xaml" && echo "✓ Style XAML exists" || echo "✗ Style XAML missing"
test -f "TestButton.Wpf.UI/Themes/TestButtonResources.xaml" && echo "✓ Resources XAML exists" || echo "✗ Resources XAML missing"

# Properties directory
test -f "TestButton.Wpf.UI/Properties/AssemblyInfo.cs" && echo "✓ UI AssemblyInfo.cs in Properties" || echo "✗ UI AssemblyInfo.cs location wrong"

# CustomControl1.cs should be deleted
test ! -f "TestButton.Wpf.UI/CustomControl1.cs" && echo "✓ CustomControl1.cs removed" || echo "✗ CustomControl1.cs still exists"
```

3. **Content Verification:**

```bash
# Control class content
grep -q "sealed class TestButton" "TestButton.Wpf.UI/Controls/TestButton.cs" && echo "✓ sealed class declaration" || echo "✗ sealed keyword missing"
grep -q "DefaultStyleKeyProperty.OverrideMetadata" "TestButton.Wpf.UI/Controls/TestButton.cs" && echo "✓ DefaultStyleKey override" || echo "✗ DefaultStyleKey missing"

# Generic.xaml MergedDictionaries
grep -q "TestButton.xaml" "TestButton.Wpf.UI/Themes/Generic.xaml" && echo "✓ Style merged in Generic.xaml" || echo "✗ Style not merged"

# MainWindow.xaml control reference
grep -q "controls:TestButton" "TestButton.Wpf.Gallery/MainWindow.xaml" && echo "✓ Control referenced in MainWindow" || echo "✗ Control not in MainWindow"
```

4. **Build Verification:**

```bash
dotnet build && echo "✓ Build succeeded" || echo "✗ Build failed"
```

5. **Project Reference Verification:**

```bash
grep -q "TestButton.Wpf.UI.csproj" "TestButton.Wpf.Gallery/TestButton.Wpf.Gallery.csproj" && echo "✓ Project reference exists" || echo "✗ Project reference missing"
```

---

### Case B: Solution Exists, No CustomControl Library

**Purpose:** Verify UI library creation in existing solution

**Setup:**

```bash
TEST_DIR="$TEST_ROOT/case-b"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Create existing solution with WPF app only
dotnet new wpf -n MyApp -o MyApp
dotnet new sln -n MyApp
dotnet sln MyApp.sln add MyApp/MyApp.csproj
dotnet sln MyApp.sln migrate
rm -f MyApp.sln
```

**Execute Command:**

```bash
# Simulate: /project:wpf-custom-control CustomCard "A card panel with shadow"
```

**Verification Checklist:**

```bash
# New UI project created with solution name
test -d "MyApp.UI" && echo "✓ MyApp.UI project created" || echo "✗ UI project not created"
test -f "MyApp.UI/MyApp.UI.csproj" && echo "✓ UI csproj exists" || echo "✗ UI csproj missing"

# Project added to solution
grep -q "MyApp.UI" "MyApp.slnx" && echo "✓ UI project in solution" || echo "✗ UI project not in solution"

# Control files
test -f "MyApp.UI/Controls/CustomCard.cs" && echo "✓ Control class exists" || echo "✗ Control class missing"
test -f "MyApp.UI/Themes/CustomCard.xaml" && echo "✓ Style XAML exists" || echo "✗ Style XAML missing"
test -f "MyApp.UI/Themes/CustomCardResources.xaml" && echo "✓ Resources XAML exists" || echo "✗ Resources XAML missing"

# Structure organization
test -f "MyApp.UI/Properties/AssemblyInfo.cs" && echo "✓ AssemblyInfo in Properties" || echo "✗ AssemblyInfo location wrong"
test ! -f "MyApp.UI/CustomControl1.cs" && echo "✓ CustomControl1.cs removed" || echo "✗ CustomControl1.cs exists"

# Build test
dotnet build && echo "✓ Build succeeded" || echo "✗ Build failed"
```

---

### Case C: Single CustomControl Library Exists

**Purpose:** Verify control generation in existing single library

**Setup:**

```bash
TEST_DIR="$TEST_ROOT/case-c"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Create solution with existing CustomControl library
dotnet new wpf -n TestApp -o TestApp
dotnet new wpfcustomcontrollib -n TestApp.Controls -o TestApp.Controls
dotnet new sln -n TestApp
dotnet sln TestApp.sln add TestApp/TestApp.csproj
dotnet sln TestApp.sln add TestApp.Controls/TestApp.Controls.csproj
dotnet sln TestApp.sln migrate
rm -f TestApp.sln

# Organize existing project
mkdir -p TestApp.Controls/Properties
mkdir -p TestApp.Controls/Controls
mv TestApp.Controls/AssemblyInfo.cs TestApp.Controls/Properties/AssemblyInfo.cs
rm -f TestApp.Controls/CustomControl1.cs
```

**Execute Command:**

```bash
# Simulate: /project:wpf-custom-control IconButton "Button with icon support"
```

**Verification Checklist:**

```bash
# Control added to existing library
test -f "TestApp.Controls/Controls/IconButton.cs" && echo "✓ Control class created" || echo "✗ Control class missing"
test -f "TestApp.Controls/Themes/IconButton.xaml" && echo "✓ Style XAML created" || echo "✗ Style XAML missing"
test -f "TestApp.Controls/Themes/IconButtonResources.xaml" && echo "✓ Resources XAML created" || echo "✗ Resources XAML missing"

# Generic.xaml updated
grep -q "IconButton.xaml" "TestApp.Controls/Themes/Generic.xaml" && echo "✓ Style merged in Generic.xaml" || echo "✗ Style not merged"

# No new project created
ls -d */ | wc -l | grep -q "2" && echo "✓ No extra projects created" || echo "✗ Unexpected projects created"

# Build test
dotnet build && echo "✓ Build succeeded" || echo "✗ Build failed"
```

---

### Case D: Multiple CustomControl Libraries Exist

**Purpose:** Verify user selection prompt for multiple libraries

**Setup:**

```bash
TEST_DIR="$TEST_ROOT/case-d"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Create solution with multiple CustomControl libraries
dotnet new wpf -n MultiApp -o MultiApp
dotnet new wpfcustomcontrollib -n MultiApp.Core -o MultiApp.Core
dotnet new wpfcustomcontrollib -n MultiApp.Extended -o MultiApp.Extended
dotnet new sln -n MultiApp
dotnet sln MultiApp.sln add MultiApp/MultiApp.csproj
dotnet sln MultiApp.sln add MultiApp.Core/MultiApp.Core.csproj
dotnet sln MultiApp.sln add MultiApp.Extended/MultiApp.Extended.csproj
dotnet sln MultiApp.sln migrate
rm -f MultiApp.sln

# Organize both libraries
for lib in MultiApp.Core MultiApp.Extended; do
    mkdir -p "$lib/Properties"
    mkdir -p "$lib/Controls"
    mv "$lib/AssemblyInfo.cs" "$lib/Properties/AssemblyInfo.cs"
    rm -f "$lib/CustomControl1.cs"
done
```

**Verification Checklist:**

```bash
# Should detect 2 libraries
find . -name "*.csproj" -exec dirname {} \; | while read dir; do
    if [ -d "$dir/Themes" ] && [ -f "$dir/Themes/Generic.xaml" ] && [ -f "$dir/Properties/AssemblyInfo.cs" ]; then
        echo "Found CustomControl library: $dir"
    fi
done | wc -l | grep -q "2" && echo "✓ 2 libraries detected" || echo "✗ Library detection failed"

# Command should prompt for selection (verify in output)
echo "✓ User selection prompt expected"
```

---

### Error Handling Tests

**Purpose:** Verify proper error handling

**Test 1: Missing Arguments**

```bash
TEST_DIR="$TEST_ROOT/error-args"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Simulate: /project:wpf-custom-control (no arguments)
# Expected: Error message with usage instructions
```

**Test 2: HTML File Not Found**

```bash
TEST_DIR="$TEST_ROOT/error-html"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Simulate: /project:wpf-custom-control TestControl ./nonexistent.html
# Expected: Error message requesting file path verification
```

**Test 3: CSS File Not Found (Warning)**

```bash
TEST_DIR="$TEST_ROOT/error-css"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"

# Create HTML file but not CSS
echo "<div class='test'>Test</div>" > test.html

# Simulate: /project:wpf-custom-control TestControl ./test.html ./nonexistent.css
# Expected: Warning message, proceed without CSS
```

---

## Test Report Generation

```bash
echo ""
echo "========================================"
echo "  WPF Custom Control Verification Report"
echo "========================================"
echo "Test Root: $TEST_ROOT"
echo "Timestamp: $(date)"
echo ""
echo "Results Summary:"
echo "----------------------------------------"
# Collect all test results here
```

## Cleanup (Optional)

```bash
# Remove test directories after verification
# rm -rf "$TEST_ROOT"
echo "Test directories preserved at: $TEST_ROOT"
echo "Run 'rm -rf $TEST_ROOT' to clean up"
```

## Expected Command Output Examples

### Successful Case A Output:

```
✓ Created solution: TestButton.Wpf.slnx
✓ Created Gallery project: TestButton.Wpf.Gallery
✓ Created UI library: TestButton.Wpf.UI
✓ Generated files:
  - Controls/TestButton.cs
  - Themes/TestButtonResources.xaml
  - Themes/TestButton.xaml
  - Themes/Generic.xaml (modified)

Gallery is ready for testing:
  dotnet run --project TestButton.Wpf.Gallery
```

### Successful Case C Output:

```
✓ Found WPF CustomControl Library: TestApp.Controls
✓ Generated files:
  - Controls/IconButton.cs
  - Themes/IconButtonResources.xaml
  - Themes/IconButton.xaml
  - Themes/Generic.xaml (modified)

Next steps:
  - Add DependencyProperties as needed
  - Run 'dotnet build' to verify
```
