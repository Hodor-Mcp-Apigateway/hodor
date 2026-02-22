using Hodor.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Hodor.Application.Mcp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMcpGateway(this IServiceCollection services)
    {
        services.AddSingleton<IHodorGateway, HodorGatewayService>();
        return services;
    }
}
