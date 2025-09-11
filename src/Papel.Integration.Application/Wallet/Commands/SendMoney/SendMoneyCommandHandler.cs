namespace Papel.Integration.Application.Wallet.Commands.Create;

using Common.Interfaces;
using Common.Models;
using Domain.AggregatesModel.ToDoAggregates.Entities;
using Domain.Entities;
using Domain.Enums;
using Papel.Integration.Common.Extensions;

public sealed class SendMoneyCommandHandler : IRequestHandler<SendMoneyCommand, Result<SendMoneyResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SendMoneyCommandHandler> _logger;
    private readonly ILockService _lockService;

    public SendMoneyCommandHandler(
        IApplicationDbContext context,
        ILogger<SendMoneyCommandHandler> logger,
        ILockService lockService)
    {
        _context = context;
        _logger = logger;
        _lockService = lockService;
    }

    public async Task<Result<SendMoneyResponse>> Handle(
        SendMoneyCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        Customer? destinationCustomer = null;
        Account? sourceAccount = null;
        long? customerId = null;

        try
        {

            if (request.IsExternal)
            {
                // 1. Check if ReferenceId already exists
                var existingReference = await _context.ExternalReferences
                    .FirstOrDefaultAsync(externalRef => externalRef.ReferenceId == request.ReferenceId, cancellationToken);

                if (existingReference != null)
                {
                    _logger.LogStructured(LogLevel.Warning, "Integration:External:Order", request.ReferenceId!,
                        "PaymentProcessService", "Order", "Bu kayıt daha önce işlenmiştir");

                    return Result.Fail<SendMoneyResponse>(new ConflictError("Bu kayıt daha önce işlenmiştir"));
                }

                // 2. Find customer by TCKN with active wallet
                if (!long.TryParse(request.Tckn, out var tcknLong))
                {
                    _logger.LogStructured(LogLevel.Warning, "Integration:External:Order", request.ReferenceId!,
                        "PaymentProcessService", "Order", "Geçersiz TCKN formatı");

                    return Result.Fail<SendMoneyResponse>(new ValidationError("Geçersiz TCKN formatı"));
                }

                destinationCustomer = await _context.Customers
                    .Include(customer => customer.Accounts.Where(account => account.StatusId == Status.Valid))
                    .FirstOrDefaultAsync(customer => customer.Tckn == tcknLong && customer.TenantId == (short)Tenant.ConsumerWallet, cancellationToken);

                if (destinationCustomer == null || !destinationCustomer.Accounts.Any())
                {
                    _logger.LogStructured(LogLevel.Warning, "Integration:External:Order", request.ReferenceId!,
                        "PaymentProcessService", "Order", "Müşteri bulunamadı veya aktif cüzdanı yok");

                    return Result.Fail<SendMoneyResponse>(new NotFoundError("Müşteri bulunamadı veya aktif cüzdanı yok"));
                }

                customerId = destinationCustomer.CustomerId;

                // Create operation lock for external transactions
                await _lockService.CreateOperationLockAsync(customerId.Value, "ExternalSendMoney", cancellationToken);

                // 3. Insert ExternalReference record within transaction
                var externalReference = new ExternalReference
                {
                    ReferenceId = request.ReferenceId!,
                    CreaDate = DateTime.UtcNow,
                    ModifDate = DateTime.UtcNow,
                    TenantId = request.TenantId
                };

                _context.ExternalReferences.Add(externalReference);
            }

            sourceAccount = await _context.Accounts
                                .Where(account => account.CustomerId == request.SourceCustomerId)
                                .FirstOrDefaultAsync(cancellationToken);

            if (sourceAccount == null)
                return Result.Fail<SendMoneyResponse>(new NotFoundError("Source account not found"));

            // Create account balance lock
            await _lockService.CreateAccountBalanceOperationLockAsync(sourceAccount.AccountId, cancellationToken);

            // For external requests, destinationAccount is already found above
            // For internal requests, get it by DestinationAccountId
            var destinationAccount = destinationCustomer!.Accounts.FirstOrDefault(
                                             account => account.IsDefault);

            if (destinationAccount == null)
                return Result.Fail<SendMoneyResponse>(new NotFoundError("Destination account not found"));

            if (sourceAccount.Balance < request.Amount)
                return Result.Fail<SendMoneyResponse>(new InsufficientFundsError("Insufficient balance"));

            var oldSourceBalance = sourceAccount.Balance;
            var oldDestinationBalance = destinationAccount.Balance;

            sourceAccount.UpdateBalance(oldSourceBalance, oldSourceBalance - request.Amount, "MoneyTransfer-Debit");
            destinationAccount.UpdateBalance(oldDestinationBalance, oldDestinationBalance + request.Amount, "MoneyTransfer-Credit");

            var orderId = Guid.NewGuid().ToString();
            var firmReferenceNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var txn = new Txn
            {
                TxnStatusId = 1,
                TxnTypeId = (short)TXN_TYPE.TransferByCorporate,
                SourceAccountId = sourceAccount.AccountId,
                DestinationAccountId = destinationAccount.AccountId,
                Amount = request.Amount,
                CurrentBalance = oldSourceBalance,
                NewBalance = sourceAccount.Balance,
                Description = request.IsExternal ? $"External transfer - ReferenceId: {request.ReferenceId}" : request.Description ?? "",
                OrderId = orderId,
                FirmReferenceNumber = firmReferenceNumber,
                TenantId = sourceAccount.TenantId,
                CreaDate = DateTime.UtcNow,
                ModifDate = DateTime.UtcNow
            };

            txn.CompleteTransaction();
            _context.Txns.Add(txn);

            // LoadMoneyRequest sadece internal işlemler için gerekli
            var loadMoneyRequest = new LoadMoneyRequest
            {
                SourceAccountId = sourceAccount.AccountId,
               DestinationAccountId = destinationAccount.AccountId,
               Amount = request.Amount,
               CurrentBalance = oldSourceBalance,
               NewBalance = sourceAccount.Balance,
               CurrencyId = request.CurrencyId,
               FirmReferenceNumber = firmReferenceNumber,
               Description = request.Description ?? "",
               OrderId = orderId,
               TxnTypeId = (short)TXN_TYPE.TransferByCorporate,
               TenantId = destinationAccount.TenantId,
                CreaDate = DateTime.UtcNow,
                ModifDate = DateTime.UtcNow,
            };

            loadMoneyRequest.CompleteLoadMoneyRequest(sourceAccount.Balance);
            _context.LoadMoneyRequests.Add(loadMoneyRequest);

            await _context.SaveChangesAsync(cancellationToken);

            // Create AccountAction records for both source and destination accounts
            var sourceAccountAction = new AccountAction
            {
                AccountId = sourceAccount.AccountId,
                ReferenceId = txn.TxnId,
                Amount = request.Amount,
                BeforeAccountBalance = oldSourceBalance,
                AfterAccountBalance = sourceAccount.Balance,
                Description = request.IsExternal ? $"External transfer - ReferenceId: {request.ReferenceId}" : request.Description ?? "",
                AccountActionTypeId = (short)EnumAccountActionType.DecreaseBalance,
                TxnTypeId = (short)TXN_TYPE.TransferByCorporate,
                TargetFullName = destinationCustomer?.FirstName + " " + destinationCustomer?.LastName ?? "",
                TenantId = sourceAccount.TenantId,
                CreaDate = DateTime.UtcNow,
                ModifDate = DateTime.UtcNow,
                CreaUserId = (int)SYSTEM_USER_CODES.SystemUserId,
                ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId,
                StatusId = Status.Valid
            };

            var destinationAccountAction = new AccountAction
            {
                AccountId = destinationAccount.AccountId,
                ReferenceId = loadMoneyRequest.LoadMoneyRequestId,
                Amount = request.Amount,
                BeforeAccountBalance = oldDestinationBalance,
                AfterAccountBalance = destinationAccount.Balance,
                Description = request.IsExternal ? $"External transfer received - ReferenceId: {request.ReferenceId}" : request.Description ?? "",
                AccountActionTypeId = (short)EnumAccountActionType.IncreaseBalance,
                TxnTypeId = (short)TXN_TYPE.TransferByCorporate,
                TargetFullName = "", // Source customer name would go here if available
                TenantId = destinationAccount.TenantId,
                CreaDate = DateTime.UtcNow,
                ModifDate = DateTime.UtcNow,
                CreaUserId = (int)SYSTEM_USER_CODES.SystemUserId,
                ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId,
                StatusId = Status.Valid
            };

            _context.AccountActions.Add(sourceAccountAction);
            _context.AccountActions.Add(destinationAccountAction);

            // Update AvailableCashBalance for both accounts at the end
            sourceAccount.UpdateAvailableCashBalance(request.Amount);
            if (destinationAccount != sourceAccount) // Only update if different accounts
            {
                // Destination account receives money, so AvailableCashBalance increases
                destinationAccount.AvailableCashBalance = (destinationAccount.AvailableCashBalance ?? 0) + request.Amount;
                destinationAccount.ModifUserId = (int)SYSTEM_USER_CODES.ModifUserId;
                destinationAccount.ModifDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (request.IsExternal)
            {
                _logger.LogStructured(LogLevel.Information, "Integration:External:Order", request.ReferenceId!,
                    "PaymentProcessService", "Order", "Başarıyla işlendi");
            }
            else
            {
                _logger.LogInformation(
                    "Money transfer completed successfully. OrderId: {OrderId}, Amount: {Amount}",
                    orderId, request.Amount);
            }

            return Result.Ok(new SendMoneyResponse
            {
                TransactionId = txn.TxnId,
                OrderId = orderId,
                NewSourceBalance = sourceAccount.Balance,
                NewDestinationBalance = destinationAccount.Balance,
                IsSuccessful = true,
                ResultMessage = request.IsExternal ? "Başarılı" : "Transfer completed successfully",
                TransactionDate = DateTime.UtcNow
            });
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            // Clean up locks on error
            if (request.IsExternal && customerId.HasValue)
            {
                await _lockService.RemoveOperationLockAsync(customerId.Value, "ExternalSendMoney", cancellationToken);
            }

            if (sourceAccount != null)
            {
                await _lockService.RemoveAccountBalanceOperationLockAsync(sourceAccount.AccountId, cancellationToken);
            }

            if (request.IsExternal)
            {
                _logger.LogStructured(LogLevel.Error, "Integration:External:Order", request.ReferenceId!,
                    "PaymentProcessService", "Order", $"Hata oluştu: {exception.Message}");

                return Result.Fail<SendMoneyResponse>($"İşlem sırasında hata oluştu: {exception.Message}");
            }
            else
            {
                _logger.LogError(exception, "Error occurred while processing money transfer");
                return Result.Fail("An error occurred while processing the transfer");
            }
        }
        finally
        {
            // Clean up locks after successful completion
            if (request.IsExternal && customerId.HasValue)
            {
                await _lockService.RemoveOperationLockAsync(customerId.Value, "ExternalSendMoney", cancellationToken);
            }

            if (sourceAccount != null)
            {
                await _lockService.RemoveAccountBalanceOperationLockAsync(sourceAccount.AccountId, cancellationToken);
            }
        }
    }
}
