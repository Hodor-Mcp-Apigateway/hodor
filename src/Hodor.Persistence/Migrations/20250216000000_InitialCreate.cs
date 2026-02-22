using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodor.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:vector", ",,");

        migrationBuilder.CreateTable(
            name: "McpServers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Command = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                Args = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                Env = table.Column<string>(type: "text", nullable: true),
                IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_McpServers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "McpTools",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                McpServerId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                InputSchemaJson = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Embedding = table.Column<object>(type: "vector(1536)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_McpTools", x => x.Id);
                table.ForeignKey(
                    name: "FK_McpTools_McpServers_McpServerId",
                    column: x => x.McpServerId,
                    principalTable: "McpServers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ToolCalls",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                McpToolId = table.Column<Guid>(type: "uuid", nullable: false),
                ArgumentsJson = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: false),
                ResultJson = table.Column<string>(type: "character varying(65536)", maxLength: 65536, nullable: true),
                ErrorMessage = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                DurationMs = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ToolCalls", x => x.Id);
                table.ForeignKey(
                    name: "FK_ToolCalls_McpTools_McpToolId",
                    column: x => x.McpToolId,
                    principalTable: "McpTools",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_McpServers_Name",
            table: "McpServers",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_McpTools_Embedding",
            table: "McpTools",
            column: "Embedding")
            .Annotation("Npgsql:IndexMethod", "hnsw")
            .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

        migrationBuilder.CreateIndex(
            name: "IX_McpTools_McpServerId_Name",
            table: "McpTools",
            columns: new[] { "McpServerId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ToolCalls_CreatedAt",
            table: "ToolCalls",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_ToolCalls_McpToolId",
            table: "ToolCalls",
            column: "McpToolId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ToolCalls");
        migrationBuilder.DropTable(name: "McpTools");
        migrationBuilder.DropTable(name: "McpServers");
    }
}
