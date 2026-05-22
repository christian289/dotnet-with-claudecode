#!/usr/bin/env dotnet

// Dotnet Version Checker Hook (SessionStart)
//
// wpf-dev-pack ships hooks as C# file-based apps (`dotnet <file.cs>`),
// which requires .NET SDK 10.0.300 or higher. If the SDK is missing or
// too old, those hooks fail silently or misbehave — the user has no
// clear signal that something is wrong.
//
// This hook runs at session start, invokes `dotnet --version`, parses
// the result, and emits a HIGH-VISIBILITY warning if the SDK is
// missing or below the required version.
//
// Caches per day in a temp file so the check runs at most once per day
// (mirrors the McpDependencyChecker convention to avoid spamming).
//
// Input:  stdin JSON (SessionStart payload). Consumed but unused.
// Output: warning text on stdout, when applicable.

using System.Diagnostics;

const int RequiredMajor = 10;
const int RequiredMinor = 0;
const int RequiredPatch = 300;

// ANSI escape sequences for terminals that support them. Non-ANSI
// terminals will render the codes as text, which is harmless because
// the warning content (emoji + caps + indentation) is independently
// high-visibility.
const string Red    = "\x1b[91m";
const string Bold   = "\x1b[1m";
const string Reset  = "\x1b[0m";

// Consume stdin for protocol compatibility.
_ = Console.In.ReadToEnd();

// Run-at-most-once-per-day cache.
var cachePath = Path.Combine(Path.GetTempPath(),
    $"wpf-dev-pack-dotnet-check-{DateTime.Now:yyyy-MM-dd}.txt");
if (File.Exists(cachePath))
    return;
try { File.WriteAllText(cachePath, DateTime.Now.ToString("o")); } catch { }

string? version = null;
try
{
    var psi = new ProcessStartInfo("dotnet", "--version")
    {
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
        CreateNoWindow         = true
    };
    using var p = Process.Start(psi);
    if (p is not null)
    {
        version = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit(5000);
    }
}
catch
{
    // dotnet not on PATH
    version = null;
}

if (string.IsNullOrEmpty(version))
{
    EmitMissingWarning();
    return;
}

if (!MeetsMinimum(version, RequiredMajor, RequiredMinor, RequiredPatch))
{
    EmitTooOldWarning(version);
    return;
}

// SDK is fine — exit silently.


static void EmitMissingWarning()
{
    Console.WriteLine();
    Console.WriteLine($"{Red}{Bold}!!! [wpf-dev-pack] .NET SDK NOT FOUND !!!{Reset}");
    Console.WriteLine();
    Console.WriteLine($"{Red}wpf-dev-pack hooks are C# file-based apps and REQUIRE{Reset}");
    Console.WriteLine($"{Red}.NET SDK {RequiredMajor}.{RequiredMinor}.{RequiredPatch} or higher.{Reset}");
    Console.WriteLine();
    Console.WriteLine($"{Red}Install from: https://dotnet.microsoft.com/download/dotnet/{RequiredMajor}.{RequiredMinor}{Reset}");
    Console.WriteLine();
    Console.WriteLine($"{Red}Until installed, hooks (FeedbackDocAuditor, XamlValidator,{Reset}");
    Console.WriteLine($"{Red}MvvmViolationDetector, CodeFormatter, LanguagePreferenceLoader,{Reset}");
    Console.WriteLine($"{Red}etc.) will not run.{Reset}");
    Console.WriteLine();
}

static void EmitTooOldWarning(string found)
{
    Console.WriteLine();
    Console.WriteLine($"{Red}{Bold}!!! [wpf-dev-pack] .NET SDK TOO OLD !!!{Reset}");
    Console.WriteLine();
    Console.WriteLine($"{Red}  Found:    {found}{Reset}");
    Console.WriteLine($"{Red}  Required: {RequiredMajor}.{RequiredMinor}.{RequiredPatch} or higher{Reset}");
    Console.WriteLine();
    Console.WriteLine($"{Red}Update from: https://dotnet.microsoft.com/download/dotnet/{RequiredMajor}.{RequiredMinor}{Reset}");
    Console.WriteLine();
    Console.WriteLine($"{Red}Until updated, wpf-dev-pack hooks may misbehave due to{Reset}");
    Console.WriteLine($"{Red}file-based-app feature gaps in older SDKs.{Reset}");
    Console.WriteLine();
}

static bool MeetsMinimum(string version, int reqMajor, int reqMinor, int reqPatch)
{
    // Tolerate suffixes like "10.0.300-preview.7" by splitting on the
    // first '-' before parsing the patch field.
    var parts = version.Split('.');
    if (parts.Length < 3) return false;
    if (!int.TryParse(parts[0], out var major)) return false;
    if (!int.TryParse(parts[1], out var minor)) return false;
    var patchToken = parts[2].Split('-', '+')[0];
    if (!int.TryParse(patchToken, out var patch)) return false;

    if (major != reqMajor) return major > reqMajor;
    if (minor != reqMinor) return minor > reqMinor;
    return patch >= reqPatch;
}
