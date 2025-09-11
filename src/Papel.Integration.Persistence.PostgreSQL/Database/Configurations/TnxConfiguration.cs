
namespace Papel.Integration.Persistence.PostgreSQL.Database.Configurations;

public class TxnConfiguration : IEntityTypeConfiguration<Txn>
{
    public void Configure(EntityTypeBuilder<Txn> builder)
    {
        builder.ToTable("Txn", "txn");

        builder.HasKey(txn => txn.TxnId);

        // Relationships
        builder.HasOne(txn => txn.SourceAccount)
            .WithMany(account => account.SourceTransactions)
            .HasForeignKey(txn => txn.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(txn => txn.DestinationAccount)
            .WithMany(account => account.DestinationTransactions)
            .HasForeignKey(txn => txn.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Property configurations
        builder.Property(txn => txn.Amount)
            .HasColumnType("numeric(17,4)");

        builder.Property(txn => txn.CurrentBalance)
            .HasColumnType("numeric(17,4)");

        builder.Property(txn => txn.NewBalance)
            .HasColumnType("numeric(17,4)");

        builder.Property(txn => txn.OrderId)
            .HasMaxLength(150);

        builder.Property(txn => txn.Description)
            .HasMaxLength(400);

        builder.Property(txn => txn.ResultDescription)
            .HasMaxLength(250);

    }
}
