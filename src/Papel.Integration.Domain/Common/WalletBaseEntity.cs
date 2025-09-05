namespace Papel.Integration.Domain.Common;

using Enums;

public abstract class WalletBaseEntity : IEntity
{
    public Status StatusId { get; set; } = Status.Valid;
    public int CreaUserId { get; set; } = (int)SYSTEM_USER_CODES.SystemUserId;
    public DateTime CreaDate { get; set; } = DateTime.UtcNow;
    public int ModifUserId { get; set; } = (int)SYSTEM_USER_CODES.ModifUserId;
    public DateTime ModifDate { get; set; } = DateTime.UtcNow;

    private readonly List<INotification> _domainEvents = [];

    public bool IsNew => CreaDate == ModifDate && (DateTime.UtcNow - CreaDate).TotalSeconds < 1;

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(INotification domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
