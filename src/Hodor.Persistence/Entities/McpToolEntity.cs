using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace Hodor.Persistence.Entities;

/// <summary>
/// MCP tool with optional embedding for semantic search.
/// </summary>
public class McpToolEntity
{
    public Guid Id { get; set; }
    public Guid McpServerId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? InputSchemaJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "vector(1536)")]
    public Vector? Embedding { get; set; }

    public McpServer McpServer { get; set; } = null!;
}
