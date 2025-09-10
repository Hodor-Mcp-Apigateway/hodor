namespace Papel.Integration.EFCore.Caching.Redis.Extensions;

using Configuration;
using Validations;

public static class ServiceCollectionExtensions
{
    private const string ProviderName = "EFCoreCahce";
    private const string SerializerName = "proto";
    private const string EfPrefix = "EF_";

    /// <summary>
    /// Adds EFCore second level caching
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the MassTransits to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing settings to be used.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEfCoreRedisCache(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.ThrowIfNull();
        configuration.ThrowIfNull();

        // ÖNCE options ve validation ekle
        services.AddOptions<RedisConnection>()
            .Bind(configuration.GetSection(CacheConfigurationSection.SectionName))
            .ValidateFluentValidation()
            .ValidateOnStart();

        // Validator'ı singleton olarak kaydet
        services.AddSingleton<IValidator<RedisConnection>, RedisConnectionValidator>();

        // EFCore second level cache
        services.AddEFSecondLevelCache(options =>
            options.UseEasyCachingCoreProvider(ProviderName, isHybridCache: false)
                .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30))
                .ConfigureLogging(enable: true)
                .UseCacheKeyPrefix(EfPrefix)
        );

        // EasyCaching configuration - BuildServiceProvider KULLANMA
        services.AddEasyCaching(option =>
        {
            // Configuration'ı direkt IConfiguration'dan al
            var section = configuration.GetSection(CacheConfigurationSection.SectionName);
            var connectionString = section["ConnectionString"] ?? 
                                 Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__REDISCACHECONNECTION__CONNECTIONSTRING");
            var healthCheckEnabled = section.GetValue<bool>("HealthCheckEnabled") || 
                                   Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__REDISCACHECONNECTION__HEALTHCHECKENABLED")?.ToLowerInvariant() == "true";
            
            var redisConfig = new RedisConnection
            {
                ConnectionString = connectionString,
                HealthCheckEnabled = healthCheckEnabled
            };

            if (string.IsNullOrEmpty(redisConfig?.ConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured");
            }

            option.WithJson(SerializerName);
            option.UseRedis(config =>
            {
                config.DBConfig.ConfigurationOptions = ConfigurationOptions.Parse(redisConfig.ConnectionString);
                config.SerializerName = SerializerName;
            }, ProviderName);

            // Health check'i ayrıca ekle
            if (redisConfig.HealthCheckEnabled)
            {
                services.AddHealthChecks().AddRedis(redisConfig.ConnectionString, ProviderName);
            }
        });

        return services;
    }
}
