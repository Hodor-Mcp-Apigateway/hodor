namespace Papel.Integration.Persistence.PostgreSQL.Extensions;

using Configuration;
using Microsoft.Extensions.Options;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add PostgresSQL as a persistence layer
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the MassTransits to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing settings to be used.</param>
    /// <param name="optionsBuilder"></param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddNgpSqlPersistence(this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsBuilder = null)
    {
        configuration.ThrowIfNull(nameof(configuration));

        services.AddOptions<PostgresConnection>()
            .Bind(configuration.GetSection(DbConfigurationSection.SectionName))
            .ValidateFluently()
            .ValidateOnStart();

        services.AddSingleton<IValidator<PostgresConnection>, PostgresConnectionValidator>();

        ConfigureDbContextFactory(services, optionsBuilder, configuration);

        services.TryAddScoped<IDbInitializer, DbInitializer>();
        services.TryAddScoped<ApplicationDbContextFactory>();

        services.TryAddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContextFactory>().CreateDbContext());

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(RepositoryBase<>)))
            .AsMatchingInterface()
            .WithScopedLifetime());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        return services;
    }

    private static IServiceCollection ConfigureDbContextFactory(this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsBuilder,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration[$"{DbConfigurationSection.SectionName}:ConnectionString"];

        services.AddPooledDbContextFactory<ApplicationDbContext>((provider, options) =>
        {
            optionsBuilder?.Invoke(provider, options);

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
            });

            // Internal service provider kullanma - gerekirse aç
            // options.UseInternalServiceProvider(provider);

            var postgresConfig = provider.GetService<IOptions<PostgresConnection>>()?.Value;
            if (postgresConfig?.LoggingEnabled == true)
            {
                options.EnableDbLogging();
            }
        });

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddHealthChecks()
                .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db", "sql", "ready" });
        }

        return services;
    }

    private static DbContextOptionsBuilder EnableDbLogging(this DbContextOptionsBuilder builder) => builder
        .LogTo(
            msg => Log.Logger.Information("{Msg}", msg),
            new[] { DbLoggerCategory.Database.Name })
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging();
}
