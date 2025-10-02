using Microsoft.Extensions.Logging;
using Papel.Integration.Domain.Entities;
using Papel.Integration.Domain.Enums;

namespace Papel.Integration.Infrastructure.Core.Services;

public class AccountActionService : IAccountActionService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AccountActionService> _logger;

    public AccountActionService(
        IApplicationDbContext context,
        ILogger<AccountActionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccountAction> AddAsync(AccountAction accountAction, CancellationToken cancellationToken = default)
    {
        try
        {
            accountAction.CreaDate = DateTime.UtcNow;
            accountAction.ModifDate = DateTime.UtcNow;
            accountAction.CreaUserId = (int)SYSTEM_USER_CODES.SystemUserId;
            accountAction.ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
            accountAction.StatusId = Status.Valid;

            await _context.AccountActions.AddAsync(accountAction);
            // SaveChangesAsync is not called here - will be handled by the calling transaction

            _logger.LogInformation(
                "AccountAction added to context. AccountId: {AccountId}, Type: {ActionType}, Amount: {Amount}",
                accountAction.AccountId, accountAction.AccountActionTypeId, accountAction.Amount);

            return accountAction;
        }
        catch (Exception accountActionCreationException)
        {
            _logger.LogError(accountActionCreationException, "Error adding AccountAction for AccountId: {AccountId}", accountAction.AccountId);
            throw;
        }
    }
}
