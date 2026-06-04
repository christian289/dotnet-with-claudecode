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
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WpfTopicTools>();

await builder.Build().RunAsync();
