using System.Text.Json;
using FluentAssertions;
using Hodor.Core.ProcessManager;
using Xunit;

namespace Hodor.Infrastructure.ProcessManager.Tests;

public class McpServerConfigTests
{
    [Fact]
    public void McpServerConfig_JsonDeserialization()
    {
        var json = """
            {
                "mcpServers": {
                    "mem": {
                        "command": "npx",
                        "args": ["-y", "@modelcontextprotocol/server-memory"],
                        "enabled": true,
                        "mode": "cold",
                        "idle_timeout": 120
                    }
                }
            }
            """;

        var config = JsonSerializer.Deserialize<McpConfigRoot>(json);

        config.Should().NotBeNull();
        config!.McpServers.Should().ContainKey("mem");
        var mem = config.McpServers["mem"];
        mem.Command.Should().Be("npx");
        mem.Args.Should().Equal("-y", "@modelcontextprotocol/server-memory");
        mem.Enabled.Should().BeTrue();
        mem.Mode.Should().Be("cold");
        mem.IdleTimeoutSeconds.Should().Be(120);
    }

    [Fact]
    public void McpServerConfig_DefaultValues()
    {
        var config = new McpServerConfig { Command = "echo" };

        config.Args.Should().BeEmpty();
        config.Enabled.Should().BeTrue();
        config.Mode.Should().Be("cold");
        config.IdleTimeoutSeconds.Should().Be(120);
    }
}
