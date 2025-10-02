namespace Papel.Integration.Domain.Common;

using Enums;

public abstract class BaseEntity<TKey>(TKey id) : IPkEntity<TKey>
{
    private readonly List<INotification> _domainEvents = [];

    public TKey Id { get; } = id;
    public Status StatusId { get; set; } = Status.Valid;
    public int CreaUserId { get; set; } = (int)SYSTEM_USER_CODES.SystemUserId;
    public DateTime CreaDate { get; set; } = DateTime.UtcNow;
    public int ModifUserId { get; set; } = (int)SYSTEM_USER_CODES.ModifUserId;
    public DateTime ModifDate { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsNew => EqualityComparer<TKey>.Default.Equals(Id, default);

    [NotMapped]
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(INotification domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();



}
