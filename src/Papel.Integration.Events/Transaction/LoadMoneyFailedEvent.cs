namespace Papel.Integration.Events.Transaction;



public sealed record LoadMoneyFailedEvent(
    long LoadMoneyRequestId,
    long SourceAccountId,
    decimal Amount,
    string ErrorMessage,
    string OrderId) : IIntegrationEvent;
