namespace Papel.Integration.Domain.Events;

public sealed record ToDoItemUpdatedDomainEvent(Guid Id, string Name, string? Note) : INotification;
