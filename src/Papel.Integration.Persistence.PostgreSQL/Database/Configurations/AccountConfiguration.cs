
namespace Papel.Integration.Persistence.PostgreSQL.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account", "customer");

        builder.HasKey(account => account.AccountId);

        // Relationships
        builder.HasMany(account => account.SourceTransactions)
            .WithOne(txn => txn.SourceAccount)
            .HasForeignKey(txn => txn.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(account => account.DestinationTransactions)
            .WithOne(txn => txn.DestinationAccount)
            .HasForeignKey(txn => txn.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(account => account.LoadMoneyRequests)
            .WithOne(load => load.SourceAccount)
            .HasForeignKey(load => load.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(account => account.Customer)
            .WithMany(customer => customer.Accounts)
            .HasForeignKey(account => account.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(account => account.Balance)
            .HasColumnType("numeric(17,2)")
            .IsRequired();

        builder.Property(account => account.AvailableCashBalance)
            .HasColumnType("numeric(17,2)");

        builder.Property(account => account.WalletName)
            .HasMaxLength(250);

    }
}
