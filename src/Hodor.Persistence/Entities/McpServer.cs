namespace Hodor.Persistence.Entities;

/// <summary>
/// Registered MCP server.
/// </summary>
public class McpServer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Command { get; set; }
    public string? Args { get; set; }
    public string? Env { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<McpToolEntity> Tools { get; set; } = [];
}
