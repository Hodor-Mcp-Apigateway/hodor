namespace Papel.Integration.Events.Customer.EmailVerified;

using Interfaces;

public sealed record CustomerEmailVerifiedEvent(
    long CustomerId,
    string Email) : IIntegrationEvent;
