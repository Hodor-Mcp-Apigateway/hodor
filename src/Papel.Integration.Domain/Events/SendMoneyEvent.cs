namespace Papel.Integration.Domain.Events;

public sealed record SendMoneyEvent(Guid Id, string Title, string? Note) : INotification;
