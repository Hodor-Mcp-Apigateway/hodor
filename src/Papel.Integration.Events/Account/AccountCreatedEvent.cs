namespace Papel.Integration.Events.Account;

public sealed record AccountCreatedEvent(
    long AccountId,
    long CustomerId,
    string WalletName,
    short CurrencyId) : IIntegrationEvent;

