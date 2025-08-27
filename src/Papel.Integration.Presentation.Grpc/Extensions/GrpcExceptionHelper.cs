namespace Papel.Integration.Presentation.Grpc.Extensions;

using System.Text;
using System.Text.Json;
using global::Grpc.Core;

internal static class GrpcExceptionHelper
{
    private const string MetadataExceptionStore = "exception-bin";

    private static JsonSerializerOptions JsonSerializerOptions
        => new JsonSerializerOptions
           {
               Converters = { new ExceptionConverter() },
           };

    public static RpcException PrepareServerException(Exception except)
    {
        var exception = JsonSerializer.Serialize(except, JsonSerializerOptions);
        var exceptionByteArray = Encoding.UTF8.GetBytes(exception);

        var metadata = new Metadata();
        metadata.Add(MetadataExceptionStore, exceptionByteArray);

        return new RpcException(new Status(StatusCode.Internal, "Error"), metadata);
    }

    public static void PrepareClientException(RpcException except)
    {
        if (!except.Trailers.Any(entity => entity.Key.Equals(MetadataExceptionStore, StringComparison.Ordinal)))
        {
            return;
        }

        var bytesValue= except.Trailers.GetValueBytes(MetadataExceptionStore);

        if (bytesValue is null)
        {
            return;
        }

        var exceptionString = Encoding.UTF8.GetString(bytesValue);

        var exception = JsonSerializer.Deserialize<Exception>(exceptionString, JsonSerializerOptions);

        if (exception is null)
        {
            return;
        }

        except.GetType().BaseType?
            .GetField("_innerException", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(except, exception);
    }
}
