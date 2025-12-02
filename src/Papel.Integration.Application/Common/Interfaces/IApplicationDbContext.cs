namespace Papel.Integration.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Domain.Entities;

public interface IApplicationDbContext
{
#pragma warning disable CA1716
    DbSet<T> Set<T>()
#pragma warning restore CA1716
        where T : class;

    DbContext AppDbContext { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task MigrateAsync();

    DbSet<Account> Accounts { get; }
    DbSet<AccountAction> AccountActions { get; }
    DbSet<Txn> Txns { get; }
    DbSet<Customer> Customers { get; }
    DbSet<LoadMoneyRequest> LoadMoneyRequests { get; }
    DbSet<ExternalReference> ExternalReferences { get; }
    DbSet<OperationLock> OperationLocks { get; }
    DbSet<MasterWhitelist> MasterWhitelists { get; }
    DbSet<DerivedWhitelist> DerivedWhitelists { get; }
    //Task SeedAsync();
}
