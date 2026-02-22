namespace Hodor.Core.ProcessManager;

/// <summary>
/// Manages MCP server processes (HOT/COLD, spawn, idle timeout).
/// </summary>
public interface IMcpProcessManager
{
    Task<IReadOnlyList<ServerStatus>> GetServerStatusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(string serverName, CancellationToken cancellationToken = default);
    Task<object?> CallToolAsync(string serverName, string toolName, object? arguments, CancellationToken cancellationToken = default);
    Task<object?> GetToolSchemaAsync(string serverName, string toolName, CancellationToken cancellationToken = default);
    Task EnsureServerRunningAsync(string serverName, bool forceEnable = false, CancellationToken cancellationToken = default);
    Task StopServerAsync(string serverName, CancellationToken cancellationToken = default);
    IReadOnlyDictionary<string, McpServerConfig> GetAllServerConfigs();
    IReadOnlyList<string> GetServerLogs(string serverName, int maxLines = 100);
    /// <summary>
    /// Forwards MCP request (prompts/list, prompts/get, resources/list, resources/read) to backend servers.
    /// Tries each enabled server, aggregates list results, returns first success for get/read.
    /// </summary>
    Task<object?> ForwardMcpRequestAsync(string method, object? @params, CancellationToken cancellationToken = default);
}

/// <summary>
/// MCP server status.
/// </summary>
public record ServerStatus(
    string Name,
    string Status, // "running", "stopped", "starting"
    int ToolCount,
    string Mode,   // "hot", "cold"
    bool IsEnabled,
    TimeSpan? Uptime = null
);

/// <summary>
/// Tool info from an MCP server.
/// </summary>
public record McpToolInfo(
    string ServerName,
    string ToolName,
    string FullName,  // server:tool
    string? Description,
    object? InputSchema
);
