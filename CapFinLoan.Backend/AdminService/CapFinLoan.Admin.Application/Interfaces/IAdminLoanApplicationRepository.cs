using CapFinLoan.Admin.Domain.Entities;

namespace CapFinLoan.Admin.Application.Interfaces;

public interface IAdminLoanApplicationRepository
{
    Task<LoanApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoanApplication>> GetQueueAsync(string? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoanApplication>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(LoanApplication application, CancellationToken cancellationToken = default);
}