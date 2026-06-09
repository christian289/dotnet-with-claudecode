using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WpfDevPackMcp.Configuration;
using WpfDevPackMcp.Git;
using WpfDevPackMcp.Knowledge;
using WpfDevPackMcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

// CRITICAL: stdout is reserved for MCP JSON-RPC. All logs go to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<ConfigStore>(_ => new ConfigStore());
builder.Services.AddSingleton<GitRunner>();
builder.Services.AddSingleton<KnowledgeService>();

builder.Services
    .AddMcpServer(options => options.ServerInstructions =
        """
        This server is the single source of truth for WPF / .NET-desktop / XAML / MVVM
        knowledge in the wpf-dev-pack plugin. Before answering any question — or writing,
        reviewing, or debugging code — that touches WPF, XAML, the WPF rendering /
        threading / binding / routed-event model, MVVM, or the plugin's supported
        libraries (Nodify, WPF-UI, LiveCharts2, ScottPlot, FluentValidation, ErrorOr,
        CommunityToolkit.Mvvm, Prism, FlaUI, and similar), FIRST call search_wpf_topics
        with the key terms from the request, then load the most relevant results with
        get_wpf_topic. Prefer these curated topics over prior knowledge, which may be
        outdated. If the search returns nothing relevant, answer normally.
        """)
    .WithStdioServerTransport()
    .WithTools<WpfTopicTools>();

await builder.Build().RunAsync();
