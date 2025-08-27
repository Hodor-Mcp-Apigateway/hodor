namespace Papel.Integration.Domain.Events;

public sealed record TransactionCompltedDomainEvent(long TxnId, long SourceAccountId, long DestinationAccountId, decimal amount, string orderId) : INotification;
