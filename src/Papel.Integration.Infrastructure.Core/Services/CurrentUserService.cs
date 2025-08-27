namespace Papel.Integration.Infrastructure.Core.Services;

public sealed class CurrentUserService(ICurrentUser currentUser) : ICurrentUserService
{
    public ICurrentUser CurrentUser { get; } = currentUser.ThrowIfNull();
}
