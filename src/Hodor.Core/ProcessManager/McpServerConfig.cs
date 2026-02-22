using System.Text.Json.Serialization;

namespace Hodor.Core.ProcessManager;

/// <summary>
/// MCP server configuration (mcp-config.json).
/// </summary>
public class McpConfigRoot
{
    [JsonPropertyName("mcpServers")]
    public IDictionary<string, McpServerConfig> McpServers { get; set; } = new Dictionary<string, McpServerConfig>();
}

/// <summary>
/// Single MCP server configuration.
/// </summary>
public class McpServerConfig
{
    [JsonPropertyName("command")]
    public required string Command { get; set; }

    [JsonPropertyName("args")]
    public string[] Args { get; set; } = [];

    [JsonPropertyName("env")]
    public IDictionary<string, string>? Env { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "cold"; // "hot" | "cold"

    [JsonPropertyName("idle_timeout")]
    public int IdleTimeoutSeconds { get; set; } = 120;

    [JsonPropertyName("min_ttl")]
    public int MinTtlSeconds { get; set; } = 60;

    [JsonPropertyName("max_ttl")]
    public int MaxTtlSeconds { get; set; } = 3600;
}
