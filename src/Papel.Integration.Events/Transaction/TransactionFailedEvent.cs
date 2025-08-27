namespace Papel.Integration.Events.Transaction;

public sealed record TransactionFailedEvent(
    long TransactionId,
    long SourceAccountId,
    decimal Amount,
    string ErrorMessage,
    string OrderId) : IIntegrationEvent;
