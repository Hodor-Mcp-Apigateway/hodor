using System.Text.Json;
using Hodor.Core;
using Hodor.Core.Mcp;
using Hodor.Core.ProcessManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodor.Application.Mcp;

/// <summary>
/// MCP Gateway - Dynamic MCP with hodor-find, hodor-exec, hodor-schema.
/// </summary>
public class HodorGatewayService : IHodorGateway
{
    private readonly ILogger<HodorGatewayService> _logger;
    private readonly IMcpProcessManager _processManager;
    private readonly bool _dynamicMcp;

    public HodorGatewayService(
        ILogger<HodorGatewayService> logger,
        IMcpProcessManager processManager,
        IConfiguration configuration)
    {
        _logger = logger;
        _processManager = processManager;
        _dynamicMcp = configuration.GetValue("DynamicMcp", true);
    }

    public async Task<IReadOnlyList<McpTool>> ListToolsAsync(CancellationToken cancellationToken)
    {
        if (_dynamicMcp)
        {
            return new List<McpTool>
            {
                new("hodor-find", "Search for tools by name, description, or server", new { type = "object", properties = new { query = new { type = "string" } } }),
                new("hodor-exec", "Execute any tool by server:tool_name", new { type = "object", properties = new { tool = new { type = "string" }, arguments = new { type = "object" } } }),
                new("hodor-schema", "Get full input schema for a tool", new { type = "object", properties = new { tool = new { type = "string" } } })
            };
        }

        var all = await _processManager.ListToolsAsync(cancellationToken);
        return all.Select(t => new McpTool(t.FullName, t.Description, t.InputSchema)).ToList();
    }

    public async Task<object?> CallToolAsync(string toolName, object? arguments, CancellationToken cancellationToken)
    {
        if (_dynamicMcp && toolName.StartsWith("hodor-", StringComparison.OrdinalIgnoreCase))
            return await HandleMetaToolAsync(toolName, arguments, cancellationToken);

        var (server, tool) = ParseServerTool(toolName);
        return await _processManager.CallToolAsync(server, tool, arguments, cancellationToken);
    }

    private async Task<object?> HandleMetaToolAsync(string toolName, object? arguments, CancellationToken cancellationToken)
    {
        var args = arguments as JsonElement? ?? JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(arguments ?? new { }));
        if (toolName.Equals("hodor-find", StringComparison.OrdinalIgnoreCase))
        {
            var query = args.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "";
            var all = (await _processManager.ListToolsAsync(cancellationToken)).ToList();
            foreach (var (name, cfg) in _processManager.GetAllServerConfigs())
            {
                if (!cfg.Enabled)
                    all.Add(new McpToolInfo(name, "(disabled - auto-enable via hodor-exec)", $"{name}:(disabled)", null, null));
            }
            var filtered = string.IsNullOrWhiteSpace(query)
                ? all
                : all.Where(t =>
                    t.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    t.ServerName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            return new { tools = filtered.Select(t => new { t.FullName, t.Description, t.ServerName }) };
        }

        if (toolName.Equals("hodor-exec", StringComparison.OrdinalIgnoreCase))
        {
            var tool = args.TryGetProperty("tool", out var t) ? t.GetString() ?? "" : "";
            object? toolArgs = null;
            if (args.TryGetProperty("arguments", out var a) && a.ValueKind != JsonValueKind.Null && a.ValueKind != JsonValueKind.Undefined)
                toolArgs = JsonSerializer.Deserialize<object>(a.GetRawText());
            var (server, toolNameOnly) = ParseServerTool(tool);
            return await _processManager.CallToolAsync(server, toolNameOnly, toolArgs, cancellationToken);
        }

        if (toolName.Equals("hodor-schema", StringComparison.OrdinalIgnoreCase))
        {
            var tool = args.TryGetProperty("tool", out var t) ? t.GetString() ?? "" : "";
            var (server, toolNameOnly) = ParseServerTool(tool);
            return await _processManager.GetToolSchemaAsync(server, toolNameOnly, cancellationToken);
        }

        throw new InvalidOperationException($"Unknown meta-tool: {toolName}");
    }

    private static (string Server, string Tool) ParseServerTool(string fullName)
    {
        var idx = fullName.IndexOf(':');
        if (idx < 0) throw new ArgumentException($"Tool must be server:tool_name, got: {fullName}", nameof(fullName));
        return (fullName[..idx], fullName[(idx + 1)..]);
    }
}
