using CapFinLoan.Application.Domain.Entities;

namespace CapFinLoan.Application.Application.Interfaces;

public interface IApplicantProfileRepository
{
    Task<ApplicantProfile?> GetByApplicantUserIdAsync(Guid applicantUserId, CancellationToken cancellationToken = default);
    Task AddAsync(ApplicantProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicantProfile profile, CancellationToken cancellationToken = default);
}
