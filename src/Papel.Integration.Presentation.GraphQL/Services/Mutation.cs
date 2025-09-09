namespace Papel.Integration.Presentation.GraphQL.Services;

using Application.Common.Models;
using Application.Wallet.Commands.Create;

public sealed class Mutation
{
    [UseMutationConvention]
#pragma warning disable CA1822, MA0038
    public async Task<SendMoneyResponse> SendMoneyAsync([Service] IMediator mediator,
    long SourceCustomerId,
    string? Tckn ,
    decimal Amount,
    short CurrencyId ,
    short TenantId ,
    string? Description ,
    string? RemoteIpAddress ,
        CancellationToken token) =>
        (await mediator.Send(new SendMoneyCommand()
                             {
                                 SourceCustomerId = SourceCustomerId,
                                 Tckn =Tckn,
                                 Amount =Amount,
                                 CurrencyId = CurrencyId,
                                 TenantId = TenantId
                             }, token).ConfigureAwait(false)).Value;
#pragma warning restore CA1822, MA0038
}
