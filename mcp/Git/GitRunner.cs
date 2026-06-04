using System.Diagnostics;

namespace WpfDevPackMcp.Git;

/// <summary>Result of a single git invocation.</summary>
public sealed record GitResult(bool Success, string StdOut, string StdErr);

/// <summary>Thin best-effort wrapper around the `git` executable.</summary>
public sealed class GitRunner
{
    public bool IsGitAvailable()
    {
        try
        {
            return Run(Environment.CurrentDirectory, "--version").Success;
        }
        catch
        {
            // git missing from PATH or process start blocked — treat as unavailable.
            return false;
        }
    }

    public GitResult Run(string workingDir, params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDir,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        // Never block on an interactive credential/auth prompt — fail fast instead.
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        foreach (var a in args)
        {
            psi.ArgumentList.Add(a);
        }

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start git process.");
        // Hand git an immediately-closed stdin so it never inherits/blocks on the
        // MCP server's own stdin pipe.
        proc.StandardInput.Close();
        // Drain stderr on a background thread while reading stdout on this one, so
        // neither stream's pipe buffer can fill and deadlock the other. We avoid
        // async-over-sync (ReadToEndAsync().GetAwaiter().GetResult()) because that
        // can deadlock when the MCP SDK invokes a synchronous tool handler on a
        // constrained execution context.
        var stderr = string.Empty;
        var errThread = new Thread(() => stderr = proc.StandardError.ReadToEnd()) { IsBackground = true };
        errThread.Start();
        var stdout = proc.StandardOutput.ReadToEnd();
        errThread.Join();
        proc.WaitForExit();
        return new GitResult(proc.ExitCode == 0, stdout, stderr);
    }
}
