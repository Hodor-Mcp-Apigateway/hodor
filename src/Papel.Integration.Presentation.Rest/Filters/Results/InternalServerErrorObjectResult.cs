namespace Papel.Integration.Presentation.Rest.Filters.Results;

public sealed class InternalServerErrorObjectResult : ObjectResult
{
    public InternalServerErrorObjectResult(object? value) : base(value)
    {
    }
}
