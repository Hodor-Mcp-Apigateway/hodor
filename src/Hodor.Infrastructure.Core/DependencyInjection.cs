namespace Hodor.Infrastructure.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Services;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, MachineDateTime>();
        return services;
    }

    public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services)
    {
        return services.AddHealthChecks()
            .AddCheck("hodor", () => HealthCheckResult.Healthy("Hodor MCP Gateway is running"));
    }
}
