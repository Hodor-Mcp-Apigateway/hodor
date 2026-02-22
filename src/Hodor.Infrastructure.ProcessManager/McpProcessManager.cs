using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Hodor.Core.ProcessManager;
using Hodor.Core.Webhooks;
using Microsoft.Extensions.Logging;

namespace Hodor.Infrastructure.ProcessManager;

/// <summary>
/// Manages MCP server processes - spawn, stdio JSON-RPC, HOT/COLD modes.
/// Best practice: retry with backoff, circuit breaker for failed servers.
/// </summary>
public sealed class McpProcessManager : IMcpProcessManager, IDisposable
{
    private readonly ILogger<McpProcessManager> _logger;
    private readonly McpConfigRoot _config;
    private readonly int _toolCallTimeoutSeconds;
    private readonly int _stdoutBufferSize;
    private readonly int _maxRetries;
    private readonly IWebhookDispatcher? _webhooks;
    private readonly Dictionary<string, ManagedProcess> _processes = new();
    private readonly Dictionary<string, (int Failures, DateTime CooldownUntil)> _circuitBreaker = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private const int CircuitBreakerThreshold = 3;
    private static readonly TimeSpan CircuitBreakerCooldown = TimeSpan.FromSeconds(30);

    public McpProcessManager(
        ILogger<McpProcessManager> logger,
        McpConfigRoot config,
        int toolCallTimeoutSeconds = 90,
        int stdoutBufferSize = 1024 * 1024, // 1MB - fixes large MCP responses (#38)
        int maxRetries = 2,
        IWebhookDispatcher? webhooks = null)
    {
        _logger = logger;
        _config = config;
        _toolCallTimeoutSeconds = toolCallTimeoutSeconds;
        _stdoutBufferSize = stdoutBufferSize;
        _maxRetries = maxRetries;
        _webhooks = webhooks;
    }

    public async Task<IReadOnlyList<ServerStatus>> GetServerStatusAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var result = new List<ServerStatus>();
            foreach (var (name, cfg) in _config.McpServers)
            {
                var proc = _processes.GetValueOrDefault(name);
                var status = proc?.IsRunning == true ? "running" : "stopped";
                var toolCount = proc?.ToolCount ?? 0;
                result.Add(new ServerStatus(name, status, toolCount, cfg.Mode, cfg.Enabled, proc?.Uptime));
            }
            return result;
        }
        finally { _lock.Release(); }
    }

    public Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken = default) =>
        ListToolsAsync(string.Empty, cancellationToken);

    public Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(string serverName, CancellationToken cancellationToken = default) =>
        ListToolsInternalAsync(string.IsNullOrEmpty(serverName) ? null : serverName, cancellationToken);

    private async Task<IReadOnlyList<McpToolInfo>> ListToolsInternalAsync(string? serverName, CancellationToken cancellationToken)
    {
        var servers = string.IsNullOrEmpty(serverName)
            ? _config.McpServers.Where(s => s.Value.Enabled).Select(s => s.Key).ToList()
            : new List<string> { serverName };

        var all = new List<McpToolInfo>();
        foreach (var name in servers)
        {
            if (!_config.McpServers.TryGetValue(name, out var cfg) || !cfg.Enabled)
                continue;
            if (IsCircuitOpen(name))
                continue;
            try
            {
                await EnsureServerRunningAsync(name, false, cancellationToken);
                var tools = await GetToolsFromServerAsync(name, cancellationToken);
                all.AddRange(tools);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Server {Server} failed (graceful degradation - continuing)", name);
            }
        }
        return all;
    }

    public async Task<object?> CallToolAsync(string serverName, string toolName, object? arguments, CancellationToken cancellationToken = default)
    {
        await EnsureServerRunningAsync(serverName, forceEnable: true, cancellationToken);
        return await SendRequestWithRetryAsync(serverName, "tools/call", new { name = toolName, arguments = arguments ?? new { } }, cancellationToken);
    }

    public async Task<object?> GetToolSchemaAsync(string serverName, string toolName, CancellationToken cancellationToken = default)
    {
        var tools = await ListToolsInternalAsync(serverName, cancellationToken);
        var t = tools.FirstOrDefault(x => string.Equals(x.ToolName, toolName, StringComparison.OrdinalIgnoreCase));
        return t?.InputSchema;
    }

    public async Task EnsureServerRunningAsync(string serverName, bool forceEnable = false, CancellationToken cancellationToken = default)
    {
        if (!_config.McpServers.TryGetValue(serverName, out var cfg))
            throw new InvalidOperationException($"Server '{serverName}' not found.");
        if (!cfg.Enabled && !forceEnable)
            throw new InvalidOperationException($"Server '{serverName}' is disabled. Call with forceEnable to auto-enable.");
        if (IsCircuitOpen(serverName))
            throw new InvalidOperationException($"Server '{serverName}' is temporarily unavailable (circuit breaker). Retry later.");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_processes.TryGetValue(serverName, out var proc) && proc.IsRunning)
                return;

            _logger.LogInformation("Starting MCP server: {Server}", serverName);
            var p = StartProcess(serverName, cfg);
            _processes[serverName] = p;
            await Task.Delay(500, cancellationToken);
            await InitializeServerAsync(serverName, cancellationToken);
            _ = _webhooks?.DispatchAsync(new WebhookEvent(
                Id: Guid.NewGuid().ToString("N")[..16],
                Type: WebhookEventTypes.ServerStarted,
                Timestamp: DateTime.UtcNow,
                Data: new { server = serverName }
            ), cancellationToken);
        }
        finally { _lock.Release(); }
    }

    private void OnServerStopped(string serverName)
    {
        _lock.Wait();
        try
        {
            _processes.Remove(serverName);
        }
        finally
        {
            _lock.Release();
        }
        _ = _webhooks?.DispatchAsync(new WebhookEvent(
            Id: Guid.NewGuid().ToString("N")[..16],
            Type: WebhookEventTypes.ServerStopped,
            Timestamp: DateTime.UtcNow,
            Data: new { server = serverName }
        ), default);
    }

    private ManagedProcess StartProcess(string name, McpServerConfig cfg)
    {
        var psi = new ProcessStartInfo
        {
            FileName = cfg.Command,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var (k, v) in cfg.Env ?? new Dictionary<string, string>())
            psi.Environment[k] = v;

        foreach (var a in cfg.Args)
            psi.ArgumentList.Add(a);

        var process = Process.Start(psi)!;
        var managed = new ManagedProcess(process, cfg, _logger, name, _stdoutBufferSize, () => OnServerStopped(name));
        _ = managed.ReadStderrAsync();
        return managed;
    }

    private async Task InitializeServerAsync(string serverName, CancellationToken cancellationToken)
    {
        await SendRequestAsync(serverName, "initialize", new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { },
            clientInfo = new { name = "hodor-gateway", version = "1.0.0" }
        }, cancellationToken);
        await SendNotificationAsync(serverName, "notifications/initialized", null, cancellationToken);
    }

    private async Task SendNotificationAsync(string serverName, string method, object? @params, CancellationToken cancellationToken)
    {
        if (!_processes.TryGetValue(serverName, out var proc) || !proc.IsRunning) return;
        var json = (@params != null
            ? JsonSerializer.Serialize(new { jsonrpc = "2.0", method, @params })
            : JsonSerializer.Serialize(new { jsonrpc = "2.0", method })) + "\n";
        await proc.SendNotificationAsync(json, cancellationToken);
    }

    private async Task<List<McpToolInfo>> GetToolsFromServerAsync(string serverName, CancellationToken cancellationToken)
    {
        var result = await SendRequestAsync(serverName, "tools/list", new { }, cancellationToken);
        if (result is not JsonElement je)
            return [];

        var tools = new List<McpToolInfo>();
        if (je.TryGetProperty("tools", out var arr))
        {
            foreach (var t in arr.EnumerateArray())
            {
                var toolName = t.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var desc = t.TryGetProperty("description", out var d) ? d.GetString() : null;
                object? schema = null;
                if (t.TryGetProperty("inputSchema", out var s))
                    schema = JsonSerializer.Deserialize<object>(s.GetRawText());
                tools.Add(new McpToolInfo(serverName, toolName, $"{serverName}:{toolName}", desc, schema));
            }
        }

        if (_processes.TryGetValue(serverName, out var proc))
            proc.ToolCount = tools.Count;

        return tools;
    }

    private async Task<object?> SendRequestWithRetryAsync(string serverName, string method, object? @params, CancellationToken cancellationToken)
    {
        Exception? lastEx = null;
        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var result = await SendRequestAsync(serverName, method, @params, cancellationToken);
                RecordSuccess(serverName);
                return result;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                if (attempt < _maxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt));
                    _logger.LogWarning(ex, "Server {Server} attempt {Attempt} failed, retrying in {Delay}ms", serverName, attempt + 1, delay.TotalMilliseconds);
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    RecordFailure(serverName);
                }
            }
        }
        throw lastEx!;
    }

    private void RecordSuccess(string serverName)
    {
        lock (_circuitBreaker)
            _circuitBreaker.Remove(serverName);
    }

    private void RecordFailure(string serverName)
    {
        lock (_circuitBreaker)
        {
            _circuitBreaker[serverName] = (CircuitBreakerThreshold, DateTime.UtcNow + CircuitBreakerCooldown);
            _logger.LogWarning("Circuit breaker opened for {Server} until {Until}", serverName, DateTime.UtcNow + CircuitBreakerCooldown);
        }
    }

    private bool IsCircuitOpen(string serverName)
    {
        lock (_circuitBreaker)
        {
            if (!_circuitBreaker.TryGetValue(serverName, out var state))
                return false;
            if (state.CooldownUntil == default)
                return false;
            if (DateTime.UtcNow < state.CooldownUntil)
            {
                _logger.LogDebug("Circuit open for {Server} until {Until}", serverName, state.CooldownUntil);
                return true;
            }
            _circuitBreaker.Remove(serverName);
        }
        return false;
    }

    private async Task<object?> SendRequestAsync(string serverName, string method, object? @params, CancellationToken cancellationToken)
    {
        if (IsCircuitOpen(serverName))
            throw new InvalidOperationException($"Server '{serverName}' is temporarily unavailable (circuit breaker open). Retry later.");

        if (!_processes.TryGetValue(serverName, out var proc) || !proc.IsRunning)
            throw new InvalidOperationException($"Server '{serverName}' is not running.");

        var id = Guid.NewGuid().ToString("N")[..8];
        var req = new { jsonrpc = "2.0", id, method, @params };
        var json = JsonSerializer.Serialize(req) + "\n";

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_toolCallTimeoutSeconds));

        return await proc.SendRequestAsync(json, id, cts.Token);
    }

    public IReadOnlyDictionary<string, McpServerConfig> GetAllServerConfigs() =>
        (IReadOnlyDictionary<string, McpServerConfig>)_config.McpServers;

    public async Task<object?> ForwardMcpRequestAsync(string method, object? @params, CancellationToken cancellationToken = default)
    {
        var servers = _config.McpServers.Where(s => s.Value.Enabled).Select(s => s.Key).ToList();
        var isList = method is "prompts/list" or "resources/list";
        if (isList)
        {
            var allItems = new List<object>();
            var key = method == "prompts/list" ? "prompts" : "resources";
            foreach (var name in servers)
            {
                if (IsCircuitOpen(name)) continue;
                try
                {
                    await EnsureServerRunningAsync(name, false, cancellationToken);
                    var result = await SendRequestAsync(name, method, @params, cancellationToken);
                    if (result is JsonElement je && je.TryGetProperty(key, out var arr))
                        AddJsonArrayItems(allItems, arr);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Server {Server} did not support {Method}", name, method);
                }
            }
            return new Dictionary<string, object> { [key] = allItems };
        }
        foreach (var name in servers)
        {
            if (IsCircuitOpen(name)) continue;
            try
            {
                await EnsureServerRunningAsync(name, false, cancellationToken);
                return await SendRequestAsync(name, method, @params, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Server {Server} did not support {Method}", name, method);
            }
        }
        throw new InvalidOperationException($"No backend server supported {method}");
    }

    private static void AddJsonArrayItems(List<object> target, JsonElement arr)
    {
        foreach (var item in arr.EnumerateArray())
            target.Add(JsonSerializer.Deserialize<object>(item.GetRawText()) ?? new { });
    }

    public async Task StopServerAsync(string serverName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_processes.TryGetValue(serverName, out var proc))
            {
                try { proc.Stop(); } catch { /* already exited */ }
                _processes.Remove(serverName);
            }
        }
        finally { _lock.Release(); }
    }

    public IReadOnlyList<string> GetServerLogs(string serverName, int maxLines = 100)
    {
        if (_processes.TryGetValue(serverName, out var proc))
            return proc.GetLogs(maxLines);
        return [];
    }

    public void Dispose() => _lock.Dispose();
}

internal sealed class ManagedProcess
{
    private readonly Process _process;
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly int _bufferSize;
    private readonly Action? _onExited;
    private readonly Dictionary<string, TaskCompletionSource<string>> _pending = new();
    private readonly Lock _sync = new();
    private readonly List<string> _logs = new();
    private const int MaxLogLines = 500;
    private DateTime _startedAt;

    public int ToolCount { get; set; }
    public bool IsRunning => _process is { HasExited: false };
    public TimeSpan? Uptime => IsRunning ? DateTime.UtcNow - _startedAt : null;

    public ManagedProcess(Process process, McpServerConfig config, ILogger logger, string name, int bufferSize = 1024 * 1024, Action? onExited = null)
    {
        _process = process;
        _logger = logger;
        _name = name;
        _bufferSize = bufferSize;
        _onExited = onExited;
        _startedAt = DateTime.UtcNow;
        if (_onExited != null)
        {
            _process.EnableRaisingEvents = true;
            _process.Exited += (_, _) => _onExited();
        }
        _ = ReadStdoutAsync();
    }

    public async Task ReadStderrAsync()
    {
        try
        {
            while (await _process.StandardError.ReadLineAsync() is { } line)
            {
                _logger.LogDebug("[{Server} stderr] {Line}", _name, line);
                AddLog($"[stderr] {line}");
            }
        }
        catch { /* process ended */ }
    }

    public void Stop() { try { _process.Kill(); } catch { } }
    public IReadOnlyList<string> GetLogs(int maxLines = 100)
    {
        lock (_logs)
        {
            var start = Math.Max(0, _logs.Count - maxLines);
            return _logs.Skip(start).Take(maxLines).ToList();
        }
    }
    private void AddLog(string line)
    {
        lock (_logs)
        {
            _logs.Add($"[{DateTime.UtcNow:HH:mm:ss}] {line}");
            if (_logs.Count > MaxLogLines) _logs.RemoveAt(0);
        }
    }

    private async Task ReadStdoutAsync()
    {
        try
        {
            var stream = _process.StandardOutput.BaseStream;
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: _bufferSize);
            while (await reader.ReadLineAsync() is { } line)
            {
                if (line.Length > 0)
                {
                    AddLog(line);
                    HandleLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stdout read ended for {Server}", _name);
        }
    }

    private void HandleLine(string line)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            var id = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            if (string.IsNullOrEmpty(id)) return;
            using (_sync.EnterScope())
            {
                if (_pending.Remove(id, out var tcs))
                    tcs.TrySetResult(line);
            }
        }
        catch { /* ignore */ }
    }

    public async Task SendNotificationAsync(string json, CancellationToken cancellationToken)
    {
        await _process.StandardInput.WriteAsync(json.AsMemory(), cancellationToken);
        await _process.StandardInput.FlushAsync(cancellationToken);
    }

    public async Task<object?> SendRequestAsync(string json, string id, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string>();
        using (_sync.EnterScope()) { _pending[id] = tcs; }
        await _process.StandardInput.WriteAsync(json.AsMemory(), cancellationToken);
        await _process.StandardInput.FlushAsync(cancellationToken);

        try
        {
            var result = await tcs.Task.WaitAsync(cancellationToken);
            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var err))
                throw new InvalidOperationException(err.TryGetProperty("message", out var m) ? m.GetString() : "MCP error");
            if (root.TryGetProperty("result", out var res))
                return JsonSerializer.Deserialize<object>(res.GetRawText());
        }
        finally
        {
            using (_sync.EnterScope()) { _pending.Remove(id, out _); }
        }
        return null;
    }
}
