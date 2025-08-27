namespace Papel.Integration.Events.Customer.Update;

using Interfaces;

public sealed record CustomerUpdatedEvent(
    long CustomerId,
    string FirstName,
    string LastName,
    string Email) : IIntegrationEvent;
