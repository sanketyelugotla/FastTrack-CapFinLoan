using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Domain.Entities;
using CapFinLoan.Application.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Application.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WalletRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WalletAccount> GetOrCreateWalletAsync(Guid ownerUserId, string ownerType, string currency = "INR", CancellationToken cancellationToken = default)
    {
        var wallet = await _dbContext.WalletAccounts
            .FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.OwnerType == ownerType, cancellationToken);

        if (wallet is not null)
        {
            return wallet;
        }

        wallet = new WalletAccount
        {
            OwnerUserId = ownerUserId,
            OwnerType = ownerType,
            Currency = currency,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _dbContext.WalletAccounts.AddAsync(wallet, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task<WalletAccount?> GetWalletAsync(Guid ownerUserId, string ownerType, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WalletAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.OwnerType == ownerType, cancellationToken);
    }

    public async Task<PaymentOrder?> GetPaymentOrderByProviderOrderIdAsync(string providerOrderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentOrders
            .FirstOrDefaultAsync(x => x.ProviderOrderId == providerOrderId, cancellationToken);
    }

    public async Task<PaymentOrder> AddPaymentOrderAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken = default)
    {
        await _dbContext.PaymentOrders.AddAsync(paymentOrder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return paymentOrder;
    }

    public async Task UpdatePaymentOrderAsync(PaymentOrder paymentOrder, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(paymentOrder).State == EntityState.Detached)
        {
            _dbContext.PaymentOrders.Update(paymentOrder);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> LedgerEntryExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WalletLedgerEntries.AnyAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<WalletLedgerEntry> AddLedgerEntryAsync(WalletLedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.WalletLedgerEntries.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task<IReadOnlyCollection<WalletLedgerEntry>> GetRecentLedgerEntriesAsync(Guid walletAccountId, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WalletLedgerEntries
            .AsNoTracking()
            .Where(x => x.WalletAccountId == walletAccountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetBalanceAsync(Guid walletAccountId, CancellationToken cancellationToken = default)
    {
        var credits = await _dbContext.WalletLedgerEntries
            .Where(x => x.WalletAccountId == walletAccountId && x.Status == "Posted" && x.Direction == "Credit")
            .Select(x => (decimal?)x.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var debits = await _dbContext.WalletLedgerEntries
            .Where(x => x.WalletAccountId == walletAccountId && x.Status == "Posted" && x.Direction == "Debit")
            .Select(x => (decimal?)x.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        return credits - debits;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
