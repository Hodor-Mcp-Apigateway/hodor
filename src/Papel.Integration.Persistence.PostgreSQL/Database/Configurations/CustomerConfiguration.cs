namespace Papel.Integration.Persistence.PostgreSQL.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.AggregatesModel.ToDoAggregates.Entities;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer", "customer");

        builder.HasKey(customer => customer.CustomerId);

        // Relationships
        builder.HasMany(customer => customer.Accounts)
            .WithOne(account => account.Customer)
            .HasForeignKey(account => account.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Property configurations
        builder.Property(customer => customer.FirstName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.LastName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(200);

        builder.Property(customer => customer.CustomerNo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Phone)
            .HasMaxLength(115);

    }
}
