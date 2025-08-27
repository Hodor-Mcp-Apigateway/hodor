namespace Papel.Integration.Events.Transaction;

public sealed record MoneyTransferInitiatedEvent(
    long SourceAccountId,
    long DestinationAccountId,
    decimal Amount,
    string OrderId,
    string Description) : IIntegrationEvent;
