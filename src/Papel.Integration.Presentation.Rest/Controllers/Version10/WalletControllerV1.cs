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
    [ProducesResponseType(typeof(ResultDtoBase<SendMoneyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ResultDtoBase<Unit>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendMoneyAsync(
        [FromBody] SendMoneyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

}
