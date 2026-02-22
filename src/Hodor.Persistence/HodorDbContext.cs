using Hodor.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hodor.Persistence;

public class HodorDbContext : DbContext
{
    public HodorDbContext(DbContextOptions<HodorDbContext> options) : base(options) { }

    public DbSet<McpServer> McpServers => Set<McpServer>();
    public DbSet<McpToolEntity> McpTools => Set<McpToolEntity>();
    public DbSet<ToolCall> ToolCalls => Set<ToolCall>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<McpServer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256);
            e.Property(x => x.Command).HasMaxLength(1024);
            e.Property(x => x.Args).HasMaxLength(2048);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<McpToolEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256);
            e.Property(x => x.Description).HasMaxLength(2048);
            e.Property(x => x.InputSchemaJson).HasMaxLength(8192);
            e.HasIndex(x => new { x.McpServerId, x.Name }).IsUnique();
            e.HasOne(x => x.McpServer).WithMany(s => s.Tools).HasForeignKey(x => x.McpServerId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(i => i.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops")
                .HasStorageParameter("m", 16)
                .HasStorageParameter("ef_construction", 64);
        });

        modelBuilder.Entity<ToolCall>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ArgumentsJson).HasMaxLength(16384);
            e.Property(x => x.ResultJson).HasMaxLength(65536);
            e.Property(x => x.ErrorMessage).HasMaxLength(4096);
            e.HasOne(x => x.McpTool).WithMany().HasForeignKey(x => x.McpToolId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
