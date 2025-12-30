#pragma warning disable CA1506

var builder = WebApplication.CreateBuilder(args);

var envFileName = $".env.{builder.Environment.EnvironmentName}";
if (File.Exists(envFileName))
{
    DotNetEnv.Env.Load(envFileName);
}
else
{
    DotNetEnv.Env.Load();
}

builder.Configuration
    .AddCommandLine(args)
    .AddEnvironmentVariables()
    .Build();

var configuration = builder.Configuration;
var environment = builder.Environment;

//#if (EnableKafka)
builder.Host.UseKafkaMessageBroker(configuration);
//#endif

builder.Services
    .AddSerilog(configuration)
    .AddOptions()
<<<<<<< HEAD
    .AddNgpSqlPersistence(configuration)
=======
//#if (EnableRedisCache)
    .AddNgpSqlPersistence(configuration, (provider, optionsBuilder)
        => optionsBuilder.AddInterceptors(provider.GetRequiredService<SecondLevelCacheInterceptor>()))
    .AddEfCoreRedisCache(configuration)
//#else
    .AddNgpSqlPersistence(configuration)
//#endif
>>>>>>> b321969 (change rabbitmq to kafka and masstransit to wolverinefx)
    .AddApplication()
    .AddCoreInfrastructure()
//#if (EnableRest)
    .AddRestPresentation(configuration)
//#endif
//#if (EnableGrpc)
    .AddGrpcPresentation()
//#endif
//#if (EnableGraphQL)
    .AddGraphQLPresentation()
//#endif
//#if (EnableSignalR)
    .AddSignalRPresentation()
//#endif
    .AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder => resourceBuilder
        .AddService(
            serviceName: Assembly.GetExecutingAssembly().GetName().Name!,
            serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            serviceInstanceId: Environment.MachineName))
    .WithTracing(trackerBuilder => trackerBuilder
        .AddAspNetCoreInstrumentation(options => options.RecordException = true)
//#if (EnableRest)
        .AddRestOpenTelemetry()
//#endif
        .AddNgpSqlPersistenceOpenTelemetry()
//#if (EnableKafka)
        .AddWolverineOpenTelemetry()
//#endif
        .AddOtlpExporter()
    );

var app = builder.Build();

//#if (EnableRest)
app.UseRestPresentation(configuration, environment)
    .UseRouting();
app.UseAuthorization();
app.MapRestEndpoints();
//#endif
//#if (EnableGrpc)
app.MapGrpcEndpoints();
//#endif
//#if (EnableGraphQL)
app.MapGraphQLEndpoints();
//#endif
//#if (EnableSignalR)
app.MapHubEndpoints();
//#endif

app.MapHealthChecks("/health/startup");
app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = _ => false });

app.MapHealthChecks("/health/info", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

await app.RunAsync().ConfigureAwait(false);

#pragma warning disable CS1591
public partial class Program;
#pragma warning restore CS1591
