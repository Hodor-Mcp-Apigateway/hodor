namespace Papel.Integration.Application.Common.Interfaces;

public interface ILockService
{
    Task CreateOperationLockAsync(long customerId, string methodName, CancellationToken cancellationToken = default);
    Task RemoveOperationLockAsync(long customerId, string methodName, CancellationToken cancellationToken = default);
    Task CreateAccountBalanceOperationLockAsync(long accountId, CancellationToken cancellationToken = default);
    Task RemoveAccountBalanceOperationLockAsync(long accountId, CancellationToken cancellationToken = default);
}