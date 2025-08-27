namespace Papel.Integration.Presentation.Grpc.Extensions;

using ProtoBuf.Grpc.Server;

public static class EndpointRouteBuilderExtension
{
    public static IEndpointRouteBuilder MapGrpcEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull( endpointRouteBuilder, nameof (endpointRouteBuilder));
        endpointRouteBuilder.MapCodeFirstGrpcReflectionService();
        return endpointRouteBuilder;
    }
}
