namespace Papel.Integration.Common.Model;

public sealed record ApiResponse<T>
{
    public T? Data { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed record ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}