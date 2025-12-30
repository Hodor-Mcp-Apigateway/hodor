//#if (EnableKafka)
namespace Papel.Integration.Application.Wallet.EventHandlers;

using Events.Transaction;

public sealed class TransactionCompletedEventHandler : INotificationHandler<TransactionCompletedIntegrationEvent>
{
    private readonly ILogger<TransactionCompletedEventHandler> _logger;
    private readonly IMessageBus _messageBus;
    private readonly IMapper _mapper;
    public TransactionCompletedEventHandler(ILogger<TransactionCompletedEventHandler> logger, IMessageBus messageBus, IMapper mapper)
    {
        _mapper = mapper.ThrowIfNull();
        _logger = logger.ThrowIfNull();
        _messageBus = messageBus.ThrowIfNull();
    }

    public async Task Handle(TransactionCompletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Transaction completed successfully: TxnId={TransactionId}, " +
            "SourceAccountId={SourceAccountId}, DestinationAccountId={DestinationAccountId}, " +
            "Amount={Amount}, OrderId={OrderId}",
            notification.TransactionId, notification.SourceAccountId,
            notification.DestinationAccountId, notification.Amount, notification.OrderId);

        var createEvent = await notification
                      .BuildAdapter(_mapper.Config)
                      .AdaptToTypeAsync<TransactionCompletedIntegrationEvent>()
                      .ConfigureAwait(false);

        await _messageBus.PublishAsync(createEvent)
            .ConfigureAwait(false);

    }
}
//#endif
