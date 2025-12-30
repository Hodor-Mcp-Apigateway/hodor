namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using Common;
using Papel.Integration.Events.Transaction;
using System.ComponentModel.DataAnnotations.Schema;


[Table("LoadMoneyRequest", Schema = "txn")]
public class LoadMoneyRequest : WalletBaseTenantEntity
{
    [Column("LoadMoneyRequestId")]
    public long LoadMoneyRequestId { get; set; }
    public long SourceAccountId { get; set; }
    public long DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal NewBalance { get; set; }
    public decimal? ExpenseAmount { get; set; }
    public short CurrencyId { get; set; }
    public long FirmReferenceNumber { get; set; }
    public bool? IsRefunded { get; set; }
    public string Description { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public short TxnTypeId { get; set; }
    public int ResultCode { get; set; }
    public string ResultDescription { get; set; } = string.Empty;
    public string VposOrderId { get; set; } = string.Empty;
    public string MaskedCreditCard { get; set; } = string.Empty;
    public string VposCardholderName { get; set; } = string.Empty;
    public string StoredCardId { get; set; } = string.Empty;
    public string VposAuthCode { get; set; } = string.Empty;
    public string SourceBankAccountNumber { get; set; } = string.Empty;
    public string DestinationBankAccountNumber { get; set; } = string.Empty;
    public string CardToken { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public short? MerchantCategoryCode { get; set; }
    public short? MerchantCategoryGroupId { get; set; }
    public long? RefundTxnId { get; set; }
    public decimal? RefundedAmount { get; set; }
    public string RemoteIpAddress { get; set; } = string.Empty;
    public short? InvoiceAgencyId { get; set; }
    public decimal? OrgAmount { get; set; }
    public decimal? ExpenseRate { get; set; }
    public decimal? FixedAmount { get; set; }
    public bool? IsContacless { get; set; }
    public decimal? UsedAvailableCashBalace { get; set; }
    public decimal? RefundedAvailableCashBalance { get; set; }
    public bool? IsInternational { get; set; }
    public string AcquirerInstutionId { get; set; } = string.Empty;
    public long? ReferrerId { get; set; }

    // Navigation Properties
    public virtual Account SourceAccount { get; set; } = null!;
    public virtual Account DestinationAccount { get; set; } = null!;

    // Domain Events Methods
    public void CompleteLoadMoneyRequest(decimal newBalance)
    {
        NewBalance = newBalance;
        ResultCode = 0;
        ResultDescription = "Success";
        ModifDate = DateTime.UtcNow;
        AddDomainEvent(new LoadMoneyCompletedEvent(
            LoadMoneyRequestId, SourceAccountId, DestinationAccountId, Amount, OrderId));
    }

    public void FailLoadMoneyRequest(string errorMessage, int errorCode)
    {
        ResultCode = errorCode;
        ResultDescription = errorMessage;
        ModifDate = DateTime.UtcNow;
        AddDomainEvent(new LoadMoneyFailedEvent(
            LoadMoneyRequestId, SourceAccountId, Amount, errorMessage, OrderId));
    }
}
