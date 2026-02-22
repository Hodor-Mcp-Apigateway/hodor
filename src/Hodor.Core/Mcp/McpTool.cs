namespace Hodor.Core.Mcp;

/// <summary>
/// MCP Tool definition (tools/list response).
/// </summary>
public record McpTool(
    string Name,
    string? Description,
    object? InputSchema
);
