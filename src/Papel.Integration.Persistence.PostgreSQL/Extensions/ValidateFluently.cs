namespace Papel.Integration.Persistence.PostgreSQL.Extensions;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluently<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(provider =>
        {
            var validator = provider.GetService<IValidator<TOptions>>();
            return new FluentValidationOptions<TOptions>(optionsBuilder.Name, validator);
        });
        return optionsBuilder;
    }
}

public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    private readonly IValidator<TOptions>? _validator;

    public FluentValidationOptions(string? name, IValidator<TOptions>? validator)
    {
        Name = name;
        _validator = validator;
    }

    public string? Name { get; }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_validator == null)
            return ValidateOptionsResult.Skip;

        var result = _validator.Validate(options);
        if (result.IsValid)
            return ValidateOptionsResult.Success;

        var errors = result.Errors.Select(error => error.ErrorMessage);
        return ValidateOptionsResult.Fail(errors);
    }
}
