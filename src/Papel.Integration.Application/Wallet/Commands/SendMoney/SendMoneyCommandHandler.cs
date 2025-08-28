namespace Papel.Integration.Application.Wallet.Commands.Create;

using Common.Interfaces;
using Common.Models;

public sealed class SendMoneyCommandHandler : IRequestHandler<SendMoneyCommand, Result<SendMoneyResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SendMoneyCommandHandler> _logger;

    public SendMoneyCommandHandler(
        IApplicationDbContext context,
        ILogger<SendMoneyCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SendMoneyResponse>> Handle(
        SendMoneyCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sourceAccount = await _context.Accounts
                .Where(account => account.AccountId == request.SourceAccountId && account.TenantId == request.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceAccount == null)
                return Result.Fail<SendMoneyResponse>("Source account not found");

            var destinationAccount = await _context.Accounts
                .Where(account => account.AccountId == request.DestinationAccountId)
                .FirstOrDefaultAsync(cancellationToken);

            if (destinationAccount == null)
                return Result.Fail<SendMoneyResponse>("Destination account not found");

            if (sourceAccount.Balance < request.Amount)
                return Result.Fail<SendMoneyResponse>("Insufficient balance");

            var oldSourceBalance = sourceAccount.Balance;
            var oldDestinationBalance = destinationAccount.Balance;

            sourceAccount.UpdateBalance(oldSourceBalance, oldSourceBalance - request.Amount, "MoneyTransfer-Debit");
            destinationAccount.UpdateBalance(oldDestinationBalance, oldDestinationBalance + request.Amount, "MoneyTransfer-Credit");

            var orderId = Guid.NewGuid().ToString();
            var firmReferenceNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var txn = new Txn
            {
                TxnStatusId = 1,
                TxnTypeId = 2,
                SourceAccountId = request.SourceAccountId,
                DestinationAccountId = request.DestinationAccountId,
                Amount = request.Amount,
                CurrentBalance = oldSourceBalance,
                NewBalance = sourceAccount.Balance,
                Description = request.Description??"",
                OrderId = orderId,
                FirmReferenceNumber = firmReferenceNumber,
                RemoteIpAddress = request.RemoteIpAddress??"",
                TenantId = request.TenantId,
                CreaDate = DateTime.Now,
                ModifDate = DateTime.Now
            };

            txn.CompleteTransaction();
            _context.Txns.Add(txn);

            var loadMoneyRequest = new LoadMoneyRequest
            {
                SourceAccountId = request.SourceAccountId,
                DestinationAccountId = request.DestinationAccountId,
                Amount = request.Amount,
                CurrentBalance = oldSourceBalance,
                NewBalance = sourceAccount.Balance,
                CurrencyId = request.CurrencyId,
                FirmReferenceNumber = firmReferenceNumber,
                Description = request.Description??"",
                OrderId = orderId,
                TxnTypeId = 2,
                RemoteIpAddress = request.RemoteIpAddress??"",
                TenantId = request.TenantId,
                CreaDate = DateTime.Now,
                ModifDate = DateTime.Now
            };

            loadMoneyRequest.CompleteLoadMoneyRequest(sourceAccount.Balance);
            _context.LoadMoneyRequests.Add(loadMoneyRequest);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Money transfer completed successfully. OrderId: {OrderId}, Amount: {Amount}",
                orderId, request.Amount);

            return Result.Ok(new SendMoneyResponse
            {
                TransactionId = txn.TxnId,
                OrderId = orderId,
                NewSourceBalance = sourceAccount.Balance,
                NewDestinationBalance = destinationAccount.Balance,
                IsSuccessful = true,
                ResultMessage = "Transfer completed successfully",
                TransactionDate = DateTime.Now
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while processing money transfer");
            return Result.Fail("An error occurred while processing the transfer");
        }
    }
}
