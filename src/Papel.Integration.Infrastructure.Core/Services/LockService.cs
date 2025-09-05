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

    public LockService(IApplicationDbContext context, ILogger<LockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateOperationLockAsync(long customerId, string methodName, CancellationToken cancellationToken = default)
    {
        try
        {
            var systemDate = long.Parse(DateTime.Now.ToString("yyyyMMdd"));
            var operationLock = new OperationLock
            {
                CustomerId = customerId,
                MethodName = methodName,
                SystemDate = systemDate,
                TenantId = 0 // Default tenant ID
            };

            _context.OperationLocks.Add(operationLock);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException?.Message?.Contains("23505", StringComparison.OrdinalIgnoreCase) == true || 
                                                exception.InnerException?.Message?.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
                                                exception.InnerException?.Message?.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Unique constraint violation - operation already in progress
            throw new ConflictException("Bu işlem zaten başka bir kullanıcı tarafından gerçekleştiriliyor");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating operation lock for customer {CustomerId}, method {MethodName}", 
                customerId, methodName);
            throw;
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