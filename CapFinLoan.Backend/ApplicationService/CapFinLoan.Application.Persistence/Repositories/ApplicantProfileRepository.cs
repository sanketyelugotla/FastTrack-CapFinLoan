using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Domain.Entities;
using CapFinLoan.Application.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Application.Persistence.Repositories;

public class ApplicantProfileRepository : IApplicantProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicantProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicantProfile?> GetByApplicantUserIdAsync(Guid applicantUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApplicantProfiles
            .FirstOrDefaultAsync(x => x.ApplicantUserId == applicantUserId, cancellationToken);
    }

    public async Task AddAsync(ApplicantProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.ApplicantProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ApplicantProfile profile, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(profile).State == EntityState.Detached)
        {
            _dbContext.ApplicantProfiles.Update(profile);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
