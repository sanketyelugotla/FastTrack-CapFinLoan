using CapFinLoan.Document.Domain.Entities;

namespace CapFinLoan.Document.Application.Interfaces;

public interface IDocumentRepository
{
    Task<LoanDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoanDocument>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LoanDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(LoanDocument document, CancellationToken cancellationToken = default);
    Task UpdateAsync(LoanDocument document, CancellationToken cancellationToken = default);
}
