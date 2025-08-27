namespace Papel.Integration.Domain.Common;

public interface ITenantEntity : IEntity
{
    short TenantId { get; set; }
}

