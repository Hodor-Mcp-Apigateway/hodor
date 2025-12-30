namespace Papel.Integration.MessageBrokers.Kafka.Extensions;

using OpenTelemetry.Trace;

public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Enables the incoming requests automatic data collection for Wolverine.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddWolverineOpenTelemetry(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSource("Wolverine");
    }
}
