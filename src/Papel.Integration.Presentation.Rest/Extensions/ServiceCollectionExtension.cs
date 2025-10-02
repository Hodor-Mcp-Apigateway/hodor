namespace Papel.Integration.Presentation.Rest.Extensions;

using Filters;
using Middleware;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRestPresentation(
        this IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors");
        var corsParams = corsSection.GetChildren().Select(child => child.Value).Where(value => !string.IsNullOrEmpty(value)).ToList();

        ArgumentNullException.ThrowIfNull(corsParams);

        services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
        {
            builder.WithOrigins([.. corsParams!,])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }));

        services.AddHttpContextAccessor()
            .AddSwagger(configuration, Assembly.GetExecutingAssembly())
            .AddValidatorsFromAssemblyContaining<IApplicationDbContext>(filter:null, includeInternalTypes:true)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .AddControllers(options => options.Filters.Add<CustomExceptionFilterAttribute>())
            .AddApplicationPart(Assembly.GetExecutingAssembly());

        return services;
    }
}
