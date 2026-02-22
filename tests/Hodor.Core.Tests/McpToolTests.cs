using FluentAssertions;
using Hodor.Core.Mcp;
using Xunit;

namespace Hodor.Core.Tests;

public class McpToolTests
{
    [Fact]
    public void McpTool_Record_StoresValues()
    {
        var tool = new McpTool("name", "description", new { type = "object" });

        tool.Name.Should().Be("name");
        tool.Description.Should().Be("description");
        tool.InputSchema.Should().NotBeNull();
    }

    [Fact]
    public void McpTool_WithNullDescription()
    {
        var tool = new McpTool("x", null, null);

        tool.Name.Should().Be("x");
        tool.Description.Should().BeNull();
        tool.InputSchema.Should().BeNull();
    }
}
