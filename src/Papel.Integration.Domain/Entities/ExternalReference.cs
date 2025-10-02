namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common;

[Table("ExternalReference", Schema = "txn")]
public class ExternalReference : WalletBaseTenantEntity
{
    [Key]
    [Column("Id")]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReferenceId { get; set; } = string.Empty;
    
    
    [Timestamp]
    public uint Version { get; set; }
}