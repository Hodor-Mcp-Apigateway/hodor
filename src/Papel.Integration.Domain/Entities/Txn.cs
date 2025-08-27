namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Common;
using Events;
using Integration.Events.Transaction;

[Table("Txn", Schema = "txn")]
public class Txn : WalletBaseTenantEntity
{
    [Column("TxnId")]
    public long TxnId { get; set; }
    public short TxnStatusId { get; set; }
    public short TxnTypeId { get; set; }
    public string SourceBankAccountNumber { get; set; } = string.Empty;
    public string DestinationBankAccountNumber { get; set; } = string.Empty;
    public long SourceAccountId { get; set; }
    public long DestinationAccountId { get; set; }
    public decimal? Amount { get; set; }
    public decimal? ExpenseAmount { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? NewBalance { get; set; }
    public long? FirmReferenceNumber { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Column("RefundTxnId")]
    public long? RefundLoadMoneyRequestId { get; set; }
    public bool? IsRefunded { get; set; }
    public int? ResultCode { get; set; }
    public string ResultDescription { get; set; } = string.Empty;
    public string VposAuthCode { get; set; } = string.Empty;
    public string MaskedCreditCard { get; set; } = string.Empty;
    public string StoredCardId { get; set; } = string.Empty;
    public string VposProcReturnCode { get; set; } = string.Empty;
    public string VposErrMsg { get; set; } = string.Empty;
    public string VposOrderId { get; set; } = string.Empty;
    public string VposCardMask { get; set; } = string.Empty;
    public string VposCardHolderName { get; set; } = string.Empty;
    public short? MerchantCategoryCode { get; set; }
    public short? MerchantCategoryGroupId { get; set; }
    public string InvoiceTypeName { get; set; } = string.Empty;
    public int? InvoiceInstitutionNumber { get; set; }
    public string InvoiceCustomerNameMasked { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceDetail { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string PaymentPurpose { get; set; } = string.Empty;
    public string CardToken { get; set; } = string.Empty;
    public decimal? RefundedAmount { get; set; }
    public decimal? OrgAmount { get; set; }
    public string QRReferenceNumber { get; set; } = string.Empty;
    public string RemoteIpAddress { get; set; } = string.Empty;
    public short? InvoiceAgencyId { get; set; }
    public decimal? ExpenseRate { get; set; }
    public decimal? FixedAmount { get; set; }
    public bool? IsContacless { get; set; }
    public decimal? UsedAvailableCashBalace { get; set; }
    public decimal? RefundedAvailableCashBalance { get; set; }
    public bool? IsInternational { get; set; }
    public string AcquirerInstutionId { get; set; } = string.Empty;
    public string? InvoiceInstitutionName { get; set; }
    public string? FirmReferenceCode { get; set; }
    public string? SenderBankCode { get; set; }
    public string? SenderBankName { get; set; }

    public virtual Account SourceAccount { get; set; } = null!;
    public virtual Account DestinationAccount { get; set; } = null!;

    public void CompleteTransaction()
    {
        TxnStatusId = 1;
        ResultCode = 0;
        ResultDescription = "Transaction completed successfully";
        ModifDate = DateTime.Now;
        AddDomainEvent(new TransactionCompltedDomainEvent(
            TxnId, SourceAccountId, DestinationAccountId, Amount ?? 0, OrderId));
    }

    public void FailTransaction(string errorMessage, int errorCode)
    {
        TxnStatusId = 3; // Failed
        ResultCode = errorCode;
        ResultDescription = errorMessage;
        ModifDate = DateTime.Now;
        AddDomainEvent(new TransactionFailedEvent(
            TxnId, SourceAccountId, Amount ?? 0, errorMessage, OrderId));
    }

    public void RefundTransaction(decimal refundAmount)
    {
        IsRefunded = true;
        RefundedAmount = refundAmount;
        ModifDate = DateTime.Now;
        AddDomainEvent(new TransactionRefundedEvent(
            TxnId, SourceAccountId, DestinationAccountId, refundAmount, OrderId));
    }
}
