namespace Papel.Integration.Persistence.PostgreSQL.Configurations;

public class OperationLockConfiguration : IEntityTypeConfiguration<OperationLock>
{
    public void Configure(EntityTypeBuilder<OperationLock> builder)
    {
        builder.ToTable("OperationLock", "wallet");

        builder.HasKey(entity => entity.Id);

        // Composite unique constraint for CustomerId, MethodName and SystemDate
        builder.HasIndex(entity => new { entity.CustomerId, entity.MethodName, entity.SystemDate })
            .IsUnique()
            .HasDatabaseName("IX_OperationLock_CustomerId_MethodName_SystemDate");

        builder.Property(entity => entity.Id)
            .IsRequired();

        builder.Property(entity => entity.CustomerId)
            .IsRequired();

        builder.Property(entity => entity.MethodName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entity => entity.SystemDate)
            .IsRequired();

        builder.Property(entity => entity.TenantId)
            .IsRequired();
    }
}