using CapFinLoan.Notification.Application.Models;

namespace CapFinLoan.Notification.Application.Interfaces;

public interface IUserProfileClient
{
    Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
