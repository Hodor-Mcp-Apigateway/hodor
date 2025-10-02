namespace Papeload.Integration.Persistence.PostgreSQload.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


public class LoadMoneyRequestConfiguration : IEntityTypeConfiguration<LoadMoneyRequest>
{
    public void Configure(EntityTypeBuilder<LoadMoneyRequest> builder)
    {
        builder.ToTable("LoadMoneyRequest", "txn");

        builder.HasKey(load => load.LoadMoneyRequestId);

        // Relationships
        builder.HasOne(load => load.SourceAccount)
            .WithMany(account  => account.LoadMoneyRequests)
            .HasForeignKey(load => load.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Property configurations
        builder.Property(load => load.Amount)
            .HasColumnType("numeric(17,4)");

        builder.Property(load => load.CurrentBalance)
            .HasColumnType("numeric(17,4)");

        builder.Property(load => load.NewBalance)
            .HasColumnType("numeric(17,4)");

        builder.Property(load => load.OrderId)
            .HasMaxLength(150);

        builder.Property(load => load.Description)
            .HasMaxLength(400);

        builder.Property(load => load.ResultDescription)
            .HasMaxLength(600);

    }
}
