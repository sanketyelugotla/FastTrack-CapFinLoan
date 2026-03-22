using CapFinLoan.Admin.Application.Interfaces;
using CapFinLoan.Admin.Domain.Constants;
using CapFinLoan.Admin.Domain.Entities;
using CapFinLoan.Admin.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Admin.Persistence.Repositories;

public class AdminLoanApplicationRepository : IAdminLoanApplicationRepository
{
    private readonly AdminDbContext _dbContext;

    public AdminLoanApplicationRepository(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoanApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoanApplications
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanApplication>> GetQueueAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LoanApplications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }
        else
        {
            query = query.Where(x => x.Status != ApplicationStatuses.Draft);
        }

        return await query
            .OrderByDescending(x => x.SubmittedAtUtc ?? x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanApplication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoanApplications
            .AsNoTracking()
            .Where(x => x.Status != ApplicationStatuses.Draft)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(LoanApplication application, CancellationToken cancellationToken = default)
    {
        _dbContext.LoanApplications.Update(application);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}