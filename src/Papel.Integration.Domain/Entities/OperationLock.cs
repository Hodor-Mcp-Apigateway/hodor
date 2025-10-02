namespace Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("OperationLock", Schema = "wallet")]
public class OperationLock
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string MethodName { get; set; } = string.Empty;

    [Required]
    public long SystemDate { get; set; }

    public short TenantId { get; set; }
}
