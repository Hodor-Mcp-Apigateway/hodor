namespace Papel.Integration.Presentation.Rest.Controllers;

using Application.Common.Models;
using FluentResults;
using Presentation.Rest.Models.Result;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected BaseController(IMediator mediator) => Mediator = mediator.ThrowIfNull();

    protected IMediator Mediator { get; }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        var dto = result.ToResultDto();

        if (result.IsSuccess)
            return Ok(dto);

        // Error metadata'dan status code al
        var firstError = result.Errors.FirstOrDefault();
        if (firstError?.Metadata?.TryGetValue("StatusCode", out var statusCode) == true && statusCode is int code)
            return StatusCode(code, dto);

        // Error type'a göre status code belirle
        return firstError switch
        {
            ValidationError => BadRequest(dto),
            NotFoundError => NotFound(dto),
            UnauthorizedError => Unauthorized(dto),
            ConflictError => Conflict(dto),
            InsufficientFundsError => StatusCode(402, dto), // Payment Required
            BusinessRuleError => StatusCode(422, dto), // Unprocessable Entity
            _ => BadRequest(dto)
        };
    }
}
