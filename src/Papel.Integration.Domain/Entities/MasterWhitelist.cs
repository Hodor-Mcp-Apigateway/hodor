namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common;

/// <summary>
/// Master Whitelist'e eklenen hesaplar (ornegin: Datajiro).
/// Bu hesaplar para gonderdiginde, karsi taraf otomatik olarak DerivedWhitelist'e eklenir.
/// Hem master whitelist'tekiler hem de turetilen kayitlar NewAccountLimit kontrolunden muaf tutulur.
/// </summary>
[Table("MasterWhitelist", Schema = "txnLmt")]
public class MasterWhitelist : WalletBaseTenantEntity
{
    [Key]
    [Column("MasterWhitelistId")]
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
    /// Bu hesap aktif mi? (true = islem yaptiginda karsi taraf otomatik DerivedWhitelist'e eklenir)
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Hesabin ismi/aciklamasi (ornegin: "Datajiro")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Opsiyonel aciklama
    /// </summary>
    public string? Description { get; set; }
}