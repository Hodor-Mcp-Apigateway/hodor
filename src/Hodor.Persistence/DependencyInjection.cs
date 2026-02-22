using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hodor.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration["ConnectionStrings:PostgreSQL"]
            ?? "Host=localhost;Port=5432;Database=hodor;Username=postgres;Password=postgres";

        services.AddDbContext<HodorDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MigrationsAssembly(typeof(HodorDbContext).Assembly.FullName);
            });
        });

        return services;
    }
}
