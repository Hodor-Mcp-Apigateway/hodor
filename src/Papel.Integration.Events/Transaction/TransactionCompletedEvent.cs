namespace Papel.Integration.Events.Transaction;

public sealed record TransactionCompletedEvent(
    long TransactionId,
    long SourceAccountId,
    long DestinationAccountId,
    decimal Amount,
    string OrderId) : IIntegrationEvent;
