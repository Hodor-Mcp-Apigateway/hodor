namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using System.ComponentModel.DataAnnotations;
using Common;
using Enums;


using System.ComponentModel.DataAnnotations.Schema;
using Integration.Events.Account;

[Table("Account", Schema = "customer")]
public class Account : WalletBaseTenantEntity
{
    [Key]
    [Column("AccountId")]
    public long AccountId { get; set; }
    public long CustomerId { get; set; }
    public string WalletName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public short CurrencyId { get; set; }
    public short AccountStatusId { get; set; }
    public bool IsDefault { get; set; }
    public decimal? AvailableCashBalance { get; set; }

    [Timestamp]
    public uint Version { get; set; }

    // Navigation Properties
    [ForeignKey("SourceAccountId")]
    public ICollection<LoadMoneyRequest> LoadMoneyRequests { get; set; } = new List<LoadMoneyRequest>();

    [ForeignKey("CustomerId")]
    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Txn> SourceTransactions { get; set; } = new List<Txn>();
    public virtual ICollection<Txn> DestinationTransactions { get; set; } = new List<Txn>();

    // Business Methods with Domain Events
    public void UpdateBalance(decimal oldBalance, decimal newBalance, string transactionType)
    {
        Balance = newBalance;
        ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
        ModifDate = DateTime.UtcNow;

        AddDomainEvent(new AccountBalanceUpdatedEvent(
            AccountId, CustomerId, oldBalance, newBalance, transactionType));
    }

    public void CreateAccount(long customerId, string walletName, short currencyId)
    {
        CustomerId = customerId;
        WalletName = walletName;
        CurrencyId = currencyId;
        Balance = 0;

        AddDomainEvent(new AccountCreatedEvent(AccountId, customerId, walletName, currencyId));
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
        ModifDate = DateTime.UtcNow;

        AddDomainEvent(new AccountSetAsDefaultEvent(AccountId, CustomerId));
    }

    public void UpdateAvailableCashBalance(decimal amount)
    {
        var currentAvailableCashBalance = AvailableCashBalance ?? 0;
        
        if (currentAvailableCashBalance >= amount)
        {
            AvailableCashBalance = currentAvailableCashBalance - amount;
        }
        else if (currentAvailableCashBalance > 0)
        {
            // Use all available cash balance and the rest will come from regular balance
            AvailableCashBalance = 0;
        }
        
        ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
        ModifDate = DateTime.UtcNow;
    }
}
