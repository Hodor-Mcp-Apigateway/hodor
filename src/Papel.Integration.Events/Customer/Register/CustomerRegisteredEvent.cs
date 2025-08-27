namespace Papel.Integration.Events.Customer.Register;

using Interfaces;

public sealed record CustomerRegisteredEvent(
    long CustomerId,
    string Email,
    string FirstName,
    string LastName) : IIntegrationEvent;
