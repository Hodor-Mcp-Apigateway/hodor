namespace Papel.Integration.Application.Wallet.Commands.Create;

public sealed class SendMoneyCommandValidator : AbstractValidator<SendMoneyCommand>
{
    public SendMoneyCommandValidator()
    {
        RuleFor(command => command.SourceAccountId)
            .GreaterThan(0)
            .WithMessage("SourceAccountId 0'dan büyük olmalıdır");

        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .WithMessage("Miktar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Miktar 1,000,000'den fazla olamaz");

        RuleFor(command => command.CurrencyId)
            .GreaterThan((short)0)
            .WithMessage("CurrencyId geçerli olmalıdır");

        RuleFor(command => command.TenantId)
            .GreaterThan((short)0)
            .WithMessage("TenantId geçerli olmalıdır");

        // External işlemler için özel validasyonlar
        When(command => command.IsExternal, () =>
        {
            RuleFor(command => command.ReferenceId)
                .NotEmpty()
                .WithMessage("External işlemler için ReferenceId boş olamaz")
                .MaximumLength(50)
                .WithMessage("ReferenceId en fazla 50 karakter olabilir");

            RuleFor(command => command.Tckn)
                .NotEmpty()
                .WithMessage("External işlemler için TCKN boş olamaz")
                .Length(11)
                .WithMessage("TCKN 11 karakter olmalıdır")
                .Matches(@"^\d{11}$")
                .WithMessage("TCKN sadece rakam içermelidir");
        });

        // Internal işlemler için DestinationAccountId kontrolü
        When(command => !command.IsExternal, () =>
        {
            RuleFor(command => command.DestinationAccountId)
                .GreaterThan(0)
                .WithMessage("Internal işlemler için DestinationAccountId geçerli olmalıdır");
        });
    }
}
