namespace Papel.Integration.Infrastructure.Core.Services;

using Application.Common.Interfaces;
using Papel.Integration.Application.Common.Exceptions;
using Domain.AggregatesModel.ToDoAggregates.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class LockService : ILockService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LockService> _logger;
    private readonly long _accountPrefix = 100000000L;

    private const int MaxRetryAttempts = 5;
    private const int InitialRetryDelayMs = 200;
    private const int LockTimeoutSeconds = 60;

    public LockService(IApplicationDbContext context, ILogger<LockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateOperationLockAsync(long customerId, string methodName, CancellationToken cancellationToken = default)
    {
        var systemDate = long.Parse(DateTime.Now.ToString("yyyyMMdd"));
        var retryCount = 0;
        var delayMs = InitialRetryDelayMs;

        while (retryCount < MaxRetryAttempts)
        {
            try
            {
                var existingLock = await _context.OperationLocks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(lockEntity => lockEntity.CustomerId == customerId
                        && lockEntity.MethodName == methodName
                        && lockEntity.SystemDate == systemDate,
                        cancellationToken);

                if (existingLock != null)
                {
                    var lockAge = DateTime.UtcNow - existingLock.CreatedAt;
                    if (lockAge.TotalSeconds > LockTimeoutSeconds)
                    {
                        var deleted = await TryRemoveStaleLockAsync(existingLock.Id, cancellationToken);
                        if (!deleted)
                        {
                            retryCount++;
                            await Task.Delay(delayMs, cancellationToken);
                            delayMs = Math.Min(delayMs * 2, 3200);
                            continue;
                        }

                        _logger.LogWarning(
                            "Removed stale lock for customer {CustomerId}, method {MethodName}. Lock age: {LockAge}s",
                            customerId, methodName, lockAge.TotalSeconds);
                    }
                    else
                    {
                        retryCount++;
                        if (retryCount >= MaxRetryAttempts)
                        {
                            throw new ConflictException("Bu işlem zaten başka bir kullanıcı tarafından gerçekleştiriliyor");
                        }

                        _logger.LogDebug(
                            "Lock exists for customer {CustomerId}, method {MethodName}. Retry {RetryCount}/{MaxRetries} after {DelayMs}ms",
                            customerId, methodName, retryCount, MaxRetryAttempts, delayMs);

                        await Task.Delay(delayMs, cancellationToken);
                        delayMs = Math.Min(delayMs * 2, 3200); // Exponential backoff, max 3.2s
                        continue;
                    }
                }

                // Create new lock
                var operationLock = new OperationLock
                {
                    CustomerId = customerId,
                    MethodName = methodName,
                    SystemDate = systemDate,
                    TenantId = 0,
                };

                _context.OperationLocks.Add(operationLock);
                await _context.SaveChangesAsync(cancellationToken);

                _context.AppDbContext.Entry(operationLock).State = EntityState.Detached;

                return;
            }
            catch (ConflictException)
            {
                throw;
            }
            catch (DbUpdateException) when (retryCount < MaxRetryAttempts - 1)
            {
                retryCount++;
                _logger.LogDebug(
                    "Race condition detected for customer {CustomerId}, method {MethodName}. Retry {RetryCount}/{MaxRetries}",
                    customerId, methodName, retryCount, MaxRetryAttempts);

                ClearTrackedLockEntities();

                await Task.Delay(delayMs, cancellationToken);
                delayMs = Math.Min(delayMs * 2, 3200);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error creating operation lock for customer {CustomerId}, method {MethodName}",
                    customerId, methodName);
                throw;
            }
        }

        throw new ConflictException("Bu işlem zaten başka bir kullanıcı tarafından gerçekleştiriliyor");
    }

    private async Task<bool> TryRemoveStaleLockAsync(long lockId, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _context.OperationLocks
                .Where(operationLock => operationLock.Id == lockId)
                .ExecuteDeleteAsync(cancellationToken);

            return deleted > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to remove stale lock {LockId}", lockId);
            return false;
        }
    }

    private void ClearTrackedLockEntities()
    {
        var trackedLocks = _context.AppDbContext.ChangeTracker.Entries<OperationLock>().ToList();
        foreach (var entry in trackedLocks)
        {
            entry.State = EntityState.Detached;
        }
    }

    public async Task RemoveOperationLockAsync(long customerId, string methodName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (customerId <= 0)
                return;

            var systemDate = long.Parse(DateTime.Now.ToString("yyyyMMdd"));
            var operationLock = await _context.OperationLocks
                .FirstOrDefaultAsync(lockEntity => lockEntity.CustomerId == customerId
                    && lockEntity.MethodName == methodName
                    && lockEntity.SystemDate == systemDate,
                    cancellationToken);

            if (operationLock != null)
            {
                _context.OperationLocks.Remove(operationLock);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error removing operation lock for customer {CustomerId}, method {MethodName}",
                customerId, methodName);
            // Don't throw on cleanup operations
        }
    }

    public async Task CreateAccountBalanceOperationLockAsync(long accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            await CreateOperationLockAsync(_accountPrefix + accountId, "BalanceOperations", cancellationToken);
        }
        catch (ConflictException)
        {
            throw new ConflictException("Bu hesap için bakiye işlemi zaten başka bir kullanıcı tarafından gerçekleştiriliyor");
        }
    }

    public async Task RemoveAccountBalanceOperationLockAsync(long accountId, CancellationToken cancellationToken = default)
    {
        await RemoveOperationLockAsync(_accountPrefix + accountId, "BalanceOperations", cancellationToken);
    }
}
