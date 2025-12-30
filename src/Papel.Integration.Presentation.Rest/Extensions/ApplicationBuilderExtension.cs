namespace Papel.Integration.Presentation.Rest.Extensions;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseRestPresentation(
        this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
    {
        app.ThrowIfNull();
        configuration.ThrowIfNull();
        env.ThrowIfNull();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("CorsPolicy")
            .UseExceptionHandler();

        return app;
    }
}
