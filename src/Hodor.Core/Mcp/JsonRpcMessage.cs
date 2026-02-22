namespace Hodor.Core.Mcp;

/// <summary>
/// JSON-RPC 2.0 message (MCP protocol base).
/// </summary>
public record JsonRpcMessage
{
    public required string Jsonrpc { get; init; } = "2.0";
    public string? Id { get; init; }
    public string? Method { get; init; }
    public object? Params { get; init; }
    public object? Result { get; init; }
    public JsonRpcError? Error { get; init; }
}

public record JsonRpcError(int Code, string Message, object? Data = null);
