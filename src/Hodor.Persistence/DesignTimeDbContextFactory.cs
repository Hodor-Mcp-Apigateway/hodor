using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Hodor.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HodorDbContext>
{
    public HodorDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("PostgreSQL")
            ?? "Host=localhost;Port=5432;Database=hodor;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<HodorDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MigrationsAssembly(typeof(HodorDbContext).Assembly.FullName);
            })
            .Options;

        return new HodorDbContext(options);
    }
}
