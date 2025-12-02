namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common;

/// <summary>
/// Turetilmis Whitelist kayitlari.
/// Master whitelist'teki bir hesap islem yaptiginda, karsi taraf bu tabloya eklenir
/// ve NewAccountLimit kontrolunden muaf tutulur.
/// </summary>
[Table("DerivedWhitelist", Schema = "txnLmt")]
public class DerivedWhitelist : WalletBaseTenantEntity
{
    [Key]
    [Column("DerivedWhitelistId")]
    public long Id { get; set; }

    /// <summary>
    /// Whitelist'e eklenen hesabin CustomerId'si
    /// </summary>
    public long CustomerId { get; set; }

    /// <summary>
    /// Whitelist'e eklenen hesabin AccountId'si
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Bu kaydi olusturan master whitelist'teki hesabin CustomerId'si
    /// </summary>
    public long DerivedFromCustomerId { get; set; }

    /// <summary>
    /// Bu kaydi olusturan master whitelist'teki hesabin AccountId'si
    /// </summary>
    public long DerivedFromAccountId { get; set; }

    /// <summary>
    /// Whitelist'e eklenmesine neden olan ilk transaction ID'si
    /// </summary>
    public long? FirstTransactionId { get; set; }
}