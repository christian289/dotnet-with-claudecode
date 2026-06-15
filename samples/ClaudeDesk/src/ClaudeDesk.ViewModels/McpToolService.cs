namespace ClaudeDesk.ViewModels;

public sealed class McpToolService : IAsyncDisposable
{
    private McpClient? _mcp;
    public IList<McpClientTool> Tools { get; private set; } = [];

    // For a framework-dependent dll: Command="dotnet", Arguments=[dllPath].
    // For a self-contained exe: Command=exePath.
    public async Task ConnectAsync(string serverCommand, IList<string> serverArgs,
        ILoggerFactory? loggerFactory, CancellationToken ct)
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "tools",
            Command = serverCommand,
            Arguments = serverArgs,
        }, loggerFactory);

        // NOTE: there is NO McpClientFactory. McpClient is abstract — use the static CreateAsync.
        _mcp = await McpClient.CreateAsync(transport, clientOptions: null, loggerFactory: null, cancellationToken: ct);
        Tools = await _mcp.ListToolsAsync(cancellationToken: ct); // McpClientTool : AIFunction
    }

    public async ValueTask DisposeAsync()
    {
        if (_mcp is not null) { await _mcp.DisposeAsync(); }
    }
}
