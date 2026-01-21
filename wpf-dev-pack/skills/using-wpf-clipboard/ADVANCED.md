# Advanced WPF Clipboard Patterns

## Clipboard Monitoring

### Watching Clipboard Changes

```csharp
namespace MyApp.Services;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public sealed class ClipboardMonitor : IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private HwndSource? _hwndSource;
    private IntPtr _hwnd;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    public event EventHandler? ClipboardChanged;

    public void Start(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.Handle;
        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        AddClipboardFormatListener(_hwnd);
    }

    public void Stop()
    {
        if (_hwnd != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(_hwnd);
        }

        _hwndSource?.RemoveHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Stop();
    }
}
```

### Usage

```csharp
public partial class MainWindow : Window
{
    private readonly ClipboardMonitor _clipboardMonitor = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _clipboardMonitor.ClipboardChanged += OnClipboardChanged;
        _clipboardMonitor.Start(this);
    }

    private void OnClipboardChanged(object? sender, EventArgs e)
    {
        // Handle clipboard change
        if (Clipboard.ContainsText())
        {
            var text = Clipboard.GetText();
            UpdatePreview(text);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _clipboardMonitor.Dispose();
    }
}
```

---

## Error Handling

```csharp
using System;
using System.Runtime.InteropServices;
using System.Windows;

public static class SafeClipboard
{
    public static bool TrySetText(string text)
    {
        try
        {
            Clipboard.SetText(text);
            return true;
        }
        catch (ExternalException)
        {
            // Clipboard is locked by another process
            return false;
        }
    }

    public static string? TryGetText()
    {
        try
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : null;
        }
        catch (ExternalException)
        {
            return null;
        }
    }

    public static bool TrySetTextWithRetry(string text, int maxRetries = 3, int delayMs = 100)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (ExternalException)
            {
                if (i < maxRetries - 1)
                {
                    System.Threading.Thread.Sleep(delayMs);
                }
            }
        }

        return false;
    }
}
```
