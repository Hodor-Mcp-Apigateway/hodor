using System.Text.Json;
using Hodor.Application.Mcp;
using Hodor.Core;
using Hodor.Core.Mcp;
using Hodor.Core.ProcessManager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hodor.Application.Mcp.Tests;

public class HodorGatewayServiceTests
{
    private readonly Mock<ILogger<HodorGatewayService>> _loggerMock = new();
    private readonly Mock<IMcpProcessManager> _processManagerMock = new();

    private HodorGatewayService CreateService(bool dynamicMcp = true)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DynamicMcp"] = dynamicMcp.ToString().ToLowerInvariant() })
            .Build();
        return new HodorGatewayService(_loggerMock.Object, _processManagerMock.Object, config);
    }

    [Fact]
    public async Task ListToolsAsync_DynamicMcp_ReturnsMetaTools()
    {
        var service = CreateService(dynamicMcp: true);
        var tools = await service.ListToolsAsync(CancellationToken.None);

        tools.Should().HaveCount(3);
        tools.Select(t => t.Name).Should().Contain("hodor-find");
        tools.Select(t => t.Name).Should().Contain("hodor-exec");
        tools.Select(t => t.Name).Should().Contain("hodor-schema");
        _processManagerMock.Verify(p => p.ListToolsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListToolsAsync_NonDynamicMcp_DelegatesToProcessManager()
    {
        var expected = new List<McpToolInfo>
        {
            new("mem", "get", "mem:get", "Get memory", null)
        };
        _processManagerMock.Setup(p => p.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var service = CreateService(dynamicMcp: false);
        var tools = await service.ListToolsAsync(CancellationToken.None);

        tools.Should().HaveCount(1);
        tools[0].Name.Should().Be("mem:get");
        tools[0].Description.Should().Be("Get memory");
    }

    [Fact]
    public async Task CallToolAsync_HodorFind_ReturnsFilteredTools()
    {
        var allTools = new List<McpToolInfo>
        {
            new("mem", "get", "mem:get", "Get memory value", null),
            new("time", "now", "time:now", "Current time", null)
        };
        _processManagerMock.Setup(p => p.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTools);
        _processManagerMock.Setup(p => p.GetAllServerConfigs())
            .Returns(new Dictionary<string, McpServerConfig>());

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { query = "mem" });
        var result = await service.CallToolAsync("hodor-find", args, CancellationToken.None);

        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("mem:get");
        json.Should().NotContain("time:now");
    }

    [Fact]
    public async Task CallToolAsync_HodorFind_EmptyQuery_ReturnsAllTools()
    {
        var allTools = new List<McpToolInfo>
        {
            new("mem", "get", "mem:get", "Get memory", null)
        };
        _processManagerMock.Setup(p => p.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTools);
        _processManagerMock.Setup(p => p.GetAllServerConfigs())
            .Returns(new Dictionary<string, McpServerConfig>());

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { query = "" });
        var result = await service.CallToolAsync("hodor-find", args, CancellationToken.None);

        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("mem:get");
    }

    [Fact]
    public async Task CallToolAsync_HodorFind_IncludesDisabledServers()
    {
        var allTools = new List<McpToolInfo>();
        _processManagerMock.Setup(p => p.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTools);
        _processManagerMock.Setup(p => p.GetAllServerConfigs())
            .Returns(new Dictionary<string, McpServerConfig>
            {
                ["disabled-server"] = new McpServerConfig { Command = "echo", Enabled = false }
            });

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { query = "" });
        var result = await service.CallToolAsync("hodor-find", args, CancellationToken.None);

        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("disabled-server");
        json.Should().Contain("disabled");
    }

    [Fact]
    public async Task CallToolAsync_HodorExec_DelegatesToProcessManager()
    {
        _processManagerMock.Setup(p => p.CallToolAsync("mem", "get", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new { value = "test" });

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { tool = "mem:get", arguments = new { } });
        var result = await service.CallToolAsync("hodor-exec", args, CancellationToken.None);

        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("test");
        _processManagerMock.Verify(p => p.CallToolAsync("mem", "get", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CallToolAsync_HodorExec_WithArguments_PassesThrough()
    {
        object? capturedArgs = null;
        _processManagerMock.Setup(p => p.CallToolAsync("srv", "tool", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, object?, CancellationToken>((_, _, a, _) => capturedArgs = a)
            .ReturnsAsync(new { ok = true });

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { tool = "srv:tool", arguments = new { key = "value" } });
        await service.CallToolAsync("hodor-exec", args, CancellationToken.None);

        capturedArgs.Should().NotBeNull();
        JsonSerializer.Serialize(capturedArgs).Should().Contain("key");
    }

    [Fact]
    public async Task CallToolAsync_HodorSchema_ReturnsInputSchema()
    {
        var schema = new { type = "object", properties = new { } };
        var tools = new List<McpToolInfo>
        {
            new("srv", "tool", "srv:tool", "Desc", schema)
        };
        _processManagerMock.Setup(p => p.ListToolsAsync("srv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tools);

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { tool = "srv:tool" });
        var result = await service.CallToolAsync("hodor-schema", args, CancellationToken.None);

        result.Should().NotBeNull();
        JsonSerializer.Serialize(result).Should().Contain("object");
    }

    [Fact]
    public async Task CallToolAsync_HodorSchema_UnknownTool_ReturnsNull()
    {
        _processManagerMock.Setup(p => p.ListToolsAsync("srv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<McpToolInfo>());

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { tool = "srv:unknown" });
        var result = await service.CallToolAsync("hodor-schema", args, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CallToolAsync_UnknownMetaTool_Throws()
    {
        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { });

        var act = () => service.CallToolAsync("hodor-unknown", args, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unknown meta-tool*");
    }

    [Fact]
    public async Task CallToolAsync_NonMetaTool_DelegatesToProcessManager()
    {
        _processManagerMock.Setup(p => p.CallToolAsync("mem", "get", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("result");

        var service = CreateService();
        var result = await service.CallToolAsync("mem:get", null, CancellationToken.None);

        result.Should().Be("result");
        _processManagerMock.Verify(p => p.CallToolAsync("mem", "get", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CallToolAsync_InvalidToolFormat_Throws()
    {
        var service = CreateService();

        var act = () => service.CallToolAsync("mem", null, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*server:tool_name*");
    }

    [Fact]
    public async Task CallToolAsync_HodorExec_InvalidToolFormat_Throws()
    {
        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { tool = "invalid-no-colon" });

        var act = () => service.CallToolAsync("hodor-exec", args, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CallToolAsync_HodorFind_CaseInsensitiveQuery()
    {
        var allTools = new List<McpToolInfo>
        {
            new("mem", "get", "mem:get", "Get MEMORY value", null)
        };
        _processManagerMock.Setup(p => p.ListToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allTools);
        _processManagerMock.Setup(p => p.GetAllServerConfigs())
            .Returns(new Dictionary<string, McpServerConfig>());

        var service = CreateService();
        var args = JsonSerializer.SerializeToElement(new { query = "memory" });
        var result = await service.CallToolAsync("hodor-find", args, CancellationToken.None);

        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("mem:get");
    }
}
