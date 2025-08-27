namespace Papel.Integration.Application.Wallet.EventHandlers;

using Events.Account;

public sealed class AccountBalanceUpdatedEventHandler : INotificationHandler<AccountBalanceUpdatedEvent>
{
    private readonly ILogger<AccountBalanceUpdatedEventHandler> _logger;

    public AccountBalanceUpdatedEventHandler(ILogger<AccountBalanceUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AccountBalanceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Account balance updated: AccountId={AccountId}, CustomerId={CustomerId}, " +
            "OldBalance={OldBalance}, NewBalance={NewBalance}, TransactionType={TransactionType}",
            notification.AccountId, notification.CustomerId, notification.OldBalance,
            notification.NewBalance, notification.TransactionType);

        // Burada isteğe bağlı olarak:
        // - Cache güncellemesi
        // - External service notification
        // - Audit logging
        // - RabbitMQ message publishing
        // gibi işlemler yapılabilir

        return Task.CompletedTask;
    }
}
