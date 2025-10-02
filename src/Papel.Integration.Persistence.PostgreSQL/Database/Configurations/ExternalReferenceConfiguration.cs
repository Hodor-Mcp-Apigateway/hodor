namespace Papel.Integration.Persistence.PostgreSQL.Configurations;

public class ExternalReferenceConfiguration : IEntityTypeConfiguration<ExternalReference>
{
    public void Configure(EntityTypeBuilder<ExternalReference> builder)
    {
        builder.ToTable("ExternalReference", "txn");

        builder.HasKey(entity => entity.Id);

        // Unique constraint for ReferenceId
        builder.HasIndex(entity => entity.ReferenceId)
            .IsUnique()
            .HasDatabaseName("IX_ExternalReference_ReferenceId");

        builder.Property(entity => entity.ReferenceId)
            .HasMaxLength(50)
            .IsRequired();
    }
}