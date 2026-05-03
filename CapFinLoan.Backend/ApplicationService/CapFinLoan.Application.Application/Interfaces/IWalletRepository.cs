using CapFinLoan.Application.Domain.Entities;

namespace CapFinLoan.Application.Application.Interfaces;

public interface IWalletRepository
{
    Task<WalletAccount> GetOrCreateWalletAsync(Guid ownerUserId, string ownerType, string currency = "INR", CancellationToken cancellationToken = default);
    Task<WalletAccount?> GetWalletAsync(Guid ownerUserId, string ownerType, CancellationToken cancellationToken = default);
    Task<PaymentOrder?> GetPaymentOrderByProviderOrderIdAsync(string providerOrderId, CancellationToken cancellationToken = default);
    Task<PaymentOrder> AddPaymentOrderAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken = default);
    Task UpdatePaymentOrderAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken = default);
    Task<bool> LedgerEntryExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<WalletLedgerEntry> AddLedgerEntryAsync(WalletLedgerEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WalletLedgerEntry>> GetRecentLedgerEntriesAsync(Guid walletAccountId, int take, CancellationToken cancellationToken = default);
    Task<decimal> GetBalanceAsync(Guid walletAccountId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
