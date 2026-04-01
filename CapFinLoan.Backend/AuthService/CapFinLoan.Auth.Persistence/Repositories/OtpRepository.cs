using CapFinLoan.Auth.Application.Interfaces;
using CapFinLoan.Auth.Domain.Entities;
using CapFinLoan.Auth.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Auth.Persistence.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly AuthDbContext _context;

    public OtpRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<EmailVerificationOtp> GenerateOtpAsync(string email, int expiryMinutes = 10, CancellationToken cancellationToken = default)
    {
        // Generate 6-digit random OTP
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        var otp = new EmailVerificationOtp
        {
            Email = email.ToLowerInvariant(),
            OtpCode = otpCode,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes),
            IsUsed = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.EmailVerificationOtps.Add(otp);
        await _context.SaveChangesAsync(cancellationToken);

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default)
    {
        var otp = await _context.EmailVerificationOtps
            .Where(x => x.Email == email.ToLowerInvariant() && x.OtpCode == otpCode && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null || !otp.IsValid)
        {
            return false;
        }

        // Mark as used
        otp.IsUsed = true;
        _context.EmailVerificationOtps.Update(otp);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<EmailVerificationOtp?> GetLatestOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.EmailVerificationOtps
            .Where(x => x.Email == email.ToLowerInvariant() && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredOtpsAsync(CancellationToken cancellationToken = default)
    {
        var expiredOtps = await _context.EmailVerificationOtps
            .Where(x => x.ExpiresAtUtc < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.EmailVerificationOtps.RemoveRange(expiredOtps);
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
