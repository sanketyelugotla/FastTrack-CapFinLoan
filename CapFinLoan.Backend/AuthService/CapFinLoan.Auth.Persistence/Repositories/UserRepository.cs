using CapFinLoan.Auth.Application.Interfaces;
using CapFinLoan.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CapFinLoan.Auth.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<IReadOnlyCollection<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(ApplicationUser user, string rawPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, rawPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    /// <summary>
    /// Add a user to a role using Identity roles (AspNetUserRoles table).
    /// </summary>
    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add user to role: {errors}");
        }
    }

    /// <summary>
    /// Get all roles for a user.
    /// </summary>
    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GetRolesAsync(user);
    }

    /// <summary>
    /// Add a claim to a user (stored in AspNetUserClaims table).
    /// </summary>
    public async Task AddClaimAsync(ApplicationUser user, Claim claim, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.AddClaimAsync(user, claim);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add claim to user: {errors}");
        }
    }

    /// <summary>
    /// Get all claims for a user.
    /// </summary>
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GetClaimsAsync(user);
    }
}