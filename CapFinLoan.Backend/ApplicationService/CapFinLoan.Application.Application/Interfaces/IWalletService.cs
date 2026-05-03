using CapFinLoan.Application.Application.Contracts.Requests;
using CapFinLoan.Application.Application.Contracts.Responses;

namespace CapFinLoan.Application.Application.Interfaces;

public interface IWalletService
{
    Task<WalletSummaryResponse> GetApplicantWalletSummaryAsync(Guid applicantUserId, CancellationToken cancellationToken = default);
    Task<WalletSummaryResponse> GetAdminWalletSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WalletLedgerEntryResponse>> GetApplicantWalletLedgerAsync(Guid applicantUserId, int take = 20, CancellationToken cancellationToken = default);
    Task<CreateTopUpOrderResponse> CreateTopUpOrderAsync(Guid applicantUserId, CreateTopUpOrderRequest request, CancellationToken cancellationToken = default);
    Task<VerifyTopUpResponse> VerifyTopUpAsync(Guid applicantUserId, VerifyTopUpRequest request, CancellationToken cancellationToken = default);
    Task<CreateTopUpOrderResponse> CreateAdminTopUpOrderAsync(CreateTopUpOrderRequest request, CancellationToken cancellationToken = default);
    Task<VerifyTopUpResponse> VerifyAdminTopUpAsync(VerifyTopUpRequest request, CancellationToken cancellationToken = default);
    Task DebitApplicationFeeAndCreditAdminAsync(Guid applicantUserId, Guid applicationId, decimal feeAmount, CancellationToken cancellationToken = default);
    Task CreditLoanDisbursalAsync(Guid applicantUserId, Guid applicationId, decimal amount, string remarks, CancellationToken cancellationToken = default);
    Task RepayEmiAsync(Guid applicantUserId, Guid applicationId, decimal amount, CancellationToken cancellationToken = default);
}
