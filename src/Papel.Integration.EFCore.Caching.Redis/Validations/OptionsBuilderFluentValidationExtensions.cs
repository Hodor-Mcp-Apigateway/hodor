namespace Papel.Integration.EFCore.Caching.Redis.Validations;

using FluentValidation;
using Microsoft.Extensions.Options;

public static class OptionsBuilderFluentValidationExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
            provider => new FluentValidationOptions<TOptions>(
                optionsBuilder.Name,
                provider.GetService<IValidator<TOptions>>()));
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
        if (Name != null && Name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        if (_validator == null)
        {
            return ValidateOptionsResult.Skip;
        }

        var validationResult = _validator.Validate(options);
        if (validationResult.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = validationResult.Errors
            .Select(error => $"Options validation failed for '{error.PropertyName}' with error: '{error.ErrorMessage}'.")
            .ToArray();

        return ValidateOptionsResult.Fail(errors);
    }
}
