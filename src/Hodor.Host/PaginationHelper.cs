using System.Text;
using System.Text.Json;

namespace Hodor.Host;

/// <summary>
/// Cursor-based pagination (MCP 2025-11-25 / mcp.gateway compatible).
/// Cursor format: base64({"offset": N}).
/// </summary>
public static class PaginationHelper
{
    private const int DefaultPageSize = 100;
    private const int MaxPageSize = 500;

    public static int ParseOffset(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return 0;
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var json = Encoding.UTF8.GetString(bytes);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("offset", out var offsetEl))
                return Math.Max(0, offsetEl.GetInt32());
        }
        catch { /* invalid cursor */ }
        return 0;
    }

    public static string? EncodeNextCursor(int offset) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"offset\":{offset}}}"));

    public static (int Offset, int PageSize) Parse(string? cursor, int? pageSize)
    {
        var offset = ParseOffset(cursor);
        var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        return (offset, size);
    }

    public static IReadOnlyList<T> Apply<T>(IReadOnlyList<T> source, int offset, int pageSize, out string? nextCursor)
    {
        nextCursor = null;
        if (offset >= source.Count) return [];
        var page = source.Skip(offset).Take(pageSize + 1).ToList();
        if (page.Count > pageSize)
        {
            page.RemoveAt(pageSize);
            nextCursor = EncodeNextCursor(offset + pageSize);
        }
        return page;
    }
}
