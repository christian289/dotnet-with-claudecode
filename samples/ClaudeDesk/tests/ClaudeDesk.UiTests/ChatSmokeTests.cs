using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

using Xunit.Abstractions;

namespace ClaudeDesk.UiTests;

public sealed class ChatSmokeTests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation = new();
    private readonly ITestOutputHelper _output;

    public ChatSmokeTests(ITestOutputHelper output)
    {
        _output = output;
        string exe = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src", "ClaudeDesk.WpfApp", "bin", "Debug",
            "net10.0-windows", "ClaudeDesk.WpfApp.exe"));
        _app = Application.Launch(exe);
    }

    [Fact]
    public void SendMessage_StreamsMockReply()
    {
        var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        Assert.NotNull(window);

        var input = Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId("ChatInput"))?.AsTextBox(),
            timeout: TimeSpan.FromSeconds(10)).Result;
        Assert.NotNull(input);

        // ValuePattern instead of keyboard input: robust against focus loss and
        // blocked SendInput; the TwoWay PropertyChanged binding still fires.
        input!.Text = "Hello ClaudeDesk";

        var send = Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId("SendButton"))?.AsButton(),
            timeout: TimeSpan.FromSeconds(5)).Result;
        Assert.NotNull(send);

        // CanExecute flips once the binding pushes the text.
        var enabled = Retry.WhileFalse(() => send!.IsEnabled, timeout: TimeSpan.FromSeconds(5));
        Assert.True(enabled.Result, "Send button should enable once the input has text.");
        send!.Invoke();

        // Let the mock stream (~0.5 s) and the debounced render finish before
        // touching UIA again — continuous automation traffic during streaming
        // keeps the bubbles from materializing in the UIA tree.
        Thread.Sleep(3000);

        // The mock provider streams "This is a **mock** streamed reply." plus a
        // ```csharp code fence. Wait for the reply bubble document to carry the
        // text (UIA TextPattern: -1 = unlimited length), and for the rendered
        // code block's Copy button as a structural witness.
        var sawReply = Retry.WhileFalse(
            () =>
            {
                // Structural witness first: the rendered code block's Copy button.
                bool copySeen = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))
                    .Any(b => b.Name == "Copy");
                if (copySeen) { return true; }

                // Text witness second; isolate it so a flaky TextPattern read
                // cannot mask the structural check on the next iteration.
                try
                {
                    var docs = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Document));
                    return docs.Any(d =>
                        d.Patterns.Text.PatternOrDefault?.DocumentRange.GetText(-1)
                            .Contains("mock", StringComparison.OrdinalIgnoreCase) == true);
                }
                catch
                {
                    return false;
                }
            },
            timeout: TimeSpan.FromSeconds(20),
            interval: TimeSpan.FromMilliseconds(250),
            ignoreException: true);

        if (!sawReply.Result)
        {
            foreach (var desc in window.FindAllDescendants())
            {
                string cls = desc.Properties.ClassName.ValueOrDefault ?? "(null)";
                string ct = desc.Properties.ControlType.ValueOrDefault.ToString();
                _output.WriteLine($"  [{cls}/{ct}] Name=\"{desc.Name}\"");
            }
        }

        Assert.True(sawReply.Result, "Expected the streamed mock reply to appear in a chat bubble document.");
    }

    [Fact]
    public void InputCleared_AfterSend()
    {
        var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
        var input = Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId("ChatInput"))?.AsTextBox(),
            timeout: TimeSpan.FromSeconds(10)).Result!;

        input.Text = "ping";
        Button send = window.FindFirstDescendant(cf => cf.ByAutomationId("SendButton"))!.AsButton()!;
        var enabled = Retry.WhileFalse(() => send.IsEnabled, timeout: TimeSpan.FromSeconds(5));
        Assert.True(enabled.Result, "Send button should enable once the input has text.");
        send.Invoke();

        var cleared = Retry.WhileFalse(
            () => string.IsNullOrEmpty(input.Text),
            timeout: TimeSpan.FromSeconds(5));
        Assert.True(cleared.Result, "Input box should be cleared immediately after send.");
    }

    public void Dispose()
    {
        _app.Close();
        _automation.Dispose();
        _app.Dispose();
    }
}
