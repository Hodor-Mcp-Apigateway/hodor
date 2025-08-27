namespace Papel.Integration.Application.Wallet.EventHandlers;

using Events.Transaction;

public sealed class TransactionCompletedEventHandler : INotificationHandler<TransactionCompletedEvent>
{
    private readonly ILogger<TransactionCompletedEventHandler> _logger;

    public TransactionCompletedEventHandler(ILogger<TransactionCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TransactionCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Transaction completed successfully: TxnId={TransactionId}, " +
            "SourceAccountId={SourceAccountId}, DestinationAccountId={DestinationAccountId}, " +
            "Amount={Amount}, OrderId={OrderId}",
            notification.TransactionId, notification.SourceAccountId,
            notification.DestinationAccountId, notification.Amount, notification.OrderId);

        // Transaction completion sonrası işlemler:
        // - SMS/Email notification
        // - Push notification
        // - Analytics event
        // - Cache invalidation

        return Task.CompletedTask;
    }
}
