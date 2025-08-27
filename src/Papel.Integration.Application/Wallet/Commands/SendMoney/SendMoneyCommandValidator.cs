namespace Papel.Integration.Application.Wallet.Commands.Create;

public sealed class SendMoneyCommandValidator : AbstractValidator<SendMoneyCommand>
{
    public SendMoneyCommandValidator() =>
        RuleFor(command => command.DestinationAccountId)
            .NotEmpty();
}
