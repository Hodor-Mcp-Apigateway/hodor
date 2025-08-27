namespace Papel.Integration.MessageBrokers.RabbitMq.Consumers;

using Common.Extensions;
using Events.Transaction;

public sealed class TransactionCompletedIntegrationEventConsumer(ILogger<TransactionCompletedIntegrationEventConsumer> logger)
    : IConsumer<TransactionCompletedIntegrationEvent>
{
    public Task Consume(ConsumeContext<TransactionCompletedIntegrationEvent> context)
    {
        logger.ConsumeIntegrationEvent(context.Message.ToString());
        return Task.CompletedTask;
    }
}
