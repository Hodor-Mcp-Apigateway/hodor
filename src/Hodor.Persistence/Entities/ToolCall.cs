namespace Hodor.Persistence.Entities;

/// <summary>
/// Tool execution history.
/// </summary>
public class ToolCall
{
    public Guid Id { get; set; }
    public Guid McpToolId { get; set; }
    public required string ArgumentsJson { get; set; }
    public string? ResultJson { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }

    public McpToolEntity McpTool { get; set; } = null!;
}
