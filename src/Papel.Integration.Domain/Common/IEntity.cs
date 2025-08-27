namespace Papel.Integration.Domain.Common;

using Enums;

public interface IEntity
{
    bool IsNew { get; }

    IReadOnlyCollection<INotification> DomainEvents { get; }

    void AddDomainEvent(INotification domainEvent);

    void RemoveDomainEvent(INotification domainEvent);

    void ClearDomainEvents();

    Status StatusId { get; set; }
    int CreaUserId { get; set; }
    DateTime CreaDate { get; set; }
    int ModifUserId { get; set; }
    DateTime ModifDate { get; set; }

}
