# mcp-gateway Uyumluluk Rehberi

Bu doküman, Microsoft mcp-gateway projesiyle Hodor arasındaki farkları ve eklenmesi gereken endpoint'leri açıklar.

## mcp-gateway vs Hodor Karşılaştırması

| mcp-gateway | Hodor | Durum |
|-------------|-------|-------|
| `POST /adapters` | — | Eklenecek |
| `GET /adapters` | `GET /process/servers` | Alias eklenebilir |
| `GET /adapters/{name}` | — | Eklenecek |
| `GET /adapters/{name}/status` | `GET /api/tools/status` | Alias eklenecek |
| `GET /adapters/{name}/logs` | — | **Mevcut** (GetServerLogs) |
| `PUT /adapters/{name}` | — | Eklenecek |
| `DELETE /adapters/{name}` | — | Eklenecek |
| `POST /tools` | — | Farklı mimari (Hodor meta-tools) |
| `GET /tools` | `GET /api/tools/combined` | Alias |
| `GET /tools/{name}` | — | Eklenecek |
| `GET /tools/{name}/status` | — | Server status |
| `GET /tools/{name}/logs` | — | Server logs |
| `POST /adapters/{name}/mcp` | `GET /sse` | SSE farklı |
| `POST /mcp` | `GET /sse` | SSE farklı |

## Hodor'da Zaten Mevcut

- `StopServerAsync` / `GetServerLogs` — ProcessManager'da
- `IAdapterConfigStore` — McpServerConfig.cs'de
- Log buffer — ManagedProcess'te

## Eklenmesi Gerekenler

### 1. AdapterConfigStore Implementasyonu

`src/Hodor.Infrastructure.ProcessManager/AdapterConfigStore.cs`:

```csharp
using System.Text.Json;
using Hodor.Core.ProcessManager;

namespace Hodor.Infrastructure.ProcessManager;

public sealed class AdapterConfigStore : IAdapterConfigStore
{
    private readonly McpConfigRoot _config;
    private readonly string _configPath;

    public AdapterConfigStore(McpConfigRoot config, string configPath)
    {
        _config = config;
        _configPath = configPath;
    }

    public IReadOnlyDictionary<string, McpServerConfig> GetAll() =>
        (IReadOnlyDictionary<string, McpServerConfig>)_config.McpServers;

    public McpServerConfig? Get(string name) =>
        _config.McpServers.TryGetValue(name, out var c) ? c : null;

    public void Add(string name, McpServerConfig config) => _config.McpServers[name] = config;
    public void Update(string name, McpServerConfig config) => _config.McpServers[name] = config;
    public bool Remove(string name) => _config.McpServers.Remove(name);

    public async Task PersistAsync(CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_configPath, json, ct);
    }
}
```

### 2. DependencyInjection Güncellemesi

`AddProcessManager` içinde:
```csharp
var configPath = configuration["McpConfigPath"] ?? "mcp-config.json";
// ... config load ...
services.AddSingleton<IAdapterConfigStore>(sp =>
    new AdapterConfigStore(config, Path.GetFullPath(configPath)));
```

### 3. Program.cs'e Eklenecek Endpoint'ler

```csharp
// === Adapters API (mcp-gateway uyumlu) ===
app.MapGet("/adapters", async (IMcpProcessManager pm, IAdapterConfigStore store, CancellationToken ct) =>
{
    var configs = store.GetAll();
    var servers = await pm.GetServerStatusAsync(ct);
    var list = configs.Select(kv => new { name = kv.Key, command = kv.Value.Command, args = kv.Value.Args, enabled = kv.Value.Enabled, mode = kv.Value.Mode });
    return Results.Json(list);
});
app.MapGet("/adapters/{name}", (IAdapterConfigStore store, string name) =>
{
    var c = store.Get(name);
    return c != null ? Results.Json(new { name, command = c.Command, args = c.Args, enabled = c.Enabled, mode = c.Mode }) : Results.NotFound();
});
app.MapGet("/adapters/{name}/status", async (IMcpProcessManager pm, string name, CancellationToken ct) =>
{
    var servers = await pm.GetServerStatusAsync(ct);
    var s = servers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    return s != null ? Results.Json(new { status = s.Status, toolCount = s.ToolCount, mode = s.Mode, uptime = s.Uptime }) : Results.NotFound();
});
app.MapGet("/adapters/{name}/logs", (IMcpProcessManager pm, string name, int instance = 0) =>
{
    var logs = pm.GetServerLogs(name, 100);
    return Results.Text(string.Join("\n", logs), "text/plain");
});
app.MapPost("/adapters", async (IAdapterConfigStore store, IMcpProcessManager pm, AdapterCreateRequest request, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Command)) return Results.BadRequest();
    store.Add(request.Name, new McpServerConfig { Command = request.Command, Args = request.Args ?? [], Enabled = request.Enabled, Mode = request.Mode ?? "cold" });
    await store.PersistAsync(ct);
    return Results.Created($"/adapters/{request.Name}", store.Get(request.Name));
});
app.MapPut("/adapters/{name}", async (IAdapterConfigStore store, IMcpProcessManager pm, string name, AdapterCreateRequest request, CancellationToken ct) =>
{
    if (store.Get(name) == null) return Results.NotFound();
    store.Update(name, new McpServerConfig { Command = request.Command, Args = request.Args ?? [], Enabled = request.Enabled, Mode = request.Mode ?? "cold" });
    await store.PersistAsync(ct);
    await pm.StopServerAsync(name, ct); // restart on next use
    return Results.Json(store.Get(name));
});
app.MapDelete("/adapters/{name}", async (IAdapterConfigStore store, IMcpProcessManager pm, string name, CancellationToken ct) =>
{
    if (store.Get(name) == null) return Results.NotFound();
    await pm.StopServerAsync(name, ct);
    store.Remove(name);
    await store.PersistAsync(ct);
    return Results.NoContent();
});

// === Tools API (mcp-gateway uyumlu) ===
app.MapGet("/tools", async (IMcpProcessManager pm, CancellationToken ct) =>
{
    var tools = await pm.ListToolsAsync(ct);
    return Results.Json(tools.Select(t => new { name = t.FullName, serverName = t.ServerName, toolName = t.ToolName, description = t.Description, inputSchema = t.InputSchema }));
});
app.MapGet("/tools/{name}", async (IMcpProcessManager pm, string name, CancellationToken ct) =>
{
    var tools = await pm.ListToolsAsync(ct);
    var t = tools.FirstOrDefault(x => string.Equals(x.FullName, name, StringComparison.OrdinalIgnoreCase));
    return t != null ? Results.Json(new { name = t.FullName, serverName = t.ServerName, toolName = t.ToolName, description = t.Description, inputSchema = t.InputSchema }) : Results.NotFound();
});
app.MapGet("/tools/{name}/status", async (IMcpProcessManager pm, string name, CancellationToken ct) =>
{
    var parts = name.Split(':', 2);
    var serverName = parts.Length == 2 ? parts[0] : name;
    var servers = await pm.GetServerStatusAsync(ct);
    var s = servers.FirstOrDefault(x => string.Equals(x.Name, serverName, StringComparison.OrdinalIgnoreCase));
    return s != null ? Results.Json(new { status = s.Status, toolCount = s.ToolCount }) : Results.NotFound();
});
app.MapGet("/tools/{name}/logs", (IMcpProcessManager pm, string name) =>
{
    var parts = name.Split(':', 2);
    var serverName = parts.Length == 2 ? parts[0] : name;
    var logs = pm.GetServerLogs(serverName, 100);
    return Results.Text(string.Join("\n", logs), "text/plain");
});

// === MCP streamable HTTP (mcp-gateway uyumlu) ===
app.MapGet("/adapters/{name}/mcp", (HttpContext ctx, string name) =>
    Results.Redirect($"{ctx.Request.Scheme}://{ctx.Request.Host}/sse"));
app.MapGet("/mcp", (HttpContext ctx) =>
    Results.Redirect($"{ctx.Request.Scheme}://{ctx.Request.Host}/sse"));

record AdapterCreateRequest(string Name, string Command, string[]? Args, bool Enabled = true, string? Mode = "cold");
```

### 4. Hodor.Host.csproj

`IAdapterConfigStore` Core'da tanımlı; Host projesi zaten Core ve ProcessManager referanslarına sahip.

### 5. DependencyInjection

`Hodor.Infrastructure.ProcessManager` projesine `Hodor.Core` referansı var. `AdapterConfigStore` için `IAdapterConfigStore` kullanılacak.

---

## Özet

- **StopServerAsync**, **GetServerLogs**, log buffer zaten mevcut.
- **AdapterConfigStore** implementasyonu ve `IAdapterConfigStore` DI kaydı eklenmeli.
- Yukarıdaki endpoint'ler eklendiğinde mcp-gateway ile uyumlu bir API sağlanır.
