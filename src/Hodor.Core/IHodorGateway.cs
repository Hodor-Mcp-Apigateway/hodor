namespace Hodor.Core;

using Hodor.Core.Mcp;

/// <summary>
/// MCP Gateway - tool discovery and execution.
/// </summary>
public interface IHodorGateway
{
    Task<IReadOnlyList<McpTool>> ListToolsAsync(CancellationToken cancellationToken);
    Task<object?> CallToolAsync(string toolName, object? arguments, CancellationToken cancellationToken);
}

