using System.Text.Json;
using Hodor.Core.ProcessManager;
using Hodor.Core.Webhooks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hodor.Infrastructure.ProcessManager;

public static class DependencyInjection
{
    public static IServiceCollection AddProcessManager(this IServiceCollection services, IConfiguration configuration)
    {
        var configPath = configuration["McpConfigPath"] ?? "mcp-config.json";
        McpConfigRoot config;
        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            config = JsonSerializer.Deserialize<McpConfigRoot>(json) ?? new McpConfigRoot();
        }
        else
        {
            config = GetBuiltInDefaultConfig();
        }

        var timeout = configuration.GetValue("ToolCallTimeout", 90);
        var stdoutBuffer = configuration.GetValue("StdoutBufferSize", 1024 * 1024); // 1MB default
        var maxRetries = configuration.GetValue("ToolCallMaxRetries", 2);

        services.AddSingleton(config);
        services.AddSingleton<IAdapterConfigStore>(new AdapterConfigStore(config, Path.GetFullPath(configPath)));
        services.AddSingleton<IMcpProcessManager>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<McpProcessManager>>();
            var webhooks = sp.GetService<IWebhookDispatcher>();
            return new McpProcessManager(logger, config, timeout, stdoutBuffer, maxRetries, webhooks);
        });

        return services;
    }

    private static McpConfigRoot GetBuiltInDefaultConfig()
    {
        return new McpConfigRoot
        {
            McpServers = new Dictionary<string, McpServerConfig>
            {
                ["memory"] = new() { Command = "npx", Args = ["-y", "@modelcontextprotocol/server-memory"], Enabled = true, Mode = "hot" },
                ["fetch"] = new() { Command = "uvx", Args = ["mcp-server-fetch"], Enabled = true, Mode = "cold" },
                ["time"] = new() { Command = "npx", Args = ["-y", "@modelcontextprotocol/server-time"], Enabled = true, Mode = "cold" },
                ["filesystem"] = new() { Command = "npx", Args = ["-y", "@modelcontextprotocol/server-filesystem", "/tmp"], Enabled = false, Mode = "cold" },
                ["github"] = new() { Command = "npx", Args = ["-y", "@modelcontextprotocol/server-github"], Enabled = false, Mode = "cold" }
            }
        };
    }
}
