using CapFinLoan.Wallet.Application.Contracts.Requests;
using CapFinLoan.Wallet.Application.Contracts.Responses;
using CapFinLoan.Wallet.Application.Exceptions;
using CapFinLoan.Wallet.Application.Interfaces;
using CapFinLoan.Wallet.Application.Options;
using CapFinLoan.Wallet.Domain.Constants;
using CapFinLoan.Wallet.Domain.Entities;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Wallet.Application.Services;

public class WalletService : IWalletService
{
    private static readonly Guid PlatformAdminWalletOwnerId = Guid.Empty;

    private readonly IWalletRepository _walletRepository;
    private readonly IRazorpayGateway _razorpayGateway;
    private readonly WalletOptions _walletOptions;

    public WalletService(IWalletRepository walletRepository, IRazorpayGateway razorpayGateway, IOptions<WalletOptions> walletOptions)
    {
        _walletRepository = walletRepository;
        _razorpayGateway = razorpayGateway;
        _walletOptions = walletOptions.Value;
    }

    public async Task<WalletSummaryResponse> GetApplicantWalletSummaryAsync(Guid applicantUserId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, _walletOptions.Currency, cancellationToken);
        return await BuildWalletSummaryAsync(wallet, 10, cancellationToken);
    }

    public async Task<WalletSummaryResponse> GetAdminWalletSummaryAsync(CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetOrCreateWalletAsync(PlatformAdminWalletOwnerId, WalletOwnerTypes.Admin, _walletOptions.Currency, cancellationToken);
        return await BuildWalletSummaryAsync(wallet, 20, cancellationToken);
    }

    public async Task<IReadOnlyCollection<WalletLedgerEntryResponse>> GetApplicantWalletLedgerAsync(Guid applicantUserId, int take = 20, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, _walletOptions.Currency, cancellationToken);
        var entries = await _walletRepository.GetRecentLedgerEntriesAsync(wallet.Id, Math.Clamp(take, 1, 100), cancellationToken);
        return entries.Select(MapEntry).ToArray();
    }

    public async Task<CreateTopUpOrderResponse> CreateTopUpOrderAsync(Guid applicantUserId, CreateTopUpOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount < _walletOptions.MinTopUpAmount || request.Amount > _walletOptions.MaxTopUpAmount)
        {
            throw new WalletValidationException($"Top-up amount should be between {_walletOptions.MinTopUpAmount} and {_walletOptions.MaxTopUpAmount}.");
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? _walletOptions.Currency : request.Currency.Trim().ToUpperInvariant();
        var wallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, currency, cancellationToken);
        var receipt = $"tu_{Guid.NewGuid():N}";
        var providerOrderId = await _razorpayGateway.CreateOrderAsync(request.Amount, currency, receipt, cancellationToken);

        await _walletRepository.AddPaymentOrderAsync(new PaymentOrder
        {
            ApplicantUserId = applicantUserId,
            WalletAccountId = wallet.Id,
            Provider = "Razorpay",
            ProviderOrderId = providerOrderId,
            Amount = request.Amount,
            Currency = currency,
            Status = "Created",
            IdempotencyKey = receipt,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);

        return new CreateTopUpOrderResponse
        {
            Provider = "Razorpay",
            KeyId = _razorpayGateway.GetPublicKey(),
            ProviderOrderId = providerOrderId,
            Amount = request.Amount,
            Currency = currency,
            ReferenceId = receipt,
        };
    }

    public async Task<VerifyTopUpResponse> VerifyTopUpAsync(Guid applicantUserId, VerifyTopUpRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _walletRepository.GetPaymentOrderByProviderOrderIdAsync(request.ProviderOrderId, cancellationToken)
            ?? throw new WalletNotFoundException("Payment order not found.");

        if (order.ApplicantUserId != applicantUserId)
        {
            throw new WalletForbiddenException("This payment order does not belong to the current user.");
        }

        if (string.Equals(order.Status, "Captured", StringComparison.OrdinalIgnoreCase))
        {
            var walletSummary = await GetApplicantWalletSummaryAsync(applicantUserId, cancellationToken);
            return new VerifyTopUpResponse
            {
                Success = true,
                Message = "Top-up already verified.",
                Wallet = walletSummary,
            };
        }

        if (!_razorpayGateway.VerifySignature(request.ProviderOrderId, request.ProviderPaymentId, request.ProviderSignature))
        {
            throw new WalletConflictException("Payment signature verification failed.");
        }

        var idempotencyKey = $"topup_capture_{request.ProviderOrderId}";
        if (!await _walletRepository.LedgerEntryExistsAsync(idempotencyKey, cancellationToken))
        {
            await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
            {
                WalletAccountId = order.WalletAccountId,
                Direction = WalletEntryDirections.Credit,
                EntryType = WalletEntryTypes.TopUp,
                Amount = order.Amount,
                Currency = order.Currency,
                Status = "Posted",
                ReferenceId = request.ProviderPaymentId,
                CorrelationId = request.ProviderOrderId,
                IdempotencyKey = idempotencyKey,
                Remarks = "Razorpay top-up captured.",
                CreatedAtUtc = DateTime.UtcNow,
            }, cancellationToken);
        }

        order.ProviderPaymentId = request.ProviderPaymentId;
        order.ProviderSignature = request.ProviderSignature;
        order.Status = "Captured";
        order.CapturedAtUtc = DateTime.UtcNow;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await _walletRepository.UpdatePaymentOrderAsync(order, cancellationToken);

        return new VerifyTopUpResponse
        {
            Success = true,
            Message = "Top-up verified successfully.",
            Wallet = await GetApplicantWalletSummaryAsync(applicantUserId, cancellationToken),
        };
    }

    public async Task<CreateTopUpOrderResponse> CreateAdminTopUpOrderAsync(CreateTopUpOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount < _walletOptions.MinTopUpAmount || request.Amount > _walletOptions.MaxTopUpAmount)
        {
            throw new WalletValidationException($"Top-up amount should be between {_walletOptions.MinTopUpAmount} and {_walletOptions.MaxTopUpAmount}.");
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? _walletOptions.Currency : request.Currency.Trim().ToUpperInvariant();
        var wallet = await _walletRepository.GetOrCreateWalletAsync(PlatformAdminWalletOwnerId, WalletOwnerTypes.Admin, currency, cancellationToken);
        var receipt = $"tu_{Guid.NewGuid():N}";
        var providerOrderId = await _razorpayGateway.CreateOrderAsync(request.Amount, currency, receipt, cancellationToken);

        await _walletRepository.AddPaymentOrderAsync(new PaymentOrder
        {
            ApplicantUserId = PlatformAdminWalletOwnerId,
            WalletAccountId = wallet.Id,
            Provider = "Razorpay",
            ProviderOrderId = providerOrderId,
            Amount = request.Amount,
            Currency = currency,
            Status = "Created",
            IdempotencyKey = receipt,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);

        return new CreateTopUpOrderResponse
        {
            Provider = "Razorpay",
            KeyId = _razorpayGateway.GetPublicKey(),
            ProviderOrderId = providerOrderId,
            Amount = request.Amount,
            Currency = currency,
            ReferenceId = receipt,
        };
    }

    public async Task<VerifyTopUpResponse> VerifyAdminTopUpAsync(VerifyTopUpRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _walletRepository.GetPaymentOrderByProviderOrderIdAsync(request.ProviderOrderId, cancellationToken)
            ?? throw new WalletNotFoundException("Payment order not found.");

        if (order.ApplicantUserId != PlatformAdminWalletOwnerId)
        {
            throw new WalletForbiddenException("This payment order does not belong to the admin wallet.");
        }

        if (string.Equals(order.Status, "Captured", StringComparison.OrdinalIgnoreCase))
        {
            var walletSummary = await GetAdminWalletSummaryAsync(cancellationToken);
            return new VerifyTopUpResponse
            {
                Success = true,
                Message = "Top-up already verified.",
                Wallet = walletSummary,
            };
        }

        if (!_razorpayGateway.VerifySignature(request.ProviderOrderId, request.ProviderPaymentId, request.ProviderSignature))
        {
            throw new WalletConflictException("Payment signature verification failed.");
        }

        var idempotencyKey = $"topup_capture_{request.ProviderOrderId}";
        if (!await _walletRepository.LedgerEntryExistsAsync(idempotencyKey, cancellationToken))
        {
            await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
            {
                WalletAccountId = order.WalletAccountId,
                Direction = WalletEntryDirections.Credit,
                EntryType = WalletEntryTypes.TopUp,
                Amount = order.Amount,
                Currency = order.Currency,
                Status = "Posted",
                ReferenceId = request.ProviderPaymentId,
                CorrelationId = request.ProviderOrderId,
                IdempotencyKey = idempotencyKey,
                Remarks = "Razorpay admin top-up captured.",
                CreatedAtUtc = DateTime.UtcNow,
            }, cancellationToken);
        }

        order.ProviderPaymentId = request.ProviderPaymentId;
        order.ProviderSignature = request.ProviderSignature;
        order.Status = "Captured";
        order.CapturedAtUtc = DateTime.UtcNow;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await _walletRepository.UpdatePaymentOrderAsync(order, cancellationToken);

        return new VerifyTopUpResponse
        {
            Success = true,
            Message = "Admin top-up verified successfully.",
            Wallet = await GetAdminWalletSummaryAsync(cancellationToken),
        };
    }

    public async Task DebitApplicationFeeAndCreditAdminAsync(Guid applicantUserId, Guid applicationId, decimal feeAmount, CancellationToken cancellationToken = default)
    {
        if (feeAmount <= 0)
        {
            throw new WalletValidationException("Application fee should be greater than zero.");
        }

        var applicantWallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, _walletOptions.Currency, cancellationToken);
        var adminWallet = await _walletRepository.GetOrCreateWalletAsync(PlatformAdminWalletOwnerId, WalletOwnerTypes.Admin, _walletOptions.Currency, cancellationToken);

        var currentBalance = await _walletRepository.GetBalanceAsync(applicantWallet.Id, cancellationToken);
        if (currentBalance < feeAmount)
        {
            throw new InsufficientWalletBalanceException(feeAmount, currentBalance);
        }

        var applicantDebitKey = $"app_fee_debit_{applicationId}";
        if (!await _walletRepository.LedgerEntryExistsAsync(applicantDebitKey, cancellationToken))
        {
            await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
            {
                WalletAccountId = applicantWallet.Id,
                Direction = WalletEntryDirections.Debit,
                EntryType = WalletEntryTypes.ApplicationFee,
                Amount = feeAmount,
                Currency = _walletOptions.Currency,
                Status = "Posted",
                ReferenceId = applicationId.ToString("N"),
                CorrelationId = applicationId.ToString("N"),
                IdempotencyKey = applicantDebitKey,
                Remarks = "Application submission fee debited.",
                CreatedAtUtc = DateTime.UtcNow,
            }, cancellationToken);
        }

        var adminCreditKey = $"app_fee_credit_admin_{applicationId}";
        if (!await _walletRepository.LedgerEntryExistsAsync(adminCreditKey, cancellationToken))
        {
            await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
            {
                WalletAccountId = adminWallet.Id,
                Direction = WalletEntryDirections.Credit,
                EntryType = WalletEntryTypes.ApplicationFee,
                Amount = feeAmount,
                Currency = _walletOptions.Currency,
                Status = "Posted",
                ReferenceId = applicationId.ToString("N"),
                CorrelationId = applicationId.ToString("N"),
                IdempotencyKey = adminCreditKey,
                Remarks = "Application submission fee credited from applicant wallet.",
                CreatedAtUtc = DateTime.UtcNow,
            }, cancellationToken);
        }
    }

    public async Task CreditLoanDisbursalAsync(Guid applicantUserId, Guid applicationId, decimal amount, string remarks, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return;
        }

        var applicantWallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, _walletOptions.Currency, cancellationToken);
        var adminWallet = await _walletRepository.GetOrCreateWalletAsync(PlatformAdminWalletOwnerId, WalletOwnerTypes.Admin, _walletOptions.Currency, cancellationToken);

        var adminBalance = await _walletRepository.GetBalanceAsync(adminWallet.Id, cancellationToken);
        if (adminBalance < amount)
        {
            throw new InsufficientWalletBalanceException(amount, adminBalance);
        }

        var disbursalKey = $"loan_disbursal_{applicationId}";
        if (await _walletRepository.LedgerEntryExistsAsync(disbursalKey, cancellationToken))
        {
            return;
        }

        // Debit Admin Wallet
        await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
        {
            WalletAccountId = adminWallet.Id,
            Direction = WalletEntryDirections.Debit,
            EntryType = WalletEntryTypes.LoanDisbursal,
            Amount = amount,
            Currency = _walletOptions.Currency,
            Status = "Posted",
            ReferenceId = applicationId.ToString("N"),
            CorrelationId = applicationId.ToString("N"),
            IdempotencyKey = $"admin_{disbursalKey}",
            Remarks = $"Loan disbursal dispatched to applicant {applicantUserId}.",
            CreatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);

        // Credit Applicant Wallet
        await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
        {
            WalletAccountId = applicantWallet.Id,
            Direction = WalletEntryDirections.Credit,
            EntryType = WalletEntryTypes.LoanDisbursal,
            Amount = amount,
            Currency = _walletOptions.Currency,
            Status = "Posted",
            ReferenceId = applicationId.ToString("N"),
            CorrelationId = applicationId.ToString("N"),
            IdempotencyKey = disbursalKey,
            Remarks = string.IsNullOrWhiteSpace(remarks) ? "Loan disbursal credited after approval." : remarks,
            CreatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);
    }

    public async Task RepayEmiAsync(Guid applicantUserId, Guid applicationId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new WalletValidationException("EMI repayment amount must be greater than zero.");
        }

        var applicantWallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, _walletOptions.Currency, cancellationToken);
        var adminWallet = await _walletRepository.GetOrCreateWalletAsync(PlatformAdminWalletOwnerId, WalletOwnerTypes.Admin, _walletOptions.Currency, cancellationToken);

        var applicantBalance = await _walletRepository.GetBalanceAsync(applicantWallet.Id, cancellationToken);
        if (applicantBalance < amount)
        {
            throw new WalletConflictException($"Insufficient wallet balance for EMI repayment. Required {amount}, available {applicantBalance}.");
        }

        var repaymentId = Guid.NewGuid().ToString("N");
        var applicantDebitKey = $"emi_repay_debit_{applicationId}_{repaymentId}";

        await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
        {
            WalletAccountId = applicantWallet.Id,
            Direction = WalletEntryDirections.Debit,
            EntryType = WalletEntryTypes.LoanRepayment,
            Amount = amount,
            Currency = _walletOptions.Currency,
            Status = "Posted",
            ReferenceId = applicationId.ToString("N"),
            CorrelationId = applicationId.ToString("N"),
            IdempotencyKey = applicantDebitKey,
            Remarks = "EMI repayment debited from wallet.",
            CreatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);

        var adminCreditKey = $"emi_repay_credit_{applicationId}_{repaymentId}";

        await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
        {
            WalletAccountId = adminWallet.Id,
            Direction = WalletEntryDirections.Credit,
            EntryType = WalletEntryTypes.LoanRepayment,
            Amount = amount,
            Currency = _walletOptions.Currency,
            Status = "Posted",
            ReferenceId = applicationId.ToString("N"),
            CorrelationId = applicationId.ToString("N"),
            IdempotencyKey = adminCreditKey,
            Remarks = $"EMI repayment received from applicant {applicantUserId}.",
            CreatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);
    }

    public async Task<WalletSummaryResponse> WithdrawAsync(Guid applicantUserId, decimal amount, string? remarks, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new WalletValidationException("Withdrawal amount must be greater than zero.");

        var wallet = await _walletRepository.GetOrCreateWalletAsync(applicantUserId, WalletOwnerTypes.Applicant, _walletOptions.Currency, cancellationToken);
        var balance = await _walletRepository.GetBalanceAsync(wallet.Id, cancellationToken);

        if (amount > balance)
            throw new WalletValidationException($"Insufficient balance. Available balance: {balance:N2}.");

        var idempotencyKey = $"withdraw_{applicantUserId:N}_{Guid.NewGuid():N}";
        await _walletRepository.AddLedgerEntryAsync(new WalletLedgerEntry
        {
            WalletAccountId = wallet.Id,
            Direction = WalletEntryDirections.Debit,
            EntryType = WalletEntryTypes.Withdrawal,
            Amount = amount,
            Currency = _walletOptions.Currency,
            Status = "Posted",
            ReferenceId = string.Empty,
            CorrelationId = idempotencyKey,
            IdempotencyKey = idempotencyKey,
            Remarks = remarks ?? "Wallet withdrawal by applicant.",
            CreatedAtUtc = DateTime.UtcNow,
        }, cancellationToken);

        return await BuildWalletSummaryAsync(wallet, 10, cancellationToken);
    }

    private async Task<WalletSummaryResponse> BuildWalletSummaryAsync(WalletAccount wallet, int recentCount, CancellationToken cancellationToken)
    {
        var balance = await _walletRepository.GetBalanceAsync(wallet.Id, cancellationToken);
        var recentEntries = await _walletRepository.GetRecentLedgerEntriesAsync(wallet.Id, recentCount, cancellationToken);

        return new WalletSummaryResponse
        {
            WalletAccountId = wallet.Id,
            OwnerUserId = wallet.OwnerUserId,
            OwnerType = wallet.OwnerType,
            Currency = wallet.Currency,
            Balance = balance,
            RecentEntries = recentEntries.Select(MapEntry).ToArray(),
        };
    }

    private static WalletLedgerEntryResponse MapEntry(WalletLedgerEntry entry)
    {
        return new WalletLedgerEntryResponse
        {
            Id = entry.Id,
            Direction = entry.Direction,
            EntryType = entry.EntryType,
            Amount = entry.Amount,
            Currency = entry.Currency,
            Status = entry.Status,
            ReferenceId = entry.ReferenceId,
            CorrelationId = entry.CorrelationId,
            Remarks = entry.Remarks,
            CreatedAtUtc = entry.CreatedAtUtc,
        };
    }
}
