using System.Threading.RateLimiting;
using System.Text.Json;
using HealthChecks.UI.Client;
using Hodor.Application.Mcp;
using Hodor.Core;
using Hodor.Core.ProcessManager;
using Hodor.Core.Webhooks;
using Hodor.Infrastructure.Core;
using Hodor.Infrastructure.Core.Extensions;
using Hodor.Infrastructure.ProcessManager;
using Hodor.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

if (File.Exists($".env.{builder.Environment.EnvironmentName}"))
    DotNetEnv.Env.Load($".env.{builder.Environment.EnvironmentName}");
else
    DotNetEnv.Env.Load();

builder.Services
    .AddSerilog(builder.Configuration)
    .AddCoreInfrastructure()
    .AddPersistence(builder.Configuration)
    .AddProcessManager(builder.Configuration)
    .AddMcpGateway()
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("PostgreSQL") ?? "Host=localhost;Port=5432;Database=hodor;Username=postgres;Password=postgres",
        name: "postgresql",
        tags: ["db", "ready"]);

builder.Services.AddHttpClient("webhook");
builder.Services.AddSingleton<Hodor.Host.Webhooks.WebhookService>();
builder.Services.AddSingleton<Hodor.Core.Webhooks.IWebhookDispatcher>(sp => sp.GetRequiredService<Hodor.Host.Webhooks.WebhookService>());

// Rate limiting (best practice: protect from abuse, ensure fair usage)
var rateLimitPermit = builder.Configuration.GetValue("RateLimitPermitPerMinute", 100);
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var path = ctx.Request.Path.Value ?? "";
        if (path is "/health" or "/ready" or "/metrics" || path.StartsWith("/health/"))
            return RateLimitPartition.GetNoLimiter("health");
        return RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitPermit,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            });
    });
    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.StatusCode = 429;
        await ctx.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Retry later." });
    };
});

var app = builder.Build();

// Correlation ID for audit trail (best practice: request tracing)
app.Use(async (ctx, next) =>
{
    var correlationId = ctx.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? ctx.Request.Headers["Request-Id"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N")[..16];
    ctx.Response.Headers["X-Correlation-ID"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        await next();
});

app.UseRateLimiter();

// API key auth (optional - excluded: /health, /ready, /, /metrics)
var apiKey = builder.Configuration["HodorApiKey"];
app.Use(async (ctx, next) =>
{
    if (string.IsNullOrEmpty(apiKey)) { await next(); return; }
    var path = ctx.Request.Path.Value ?? "";
    if (path is "/" or "/health" or "/ready" or "/metrics" || path.StartsWith("/health/")) { await next(); return; }
    if (ctx.Request.Headers.Authorization is var auth && auth.Count > 0 && auth[0]?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
    {
        var token = auth[0]["Bearer ".Length..].Trim();
        if (token == apiKey) { await next(); return; }
    }
    ctx.Response.StatusCode = 401;
    await ctx.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
});

// Auto-migrate on startup (graceful: continues if PostgreSQL unavailable)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Hodor.Migrations");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Hodor.Persistence.HodorDbContext>();
        logger.LogInformation("Running database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration skipped (PostgreSQL may be unavailable). Gateway runs without persistence.");
    }
}

// Health
app.MapGet("/health", () => Results.Json(new { status = "healthy" }));
app.MapGet("/ready", async (IHodorGateway gateway, CancellationToken cancellationToken) =>
{
    try
    {
        var tools = await gateway.ListToolsAsync(cancellationToken);
        return Results.Json(new { ready = true, tools_count = tools.Count });
    }
    catch (Exception ex)
    {
        return Results.Json(new { ready = false, error = ex.Message }, statusCode: 500);
    }
});

// MCP REST - tools/list (with pagination: cursor, pageSize)
app.MapGet("/api/tools", async (IHodorGateway gateway, string? cursor, int? pageSize, CancellationToken cancellationToken) =>
{
    var tools = (await gateway.ListToolsAsync(cancellationToken)).ToList();
    var (offset, size) = PaginationHelper.Parse(cursor, pageSize);
    var page = PaginationHelper.Apply(tools, offset, size, out var nextCursor);
    return Results.Json(nextCursor != null ? new { tools = page, nextCursor } : new { tools = page });
});

// Combined tools from all servers (with pagination)
app.MapGet("/api/tools/combined", async (IMcpProcessManager pm, string? cursor, int? pageSize, CancellationToken ct) =>
{
    var tools = (await pm.ListToolsAsync(ct)).ToList();
    var (offset, size) = PaginationHelper.Parse(cursor, pageSize);
    var page = PaginationHelper.Apply(tools, offset, size, out var nextCursor);
    return Results.Json(nextCursor != null
        ? new { tools_count = page.Count, tools = page, nextCursor }
        : new { tools_count = page.Count, tools = page });
});

// Server status
app.MapGet("/api/tools/status", async (IMcpProcessManager pm, CancellationToken ct) =>
{
    var servers = await pm.GetServerStatusAsync(ct);
    return Results.Json(new { servers = servers });
});

// Process servers list
app.MapGet("/process/servers", async (IMcpProcessManager pm, CancellationToken ct) =>
{
    var servers = await pm.GetServerStatusAsync(ct);
    return Results.Json(new { servers = servers });
});

// Adapters API (mcp-gateway compatible)
app.MapGet("/adapters", async (IMcpProcessManager pm, IAdapterConfigStore store, CancellationToken ct) =>
{
    var configs = store.GetAll();
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
app.MapPost("/adapters", async (IAdapterConfigStore store, AdapterCreateRequest request, CancellationToken ct) =>
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
    await pm.StopServerAsync(name, ct);
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

// Tools API (mcp-gateway compatible, with pagination)
app.MapGet("/tools", async (IMcpProcessManager pm, string? cursor, int? pageSize, CancellationToken ct) =>
{
    var tools = (await pm.ListToolsAsync(ct)).Select(t => new { name = t.FullName, serverName = t.ServerName, toolName = t.ToolName, description = t.Description, inputSchema = t.InputSchema }).ToList();
    var (offset, size) = PaginationHelper.Parse(cursor, pageSize);
    var page = PaginationHelper.Apply(tools, offset, size, out var nextCursor);
    return Results.Json(nextCursor != null ? new { tools = page, nextCursor } : new { tools = page });
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

// MCP streamable (mcp-gateway compatible)
app.MapGet("/adapters/{name}/mcp", (HttpContext ctx, string name) =>
    Results.Redirect($"{ctx.Request.Scheme}://{ctx.Request.Host}/sse"));
app.MapGet("/mcp", (HttpContext ctx) =>
    Results.Redirect($"{ctx.Request.Scheme}://{ctx.Request.Host}/sse"));
// POST /mcp - MCP 2025 streamable HTTP (single request/response)
app.MapPost("/mcp", async (HttpContext ctx, IHodorGateway gateway, IWebhookDispatcher webhooks) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync(ctx.RequestAborted);
    JsonElement msg;
    try { msg = JsonSerializer.Deserialize<JsonElement>(body); }
    catch { return Results.BadRequest(new { error = "Invalid JSON" }); }
    var id = msg.TryGetProperty("id", out var idEl) ? idEl.GetRawText() : null;
    var method = msg.TryGetProperty("method", out var m) ? m.GetString() : "";
    var @params = msg.TryGetProperty("params", out var p) ? p : (JsonElement?)null;
    if (string.IsNullOrEmpty(method)) return Results.BadRequest(new { error = "method required" });
    object? result = null;
    object? error = null;
    try
    {
        if (method == "initialize")
            result = new { protocolVersion = "2024-11-05", capabilities = new { tools = new { listChanged = false }, prompts = new { } }, serverInfo = new { name = "hodor", version = "1.0.0" } };
        else if (method == "tools/list")
        {
            var tools = (await gateway.ListToolsAsync(ctx.RequestAborted)).Select(t => new { name = t.Name, description = t.Description, inputSchema = t.InputSchema }).ToList();
            var cursor = @params?.TryGetProperty("cursor", out var c) == true ? c.GetString() : null;
            var pageSize = @params?.TryGetProperty("pageSize", out var ps) == true ? ps.GetInt32() : (int?)null;
            var (offset, size) = PaginationHelper.Parse(cursor, pageSize);
            var page = PaginationHelper.Apply(tools, offset, size, out var nextCursor);
            result = nextCursor != null ? new { tools = page, nextCursor } : new { tools = page };
        }
        else if (method == "tools/call")
        {
            var name = @params?.TryGetProperty("name", out var n) == true ? n.GetString() ?? "" : "";
            var args = @params?.TryGetProperty("arguments", out var a) == true ? JsonSerializer.Deserialize<object>(a.GetRawText()) : null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var callResult = await gateway.CallToolAsync(name, args, ctx.RequestAborted);
            sw.Stop();
            result = new { content = new[] { new { type = "text", text = JsonSerializer.Serialize(callResult ?? new { }) } } };
            _ = webhooks.DispatchAsync(new WebhookEvent(
                Id: Guid.NewGuid().ToString("N")[..16],
                Type: WebhookEventTypes.ToolCall,
                Timestamp: DateTime.UtcNow,
                Data: new { tool = name, arguments = args, result = callResult, durationMs = sw.ElapsedMilliseconds }
            ), ctx.RequestAborted);
        }
        else if (method is "prompts/list" or "prompts/get" or "resources/list" or "resources/read")
        {
            var pm = ctx.RequestServices.GetRequiredService<IMcpProcessManager>();
            var fwdParams = @params is { } fp ? JsonSerializer.Deserialize<object>(fp.GetRawText()) : null;
            result = await pm.ForwardMcpRequestAsync(method, fwdParams, ctx.RequestAborted);
        }
        else
            error = new { code = -32601, message = $"Method not found: {method}" };
    }
    catch (Exception ex)
    {
        error = new { code = -32603, message = ex.Message };
    }
    var response = error != null
        ? new { jsonrpc = "2.0", id = id != null ? JsonSerializer.Deserialize<object>(id) : null, error }
        : new { jsonrpc = "2.0", id = id != null ? JsonSerializer.Deserialize<object>(id) : null, result };
    return Results.Json(response);
});

// Prometheus metrics
app.MapMetrics();

// SSE (Claude/Cursor) - MCP transport. MCP-Session-Id header supported for session resume.
app.MapGet("/sse", async (HttpContext ctx) =>
{
    ctx.Response.Headers["Content-Type"] = "text/event-stream";
    ctx.Response.Headers["Cache-Control"] = "no-cache";
    ctx.Response.Headers["Connection"] = "keep-alive";
    var sessionId = SseSessionStore.CreateSession(ctx.Response);
    ctx.Response.Headers["MCP-Session-Id"] = sessionId;
    ctx.Response.Headers["MCP-Protocol-Version"] = "2024-11-05";
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    await ctx.Response.WriteAsync($"event: endpoint\ndata: {{\"url\":\"{baseUrl}/messages?session={sessionId}\"}}\n\n");
    await ctx.Response.Body.FlushAsync();
    try { await ctx.RequestAborted; } finally { SseSessionStore.Remove(sessionId); }
});

app.MapPost("/messages", async (HttpContext ctx, IHodorGateway gateway, IWebhookDispatcher webhooks) =>
{
    var sessionId = ctx.Request.Query["session"].ToString();
    if (string.IsNullOrEmpty(sessionId) || !SseSessionStore.TryGet(sessionId, out _))
        return Results.BadRequest(new { error = "Invalid or missing session" });

    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync(ctx.RequestAborted);
    JsonElement msg;
    try { msg = JsonSerializer.Deserialize<JsonElement>(body); }
    catch { return Results.BadRequest(new { error = "Invalid JSON" }); }

    var id = msg.TryGetProperty("id", out var idEl) ? idEl.GetRawText() : null;
    var method = msg.TryGetProperty("method", out var m) ? m.GetString() : "";
    var @params = msg.TryGetProperty("params", out var p) ? p : (JsonElement?)null;

    if (method == "notifications/initialized") return Results.Accepted();

    object? result = null;
    object? error = null;
    try
    {
        if (method == "initialize")
            result = new { protocolVersion = "2024-11-05", capabilities = new { tools = new { listChanged = false }, prompts = new { } }, serverInfo = new { name = "hodor", version = "1.0.0" } };
        else if (method == "tools/list")
        {
            var tools = (await gateway.ListToolsAsync(ctx.RequestAborted)).Select(t => new { name = t.Name, description = t.Description, inputSchema = t.InputSchema }).ToList();
            var cursor = @params?.TryGetProperty("cursor", out var c) == true ? c.GetString() : null;
            var pageSize = @params?.TryGetProperty("pageSize", out var ps) == true ? ps.GetInt32() : (int?)null;
            var (offset, size) = PaginationHelper.Parse(cursor, pageSize);
            var page = PaginationHelper.Apply(tools, offset, size, out var nextCursor);
            result = nextCursor != null ? new { tools = page, nextCursor } : new { tools = page };
        }
        else if (method == "tools/call")
        {
            var name = @params?.TryGetProperty("name", out var n) == true ? n.GetString() ?? "" : "";
            var args = @params?.TryGetProperty("arguments", out var a) == true ? JsonSerializer.Deserialize<object>(a.GetRawText()) : null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var callResult = await gateway.CallToolAsync(name, args, ctx.RequestAborted);
            sw.Stop();
            result = new { content = new[] { new { type = "text", text = JsonSerializer.Serialize(callResult ?? new { }) } } };
            _ = webhooks.DispatchAsync(new WebhookEvent(
                Id: Guid.NewGuid().ToString("N")[..16],
                Type: WebhookEventTypes.ToolCall,
                Timestamp: DateTime.UtcNow,
                Data: new { tool = name, arguments = args, result = callResult, durationMs = sw.ElapsedMilliseconds }
            ), ctx.RequestAborted);
        }
        else if (method is "prompts/list" or "prompts/get" or "resources/list" or "resources/read")
        {
            var pm = ctx.RequestServices.GetRequiredService<IMcpProcessManager>();
            var fwdParams = @params is { } p ? JsonSerializer.Deserialize<object>(p.GetRawText()) : null;
            result = await pm.ForwardMcpRequestAsync(method, fwdParams, ctx.RequestAborted);
        }
    }
    catch (Exception ex)
    {
        error = new { code = -32603, message = ex.Message };
    }

    var response = error != null
        ? new { jsonrpc = "2.0", id = id != null ? JsonSerializer.Deserialize<object>(id) : null, error }
        : new { jsonrpc = "2.0", id = id != null ? JsonSerializer.Deserialize<object>(id) : null, result };
    await SseSessionStore.SendMessageAsync(sessionId, response, ctx.RequestAborted);
    return Results.Accepted();
});

app.MapHealthChecks("/health/info", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Webhooks - event-based delivery to registered URLs
app.MapPost("/webhooks", (Hodor.Host.Webhooks.WebhookService ws, WebhookRegisterRequest? req) =>
{
    if (req == null || string.IsNullOrWhiteSpace(req.Url))
        return Results.BadRequest(new { error = "url required", example = new { url = "https://example.com/hooks/hodor", events = new[] { "tool.call" }, secret = "optional" } });
    try
    {
        var sub = ws.Register(req.Url, req.Events, req.Secret, req.Description);
        return Results.Created($"/webhooks/{sub.Id}", sub);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});
app.MapGet("/webhooks", (Hodor.Host.Webhooks.WebhookService ws) => Results.Json(ws.List()));
app.MapDelete("/webhooks/{id}", (Hodor.Host.Webhooks.WebhookService ws, string id) =>
    ws.Unregister(id) ? Results.NoContent() : Results.NotFound());
app.MapGet("/webhooks/events", () => Results.Json(new { events = WebhookEventTypes.All }));

// Integration config (ready-to-paste for Claude/Cursor)
app.MapGet("/config/claude", (HttpContext ctx) =>
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Text($"# Add to Claude Code:\nclaude mcp add --scope user --transport sse hodor {baseUrl}/sse\n\n# Or Cursor: Settings > MCP > Add server\n# URL: {baseUrl}/sse", "text/plain");
});
app.MapGet("/config/cursor", (HttpContext ctx) =>
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    return Results.Json(new
    {
        mcpServers = new
        {
            hodor = new
            {
                url = $"{baseUrl}/sse"
            }
        }
    });
});

record WebhookRegisterRequest(string Url, string[]? Events, string? Secret, string? Description);
record AdapterCreateRequest(string Name, string Command, string[]? Args, bool Enabled = true, string? Mode = "cold");

await app.RunAsync();
