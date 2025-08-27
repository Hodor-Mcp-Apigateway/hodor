namespace Papel.Integration.Domain.Common;

public abstract class WalletBaseTenantEntity : WalletBaseEntity, ITenantEntity
{
    public short TenantId { get; set; }
}
