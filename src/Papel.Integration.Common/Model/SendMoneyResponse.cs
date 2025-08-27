namespace Papel.Integration.Application.Common.Models;

public sealed record SendMoneyResponse
{
    public long TransactionId { get; init; }
    public string OrderId { get; init; } = string.Empty;
    public decimal NewSourceBalance { get; init; }
    public decimal NewDestinationBalance { get; init; }
    public bool IsSuccessful { get; init; }
    public string ResultMessage { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
}
