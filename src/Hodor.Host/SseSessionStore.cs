using System.Collections.Concurrent;
using System.Text.Json;

namespace Hodor.Host;

/// <summary>
/// Stores active SSE sessions for MCP transport.
/// </summary>
public static class SseSessionStore
{
    private static readonly ConcurrentDictionary<string, HttpResponse> Sessions = new();

    public static string CreateSession(HttpResponse response)
    {
        var id = Guid.NewGuid().ToString("N")[..16];
        Sessions[id] = response;
        return id;
    }

    public static bool TryGet(string sessionId, out HttpResponse? response)
    {
        return Sessions.TryGetValue(sessionId, out response);
    }

    public static void Remove(string sessionId)
    {
        Sessions.TryRemove(sessionId, out _);
    }

    public static async Task SendMessageAsync(string sessionId, object message, CancellationToken ct = default)
    {
        if (!Sessions.TryGetValue(sessionId, out var response))
            return;
        var json = JsonSerializer.Serialize(message);
        var payload = $"event: message\ndata: {json}\n\n";
        await response.WriteAsync(payload, ct);
        await response.Body.FlushAsync(ct);
    }
}
