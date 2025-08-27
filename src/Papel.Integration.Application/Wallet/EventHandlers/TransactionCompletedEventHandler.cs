namespace Papel.Integration.Application.Wallet.EventHandlers;

using Events.Transaction;

public sealed class TransactionCompletedEventHandler : INotificationHandler<TransactionCompletedIntegrationEvent>
{
    private readonly ILogger<TransactionCompletedEventHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;
    public TransactionCompletedEventHandler(ILogger<TransactionCompletedEventHandler> logger, IPublishEndpoint publishEndpoint, IMapper mapper)
    {
        _mapper = mapper.ThrowIfNull();
        _logger = logger.ThrowIfNull();
        _publishEndpoint = publishEndpoint.ThrowIfNull();
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

        await _publishEndpoint.Publish(createEvent, cancellationToken)
            .ConfigureAwait(false);

    }
}
