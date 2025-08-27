namespace Papel.Integration.Events.Transaction;

public sealed record LoadMoneyCompletedEvent(
    long LoadMoneyRequestId,
    long SourceAccountId,
    long DestinationAccountId,
    decimal Amount,
    string OrderId) : IIntegrationEvent;
