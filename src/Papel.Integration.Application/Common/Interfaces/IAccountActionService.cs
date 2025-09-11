using Papel.Integration.Domain.Entities;

namespace Papel.Integration.Application.Common.Interfaces;

public interface IAccountActionService
{
    Task<AccountAction> AddAsync(AccountAction accountAction, CancellationToken cancellationToken = default);
}