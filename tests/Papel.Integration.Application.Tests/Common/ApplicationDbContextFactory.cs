namespace Papel.Integration.Application.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Papel.Integration.Application.Common.Interfaces;
using Papel.Integration.Common.Tests;
using Infrastructure.Core.Services;
using Persistence.PostgreSQL;

public static class ApplicationDbContextFactory
{
    public static async Task<IApplicationDbContext> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.InitContext(
            AppMockFactory.CreateCurrentUserServiceMock(),new DbInitializer(),
            new MachineDateTime(),
            AppMockFactory.CreateMediatorMock());

        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        //await context.SeedAsync().ConfigureAwait(false);

        return context;
    }

    public static void Destroy(IApplicationDbContext context)
    {
        context.AppDbContext.Database.EnsureDeleted();

        context.AppDbContext.Dispose();
    }
}
