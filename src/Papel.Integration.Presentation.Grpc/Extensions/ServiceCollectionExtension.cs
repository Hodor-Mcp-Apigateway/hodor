namespace Papel.Integration.Presentation.Grpc.Extensions;

using Interceptor;
using ProtoBuf.Grpc.Server;
using Mapper = MapsterMapper.Mapper;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGrpcPresentation(
        this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());

        services.TryAddSingleton<IMapper>(new Mapper(typeAdapterConfig));
        services.TryAddSingleton<IMapper, ServiceMapper>();
        services.AddCodeFirstGrpcReflection();

        services.AddCodeFirstGrpc(opt => opt.Interceptors.Add<GrpcServerExceptionInterceptor>());

        return services;
    }
}
