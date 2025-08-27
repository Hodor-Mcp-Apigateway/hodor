namespace Papel.Integration.Application.Common.Interfaces;

public interface IDbInitializer
{
    Task SeedAsync(IApplicationDbContext context, CancellationToken cancellationToken = default);
}
