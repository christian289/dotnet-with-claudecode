---
description: "One-button generator for a complete, runnable streaming LLM chat client in WPF (CommunityToolkit.Mvvm): markdown bubbles, provider-agnostic IChatClient with a shared HttpClient pool, MCP tools, Enter-to-send, and pin-to-bottom auto-scroll — defaulting to a keyless mock provider so it runs immediately. Use when adding a chat/assistant panel to a WPF app. Usage: /wpf-dev-pack:make-wpf-chatclient <Name>"
argument-hint: [Name]
---

# WPF Chat Client Generator (one-button)

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the chat feature name (e.g., Chat, Assistant, Copilot)". Do NOT proceed until a valid name is provided. Use the response as the feature name for all subsequent steps.**

This is the **self-contained, one-button** scaffolder: a single invocation emits an entire working streaming chat client and wires it into the app via ViewModel First (Mappings.xaml + DI). It composes the four component scaffolders and adds the glue (input behavior, auto-scroll behavior, view, view-model, DI) they don't cover. It defaults to a **mock provider with no MCP**, so the generated app builds and streams a reply with no API key and no server.

- Replace `{Namespace}` with the project's root namespace, `{Project}` with the
  `*.WpfApp` path, and `{ViewModels}` with the `*.ViewModels` project.
- Target app must be a `make-wpf-project` solution (`.WpfApp` + `.ViewModels`),
  CommunityToolkit.Mvvm + GenericHost. (Prism users: substitute `BindableBase` /
  `DelegateCommand` / `RegisterForNavigation` per `view-viewmodel-wiring-prism.md`.)

> This skill is the one-button equivalent of running, in one pass,
> `make-wpf-markdown-presenter` + `make-wpf-chat-bubble-template` +
> `make-wpf-chatclient-factory` + `make-wpf-chat-orchestrator`, plus the view,
> view-model, and behaviors below. The full rationale for every piece lives in
> the knowledge topics listed under "Related".

## ⚠️ Verify SDK signatures with HandMirror before emitting provider/MCP code

The factory and orchestrator call version-sensitive APIs across several SDKs.
Before emitting them, verify signatures with HandMirror
(`inspect_nuget_package` / `inspect_nuget_package_type`): `Microsoft.Extensions.AI`,
`ModelContextProtocol`, and any real providers you enable (`OpenAI`, `OllamaSharp`,
`Anthropic.SDK`, `Google.GenAI`, `System.ClientModel`). The mock path needs none of these.

## Workflow

Copy this checklist and emit each file in order:

```
ChatClient Scaffold Progress:
- [ ] 1. Packages: add the required PackageReferences to {Project}
- [ ] 2. Components: emit MarkdownPresenter, ChatClientFactory(+Mock), ChatOrchestrator(+ChatEvent, McpToolService)
- [ ] 3. Behaviors: emit EnterCommandBehavior + StickToBottomBehavior
- [ ] 4. Turn VM: emit ChatTurnViewModel (IsUser, Markdown, IsWaiting)
- [ ] 5. Chat VM: emit ChatViewModel
- [ ] 6. View: emit ChatView.xaml (bubbles + input)
- [ ] 7. Wire: register DI (singleton factory + orchestrator + ChatViewModel), add Mappings.xaml entry
- [ ] 8. Verify: dotnet build; the mock provider streams a reply with no key
```

## 1. Required Packages (in {Project} = *.WpfApp)

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.*" />
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.*" />
<PackageReference Include="Markdig" Version="0.37.*" />
<PackageReference Include="Microsoft.Extensions.AI" Version="9.*" />
<PackageReference Include="ModelContextProtocol" Version="0.*" />
<!-- Real providers are optional; the default mock path needs none of them. -->
```

## 2. Components (emit using the canonical templates)

Emit these files now (this is what makes the skill self-contained — do not ask
the user to run the component commands). Use the exact code from each component
scaffolder / knowledge topic:

| File(s) | Source template | Knowledge topic |
|---------|-----------------|-----------------|
| `Controls/MarkdownPresenter.cs` | `make-wpf-markdown-presenter` | `rendering-markdown-in-wpf`, `displaying-selectable-rich-text-in-wpf` |
| `Services/ChatClientFactory.cs`, `IChatClientFactory.cs`, `ChatSettings.cs`, `Provider.cs`, `MockChatClient.cs` | `make-wpf-chatclient-factory` | `sharing-httpclient-across-llm-sdks` |
| `Services/ChatOrchestrator.cs`, `ChatEvent.cs`, `McpToolService.cs` | `make-wpf-chat-orchestrator` | `hosting-extensions-ai-chatclient-in-wpf-mvvm`, `consuming-mcp-tools-in-extensions-ai` |

## 3. Behaviors

### Behaviors/EnterCommandBehavior.cs (Enter=send, Shift+Enter=newline, IME-safe)

```csharp
namespace {Namespace}.Behaviors;

public sealed class EnterCommandBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EnterCommandBehavior), new(null));
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(EnterCommandBehavior), new(null));
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttached()  => AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    protected override void OnDetaching() => AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) { return; }
        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) { return; } // Shift+Enter -> newline (default)
        if (e.Key == Key.ImeProcessed) { return; }                     // ignore IME candidate confirm
        if (Command?.CanExecute(CommandParameter) == true) { Command.Execute(CommandParameter); }
        e.Handled = true;                                              // suppress the newline insert
    }
}
```

### Behaviors/StickToBottomBehavior.cs (pin to newest unless scrolled up)

```csharp
namespace {Namespace}.Behaviors;

public sealed class StickToBottomBehavior : Behavior<ScrollViewer>
{
    private bool _pinned = true;

    protected override void OnAttached()  => AssociatedObject.ScrollChanged += OnScrollChanged;
    protected override void OnDetaching() => AssociatedObject.ScrollChanged -= OnScrollChanged;

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var sv = AssociatedObject;
        if (e.ExtentHeightChange == 0)
        {
            // user moved (no content growth): recompute whether we're at the bottom
            _pinned = sv.VerticalOffset >= sv.ScrollableHeight - 1.0;
        }
        else if (_pinned)
        {
            // content grew while pinned: follow it
            sv.ScrollToVerticalOffset(sv.ScrollableHeight);
        }
    }
}
```

## 4. ViewModels/ChatTurnViewModel.cs (one per turn)

```csharp
namespace {Namespace}.ViewModels;

public sealed partial class ChatTurnViewModel : ObservableObject
{
    [ObservableProperty] private bool _isUser;
    [ObservableProperty] private bool _isWaiting;          // "waiting…" until the first token
    [ObservableProperty] private string _markdown = string.Empty;

    public void AppendMarkdown(string delta) => Markdown += delta;
}
```

## 5. ViewModels/ChatViewModel.cs

```csharp
namespace {Namespace}.ViewModels;

public sealed partial class ChatViewModel : ObservableObject
{
    private readonly ChatOrchestrator _orchestrator;
    private readonly List<ChatMessage> _history = [];
    private CancellationTokenSource? _cts;
    private ChatTurnViewModel? _current;

    public ObservableCollection<ChatTurnViewModel> Turns { get; } = [];

    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    [ObservableProperty] private string _input = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [ObservableProperty] private bool _isStreaming;

    public ChatViewModel(ChatOrchestrator orchestrator) => _orchestrator = orchestrator;

    private bool CanSend() => !IsStreaming && !string.IsNullOrWhiteSpace(Input);

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        string prompt = Input.Trim();
        Input = string.Empty;

        Turns.Add(new ChatTurnViewModel { IsUser = true, Markdown = prompt });
        _history.Add(new ChatMessage(ChatRole.User, prompt));

        _current = new ChatTurnViewModel { IsUser = false, IsWaiting = true };
        Turns.Add(_current);

        IsStreaming = true;
        _cts = new CancellationTokenSource();
        try
        {
            await foreach (ChatEvent ev in _orchestrator.SendAsync(_history, _cts.Token))
            {
                Application.Current.Dispatcher.Invoke(() => Apply(ev)); // marshal before touching bound state
            }
            _history.Add(new ChatMessage(ChatRole.Assistant, _current.Markdown));
        }
        catch (OperationCanceledException) { /* user pressed Stop — normal, not an error */ }
        finally
        {
            if (_current is not null) { _current.IsWaiting = false; }
            IsStreaming = false;
            _cts?.Dispose(); _cts = null;
        }
    }

    private bool CanStop() => IsStreaming;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _cts?.Cancel();

    private void Apply(ChatEvent ev)
    {
        switch (ev)
        {
            case ChatText t:     _current!.AppendMarkdown(t.Text); _current.IsWaiting = false; break;
            case ToolStarted s:  _current!.AppendMarkdown($"\n\n*Calling `{s.Name}`…*\n\n"); break;
            case ToolCompleted:  break; // optionally surface the tool result
            case ChatFailed f:   _current!.AppendMarkdown($"\n\n> {f.Error.Message}"); break;
        }
    }
}
```

> ViewModels reference `ChatMessage`/`ChatRole` from `Microsoft.Extensions.AI`
> and `ChatEvent`/`ChatOrchestrator` from the app's `Services` namespace. Add a
> `ProjectReference` from `{ViewModels}` to whichever project owns `ChatOrchestrator`,
> or place the orchestrator + events in `{ViewModels}`. `Application.Current.Dispatcher`
> is the one allowed `System.Windows` touch here; if strict MVVM purity is required,
> inject an `IDispatcher` abstraction instead.

## 6. Views/ChatView.xaml

```xml
<UserControl x:Class="{Namespace}.Views.ChatView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:b="http://schemas.microsoft.com/xaml/behaviors"
             xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
             xmlns:behaviors="clr-namespace:{Namespace}.Behaviors"
             xmlns:controls="clr-namespace:{Namespace}.Controls"
             xmlns:vm="clr-namespace:{Namespace}.ViewModels;assembly={ViewModels}"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:ChatViewModel}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Conversation: pin-to-bottom unless the user scrolled up -->
        <ScrollViewer Grid.Row="0" VerticalScrollBarVisibility="Auto">
            <b:Interaction.Behaviors>
                <behaviors:StickToBottomBehavior/>
            </b:Interaction.Behaviors>
            <ItemsControl ItemsSource="{Binding Turns}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate DataType="{x:Type vm:ChatTurnViewModel}">
                        <Border Padding="10" CornerRadius="6">
                            <Border.Style>
                                <Style TargetType="Border">
                                    <!-- DEFAULT = ASSISTANT: left, card background, right gutter -->
                                    <Setter Property="HorizontalAlignment" Value="Left"/>
                                    <Setter Property="Margin" Value="0,4,48,0"/>
                                    <Setter Property="Background" Value="{ui:ThemeResource CardBackgroundFillColorDefaultBrush}"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding IsUser}" Value="True">
                                            <!-- USER: right, accent background, left gutter -->
                                            <Setter Property="HorizontalAlignment" Value="Right"/>
                                            <Setter Property="Margin" Value="48,4,0,0"/>
                                            <Setter Property="Background" Value="{ui:ThemeResource SystemAccentColorPrimaryBrush}"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Border.Style>
                            <controls:MarkdownPresenter Markdown="{Binding Markdown}"/>
                        </Border>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>

        <!-- Input: Enter=send, Shift+Enter=newline -->
        <Grid Grid.Row="1" Margin="0,8,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            <TextBox Grid.Column="0" AcceptsReturn="True" MaxLines="4"
                     Text="{Binding Input, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
                <b:Interaction.Behaviors>
                    <behaviors:EnterCommandBehavior Command="{Binding SendCommand}"/>
                </b:Interaction.Behaviors>
            </TextBox>
            <Button Grid.Column="1" Margin="6,0,0,0" Content="Send" Command="{Binding SendCommand}"/>
            <Button Grid.Column="2" Margin="6,0,0,0" Content="Stop" Command="{Binding StopCommand}"/>
        </Grid>
    </Grid>
</UserControl>
```

```csharp
namespace {Namespace}.Views;

public partial class ChatView : UserControl
{
    public ChatView() => InitializeComponent();
}
```

> The `ui:ThemeResource` brushes require WPF-UI (`Wpf.Ui`); for stock WPF, swap
> them per `make-wpf-chat-bubble-template`. An invalid theme-resource key throws
> at render time, not at build.

## 7. Wire DI + Mappings.xaml (ViewModel First)

In `App.xaml.cs` `ConfigureServices` — the factory MUST be a singleton (shared
HttpClient pool), and the default settings use `Provider.Mock` so it runs keyless:

```csharp
// Shared HttpClient pool — singleton is part of the pattern's correctness.
services.AddSingleton<IChatClientFactory, ChatClientFactory>();

services.AddSingleton(sp =>
{
    var settings = new ChatSettings { Provider = Provider.Mock, ModelId = "mock" };
    IChatClient client = sp.GetRequiredService<IChatClientFactory>().Create(settings, apiKey: null);
    return new ChatOrchestrator(client, tools: [], settings); // no MCP tools in the mock default
});

services.AddSingleton<ChatViewModel>();
```

Append one entry to `Mappings.xaml` (the View is resolved from the ViewModel
type — no `x:Key`, ViewModel First; `ViewModelLocator` / code-behind
`DataContext` are prohibited, see `prohibitions.md`):

```xml
<DataTemplate DataType="{x:Type vm:ChatViewModel}">
    <views:ChatView />
</DataTemplate>
```

Show the chat by assigning the shell's `CurrentViewModel`:
`CurrentViewModel = host.Services.GetRequiredService<ChatViewModel>();`

## 8. Going live (beyond the mock)

- **Real provider**: change `ChatSettings.Provider`/`ModelId`/`BaseUrl`, and pass
  the API key from the OS credential store — **never** config. See
  `storing-api-keys-and-binding-passwordbox-in-wpf` (and bind the key field with
  the attached behavior, since `PasswordBox.Password` is not a DP).
- **Settings UI**: a provider/model/BaseURL/key panel with per-provider
  conditional fields is covered by `building-a-provider-settings-panel`.
- **MCP tools**: call `McpToolService.ConnectAsync(...)` once at startup and pass
  `[.. mcpToolService.Tools]` into the `ChatOrchestrator`; dispose it OFF the
  Dispatcher in `App.OnExit` (`consuming-mcp-tools-in-extensions-ai`).
- **Long histories**: virtualize the conversation list — see the variable-height
  selectable-bubble caveats in `virtualizing-wpf-ui`.

## File Location

```
{Project} (*.WpfApp)/
├── Behaviors/
│   ├── EnterCommandBehavior.cs
│   └── StickToBottomBehavior.cs
├── Controls/
│   └── MarkdownPresenter.cs
├── Services/
│   ├── ChatClientFactory.cs   IChatClientFactory.cs   ChatSettings.cs   Provider.cs
│   ├── MockChatClient.cs
│   ├── ChatOrchestrator.cs    ChatEvent.cs            McpToolService.cs
├── Views/
│   ├── ChatView.xaml
│   └── ChatView.xaml.cs
└── Mappings.xaml              (+1 DataTemplate)
{ViewModels} (*.ViewModels)/
├── ChatViewModel.cs
└── ChatTurnViewModel.cs
```

## Related

- Component scaffolders: `/wpf-dev-pack:make-wpf-markdown-presenter`,
  `/wpf-dev-pack:make-wpf-chat-bubble-template`,
  `/wpf-dev-pack:make-wpf-chatclient-factory`,
  `/wpf-dev-pack:make-wpf-chat-orchestrator`,
  `/wpf-dev-pack:make-wpf-behavior` (the two behaviors above are instances of it).
- Knowledge (fetch via `WpfDevPackMcp get_wpf_topic`): `rendering-markdown-in-wpf`,
  `displaying-selectable-rich-text-in-wpf`, `styling-chat-bubbles-in-wpf`,
  `hosting-extensions-ai-chatclient-in-wpf-mvvm`, `sharing-httpclient-across-llm-sdks`,
  `consuming-mcp-tools-in-extensions-ai`, `storing-api-keys-and-binding-passwordbox-in-wpf`,
  `building-a-provider-settings-panel`, `virtualizing-wpf-ui`.
