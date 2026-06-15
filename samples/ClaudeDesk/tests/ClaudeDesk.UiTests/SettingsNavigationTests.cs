using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace ClaudeDesk.UiTests;

public sealed class SettingsNavigationTests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation = new();

    public SettingsNavigationTests()
    {
        string exe = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src", "ClaudeDesk.WpfApp", "bin", "Debug",
            "net10.0-windows", "ClaudeDesk.WpfApp.exe"));
        _app = Application.Launch(exe);
    }

    [Fact]
    public void Navigate_SettingsAndBack()
    {
        var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));

        var navSettings = Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId("NavSettingsButton"))?.AsButton(),
            timeout: TimeSpan.FromSeconds(10)).Result!;
        navSettings.Invoke();

        // The settings screen shows the provider combo and the Save button.
        var saveButton = Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId("SaveSettingsButton")),
            timeout: TimeSpan.FromSeconds(10)).Result;
        Assert.NotNull(saveButton);
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("ProviderCombo")));

        // Back to chat.
        window.FindFirstDescendant(cf => cf.ByAutomationId("NavChatButton"))!.AsButton()!.Invoke();
        var chatInput = Retry.WhileNull(
            () => window.FindFirstDescendant(cf => cf.ByAutomationId("ChatInput")),
            timeout: TimeSpan.FromSeconds(10)).Result;
        Assert.NotNull(chatInput);
    }

    public void Dispose()
    {
        _app.Close();
        _automation.Dispose();
        _app.Dispose();
    }
}
