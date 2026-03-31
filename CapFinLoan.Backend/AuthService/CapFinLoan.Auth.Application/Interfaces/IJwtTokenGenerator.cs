using CapFinLoan.Auth.Domain.Entities;

namespace CapFinLoan.Auth.Application.Interfaces;

public interface IJwtTokenGenerator
{
    Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(ApplicationUser user, IList<string> roles);
}