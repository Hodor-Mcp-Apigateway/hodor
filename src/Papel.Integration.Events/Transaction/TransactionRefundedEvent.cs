namespace Papel.Integration.Events.Transaction;

public sealed record TransactionRefundedEvent(
    long TransactionId,
    long SourceAccountId,
    long DestinationAccountId,
    decimal RefundAmount,
    string OrderId) : IIntegrationEvent;
