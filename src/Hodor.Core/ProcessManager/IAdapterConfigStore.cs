namespace Hodor.Core.ProcessManager;

/// <summary>
/// Mutable store for MCP adapter (server) configuration.
/// Supports CRUD and persistence to mcp-config.json.
/// </summary>
public interface IAdapterConfigStore
{
    IReadOnlyDictionary<string, McpServerConfig> GetAll();
    McpServerConfig? Get(string name);
    void Add(string name, McpServerConfig config);
    void Update(string name, McpServerConfig config);
    bool Remove(string name);
    Task PersistAsync(CancellationToken cancellationToken = default);
}
