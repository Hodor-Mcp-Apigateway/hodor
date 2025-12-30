namespace Papel.Integration.MessageBrokers.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Wolverine with Kafka transport for the host builder.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing settings to be used.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder UseKafkaMessageBroker(
        this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
        var consumerGroupId = configuration.GetValue<string>("Kafka:ConsumerGroupId") ?? "papel-integration";

        hostBuilder.UseWolverine(opts =>
        {
            opts.UseKafka(kafkaBootstrapServers)
                .ConfigureConsumers(consumer =>
                {
                    consumer.GroupId = consumerGroupId;
                    consumer.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
                });

            opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        return hostBuilder;
    }
}
