namespace Papel.Integration.Domain.Entities;
using Enums;

[Table("AccountAction", Schema = "customer")]
public class AccountAction
{
    public long AccountActionId { get; set; }
    public long AccountId { get; set; }
    public long ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public decimal BeforeAccountBalance { get; set; }
    public decimal AfterAccountBalance { get; set; }
    public string Description { get; set; } = string.Empty;
    public short AccountActionTypeId { get; set; }
    public short TxnTypeId { get; set; }
    public string TargetFullName { get; set; } = string.Empty;
    public string ReceiptNo { get; set; } = string.Empty;
    public Status StatusId { get; set; } = Status.Valid;
    public int CreaUserId { get; set; } = (int)SYSTEM_USER_CODES.SystemUserId;
    public DateTime CreaDate { get; set; } = DateTime.Now;
    public int ModifUserId { get; set; } = (int)SYSTEM_USER_CODES.ModifUserId;
    public DateTime ModifDate { get; set; } = DateTime.Now;
    public short TenantId { get; set; }
}
