namespace Papel.Integration.Infrastructure.Core.Extensions;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

public static class ServiceCollectionExtensions
{
    private const string MicroserviceNameProperty = "IntegrationService";

    public static IServiceCollection AddSerilog(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var serviceName = Assembly.GetCallingAssembly().GetName().Name!;

        services.AddLogging(loggingBuilder =>
                                loggingBuilder.AddSerilog(dispose: true));

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty(MicroserviceNameProperty, serviceName, true)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationIdHeader()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Async(config => config.Console(
                               outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}] [{ThreadId}] [{SourceContext}] {Message:lj} {NewLine}{Exception}"
                           ), bufferSize: 10000, blockWhenFull: false)
            .WriteTo.Async(config =>config.File(
                               path: "logs/"+MicroserviceNameProperty+".txt",
                               rollingInterval: RollingInterval.Day,
                               retainedFileCountLimit: 7,
                               buffered: true,
                               flushToDiskInterval: TimeSpan.FromSeconds(5),
                               outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}] [{ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                           ), bufferSize: 50000, blockWhenFull: false)
            .CreateLogger();

        Log.Logger = logger;

        var loggerFactory = new LoggerFactory();
        loggerFactory.AddSerilog(logger);
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        return services;
    }
}
