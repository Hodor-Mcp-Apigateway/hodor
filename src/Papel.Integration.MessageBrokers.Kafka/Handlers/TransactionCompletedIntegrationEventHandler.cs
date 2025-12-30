namespace Papel.Integration.MessageBrokers.Kafka.Handlers;

using Common.Extensions;
using Events.Transaction;

public sealed class TransactionCompletedIntegrationEventHandler(ILogger<TransactionCompletedIntegrationEventHandler> logger)
{
    public Task Handle(TransactionCompletedIntegrationEvent message)
    {
        logger.ConsumeIntegrationEvent(message.ToString());
        return Task.CompletedTask;
    }
}
