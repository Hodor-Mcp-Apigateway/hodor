namespace Papel.Integration.Events.Account;

public sealed record AccountSetAsDefaultEvent(
    long AccountId,
    long CustomerId) : IIntegrationEvent;
