namespace Papel.Integration.Persistence.PostgreSQL.Database.Configurations;

public class AuditableConfiguration<T> where T : class, IEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(entity => entity.CreaDate).HasColumnType("timestamp")
            .IsRequired();

        //builder.Property(entity => entity.CreatedBy)
        //    .IsRequired();

        builder.Property(entity => entity.ModifDate).HasColumnType("timestamp");

        //builder.Property(entity => entity.ModifiedBy);
    }
}
