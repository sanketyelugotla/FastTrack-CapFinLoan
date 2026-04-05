using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Domain.Entities;
using CapFinLoan.Document.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using CapFinLoan.Document.Domain.Constants;

namespace CapFinLoan.Document.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _dbContext;

    public DocumentRepository(DocumentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoanDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanDocument>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .Where(d => d.ApplicationId == applicationId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanDocument>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Documents.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            if (Enum.TryParse<DocumentStatus>(status, true, out var documentStatus))
            {
                query = query.Where(d => d.Status == documentStatus);
            }
        }

        return await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LoanDocument document, CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LoanDocument document, CancellationToken cancellationToken = default)
    {
        _dbContext.Documents.Update(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
