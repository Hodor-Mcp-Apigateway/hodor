using System.Threading.Tasks;
using Hodor.Core.ProcessManager;
using Hodor.Infrastructure.ProcessManager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hodor.Infrastructure.ProcessManager.Tests;

public class McpProcessManagerTests
{
    private readonly Mock<ILogger<McpProcessManager>> _loggerMock = new();

    private McpProcessManager CreateManager(McpConfigRoot config = null!)
    {
        config ??= new McpConfigRoot();
        return new McpProcessManager(_loggerMock.Object, config);
    }

    [Fact]
    public async Task GetServerStatusAsync_EmptyConfig_ReturnsEmpty()
    {
        var manager = CreateManager();

        var status = await manager.GetServerStatusAsync();

        status.Should().BeEmpty();
    }

    [Fact]
    public async Task GetServerStatusAsync_WithServers_ReturnsStatus()
    {
        var config = new McpConfigRoot
        {
            McpServers =
            {
                ["mem"] = new McpServerConfig { Command = "echo", Mode = "cold", Enabled = true },
                ["time"] = new McpServerConfig { Command = "echo", Mode = "hot", Enabled = false }
            }
        };
        var manager = CreateManager(config);

        var status = await manager.GetServerStatusAsync();

        status.Should().HaveCount(2);
        status.Should().Contain(s => s.Name == "mem" && s.Mode == "cold" && s.IsEnabled);
        status.Should().Contain(s => s.Name == "time" && s.Mode == "hot" && !s.IsEnabled);
    }

    [Fact]
    public void GetAllServerConfigs_ReturnsConfig()
    {
        var config = new McpConfigRoot
        {
            McpServers = { ["x"] = new McpServerConfig { Command = "cmd" } }
        };
        var manager = CreateManager(config);

        var all = manager.GetAllServerConfigs();

        all.Should().ContainKey("x");
        all["x"].Command.Should().Be("cmd");
    }

    [Fact]
    public async Task EnsureServerRunningAsync_UnknownServer_Throws()
    {
        var manager = CreateManager();

        var act = () => manager.EnsureServerRunningAsync("unknown");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task EnsureServerRunningAsync_DisabledServer_WithoutForceEnable_Throws()
    {
        var config = new McpConfigRoot
        {
            McpServers =
            {
                ["disabled"] = new McpServerConfig { Command = "echo", Enabled = false }
            }
        };
        var manager = CreateManager(config);

        var act = () => manager.EnsureServerRunningAsync("disabled", forceEnable: false);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*disabled*");
    }

    [Fact]
    public async Task ListToolsAsync_EmptyConfig_ReturnsEmpty()
    {
        var manager = CreateManager();

        var tools = await manager.ListToolsAsync();

        tools.Should().BeEmpty();
    }

    [Fact]
    public async Task ListToolsAsync_WithServerName_OnlyQueriesThatServer()
    {
        var config = new McpConfigRoot
        {
            McpServers =
            {
                ["mem"] = new McpServerConfig { Command = "nonexistent-cmd-xyz", Enabled = true }
            }
        };
        var manager = CreateManager(config);

        var tools = await manager.ListToolsAsync("mem");

        tools.Should().BeEmpty();
    }
}
