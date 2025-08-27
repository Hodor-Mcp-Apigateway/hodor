namespace Papel.Integration.Presentation.Rest.Controllers.Version10;

using Application.Common.Models;
using Application.Wallet.Commands.Create;

[ApiVersion(VersionController.VersionOne)]
[Route("api/v{version:apiVersion}/wallet")]
public class WalletControllerV1 : BaseController
{
    public WalletControllerV1(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [Route("sendmoney")]
    [ProducesResponseType(typeof(ResultDtoBase<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultDtoBase<SendMoneyResponse>>> SendMoneyAsync(
        [FromBody] SendMoneyCommand command,
        CancellationToken cancellationToken)
        => (await Mediator.Send(command, cancellationToken).ConfigureAwait(false))
            .ToResultDto();

}
