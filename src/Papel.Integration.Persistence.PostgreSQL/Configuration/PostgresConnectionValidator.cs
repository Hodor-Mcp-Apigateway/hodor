namespace Papel.Integration.Persistence.PostgreSQL.Configuration;

internal sealed class PostgresConnectionValidator : AbstractValidator<PostgresConnection>
{
    public PostgresConnectionValidator()
    {
        RuleFor(connection => connection.ConnectionString)
            .NotEmpty().WithMessage("PostgreSQL connection string is required")
            .Must(BeValidPostgresConnectionString).WithMessage("Invalid PostgreSQL connection string format");

        RuleFor(connection => connection.LoggingEnabled)
            .NotNull().WithMessage("LoggingEnabled must be specified");

        RuleFor(connection => connection.HealthCheckEnabled)
            .NotNull().WithMessage("HealthCheckEnabled must be specified");
    }

    private bool BeValidPostgresConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return false;

        return connectionString.Contains("Server=") &&
               connectionString.Contains("Database=") &&
               connectionString.Contains("userid=");
    }
}
