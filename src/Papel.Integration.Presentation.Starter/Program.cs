#pragma warning disable CA1506

using Papel.Integration.Infrastructure.Core.Extensions;

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

builder.Services
    .AddSerilog(configuration)
    .AddOptions()
    .AddNgpSqlPersistence(configuration)
    .AddApplication()
    .AddCoreInfrastructure()
    .AddRestPresentation(configuration)
    .AddGrpcPresentation()
    .AddGraphQLPresentation()
    .AddSignalRPresentation()
    .AddMessageBroker(configuration)
    .AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder => resourceBuilder
        .AddService(
            serviceName: Assembly.GetExecutingAssembly().GetName().Name!,
            serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            serviceInstanceId: Environment.MachineName))
    .WithTracing(trackerBuilder => trackerBuilder
        .AddAspNetCoreInstrumentation(options => options.RecordException = true)
        .AddRestOpenTelemetry()
        .AddNgpSqlPersistenceOpenTelemetry()
        .AddMassTransitOpenTelemetry()
        .AddOtlpExporter()
        //.AddConsoleExporter()
    );

var app = builder.Build();
app.UseRestPresentation(configuration, environment)
    .UseRouting();
app.UseAuthorization();
app.MapRestEndpoints();
app.MapGrpcEndpoints();
app.MapGraphQLEndpoints();
app.MapHubEndpoints();

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
