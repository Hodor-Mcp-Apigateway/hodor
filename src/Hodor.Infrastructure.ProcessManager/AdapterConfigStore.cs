using System.Text.Json;
using Hodor.Core.ProcessManager;

namespace Hodor.Infrastructure.ProcessManager;

public sealed class AdapterConfigStore : IAdapterConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly McpConfigRoot _config;
    private readonly string _configPath;

    public AdapterConfigStore(McpConfigRoot config, string configPath)
    {
        _config = config;
        _configPath = configPath;
    }

    public IReadOnlyDictionary<string, McpServerConfig> GetAll() =>
        (IReadOnlyDictionary<string, McpServerConfig>)_config.McpServers;

    public McpServerConfig? Get(string name) =>
        _config.McpServers.TryGetValue(name, out var c) ? c : null;

    public void Add(string name, McpServerConfig config) => _config.McpServers[name] = config;
    public void Update(string name, McpServerConfig config) => _config.McpServers[name] = config;
    public bool Remove(string name) => _config.McpServers.Remove(name);

    public async Task PersistAsync(CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(_config, JsonOptions);
        await File.WriteAllTextAsync(_configPath, json, ct);
    }
}
