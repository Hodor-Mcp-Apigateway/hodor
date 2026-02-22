using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hodor.Persistence.Migrations;

[DbContext(typeof(HodorDbContext))]
sealed partial class HodorDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("vector")
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("Hodor.Persistence.Entities.McpServer", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<string>("Args")
                .HasMaxLength(2048)
                .HasColumnType("character varying(2048)");

            b.Property<string>("Command")
                .IsRequired()
                .HasMaxLength(1024)
                .HasColumnType("character varying(1024)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Env")
                .HasColumnType("text");

            b.Property<bool>("IsEnabled")
                .HasColumnType("boolean");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnType("character varying(256)");

            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("Name")
                .IsUnique();

            b.ToTable("McpServers");
        });

        modelBuilder.Entity("Hodor.Persistence.Entities.McpToolEntity", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Description")
                .HasMaxLength(2048)
                .HasColumnType("character varying(2048)");

            b.Property<object>("Embedding")
                .HasColumnType("vector(1536)");

            b.Property<string>("InputSchemaJson")
                .HasMaxLength(8192)
                .HasColumnType("character varying(8192)");

            b.Property<Guid>("McpServerId")
                .HasColumnType("uuid");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnType("character varying(256)");

            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("Embedding")
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            b.HasIndex("McpServerId", "Name")
                .IsUnique();

            b.HasOne("Hodor.Persistence.Entities.McpServer", "McpServer")
                .WithMany("Tools")
                .HasForeignKey("McpServerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.ToTable("McpTools");
        });

        modelBuilder.Entity("Hodor.Persistence.Entities.ToolCall", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<string>("ArgumentsJson")
                .IsRequired()
                .HasMaxLength(16384)
                .HasColumnType("character varying(16384)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("DurationMs")
                .HasColumnType("integer");

            b.Property<string>("ErrorMessage")
                .HasMaxLength(4096)
                .HasColumnType("character varying(4096)");

            b.Property<Guid>("McpToolId")
                .HasColumnType("uuid");

            b.Property<string>("ResultJson")
                .HasMaxLength(65536)
                .HasColumnType("character varying(65536)");

            b.HasKey("Id");

            b.HasIndex("CreatedAt");

            b.HasIndex("McpToolId");

            b.HasOne("Hodor.Persistence.Entities.McpToolEntity", "McpTool")
                .WithMany()
                .HasForeignKey("McpToolId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.ToTable("ToolCalls");
        });
    }
}
