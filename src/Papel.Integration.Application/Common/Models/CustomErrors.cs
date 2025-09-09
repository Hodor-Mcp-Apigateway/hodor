namespace Papel.Integration.Application.Common.Models;

using FluentResults;

public class ConflictError : Error
{
    public ConflictError(string message) : base(message)
    {
        WithMetadata("StatusCode", 409);
    }
}

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
        WithMetadata("StatusCode", 404);
    }
}

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message)
    {
        WithMetadata("StatusCode", 401);
    }
}

public class ValidationError : Error
{
    public ValidationError(string message) : base(message)
    {
        WithMetadata("StatusCode", 400);
    }
}

public class InsufficientFundsError : Error
{
    public InsufficientFundsError(string message) : base(message)
    {
        WithMetadata("StatusCode", 402); // Payment Required
    }
}

public class BusinessRuleError : Error
{
    public BusinessRuleError(string message) : base(message)
    {
        WithMetadata("StatusCode", 422); // Unprocessable Entity
    }
}