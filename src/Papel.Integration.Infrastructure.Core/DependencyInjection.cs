namespace Papel.Integration.Infrastructure.Core;

using Services;
using Application.Common.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, MachineDateTime>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ILockService, LockService>();

        return services;
    }
}
