namespace Papel.Integration.Persistence.PostgreSQL.Configurations;

using Domain.Entities;

public class AccountActionConfiguration : IEntityTypeConfiguration<AccountAction>
{
    public void Configure(EntityTypeBuilder<AccountAction> builder)
    {
        builder.ToTable("AccountAction", "customer");

        builder.HasKey(action => action.AccountActionId);

        // Properties
        builder.Property(action => action.Amount)
            .HasColumnType("numeric(17,4)")
            .IsRequired();

        builder.Property(action => action.BeforeAccountBalance)
            .HasColumnType("numeric(17,4)")
            .IsRequired();

        builder.Property(action => action.AfterAccountBalance)
            .HasColumnType("numeric(17,4)")
            .IsRequired();

        builder.Property(action => action.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(action => action.TargetFullName)
            .HasMaxLength(250);

        builder.Property(action => action.ReceiptNo)
            .HasMaxLength(100);

        builder.Property(action => action.AccountActionTypeId)
            .IsRequired();

        builder.Property(action => action.TxnTypeId)
            .IsRequired();

        builder.Property(action => action.ReferenceId)
            .IsRequired();
    }
}
