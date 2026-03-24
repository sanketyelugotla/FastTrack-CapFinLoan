using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Domain.Entities;
using CapFinLoan.Application.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Application.Persistence.Repositories;

public class LoanApplicationRepository : ILoanApplicationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LoanApplicationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LoanApplication application, CancellationToken cancellationToken = default)
    {
        await _dbContext.LoanApplications.AddAsync(application, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoanApplications
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanApplication>> GetByApplicantUserIdAsync(Guid applicantUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoanApplications
            .AsNoTracking()
            .Where(x => x.ApplicantUserId == applicantUserId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(LoanApplication application, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(application).State == EntityState.Detached)
        {
            _dbContext.LoanApplications.Update(application);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(LoanApplication application, CancellationToken cancellationToken = default)
    {
        _dbContext.LoanApplications.Remove(application);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}