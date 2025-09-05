namespace Papel.Integration.Application.Wallet.Commands.Create;

using Common.Models;

public sealed record SendMoneyCommand : IRequest<Result<SendMoneyResponse>>
{
    public required long SourceAccountId { get; init; }
    public required long DestinationAccountId { get; init; }
    public required decimal Amount { get; init; }
    public required short CurrencyId { get; init; }
    public required short TenantId { get; init; }
    public string? Description { get; init; }
    public string? RemoteIpAddress { get; init; }
    
    // External işlemler için
    public string? ReferenceId { get; init; }
    public string? Tckn { get; init; }
    public bool IsExternal { get; init; }
}
