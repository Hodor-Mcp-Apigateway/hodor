namespace Papel.Integration.Common.Model;

public sealed record StructuredLog
{
    public string LogPrefix { get; init; } = string.Empty;
    public string ReferenceId { get; init; } = string.Empty;
    public string System { get; init; } = string.Empty;
    public string Entity { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}