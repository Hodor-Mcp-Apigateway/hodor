namespace Papel.Integration.Presentation.Rest.Extensions;

public static class EndpointRouteBuilderExtension
{
    public static IEndpointRouteBuilder MapRestEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllers();
        return endpointRouteBuilder;
    }
}
