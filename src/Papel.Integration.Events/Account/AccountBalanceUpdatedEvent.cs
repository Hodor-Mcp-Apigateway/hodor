namespace Papel.Integration.Events.Account;

public sealed record AccountBalanceUpdatedEvent(
    long AccountId,
    long CustomerId,
    decimal OldBalance,
    decimal NewBalance,
    string TransactionType) : IIntegrationEvent;
