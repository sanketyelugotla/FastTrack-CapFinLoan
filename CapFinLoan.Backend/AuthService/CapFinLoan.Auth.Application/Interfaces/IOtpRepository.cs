using CapFinLoan.Auth.Domain.Entities;

namespace CapFinLoan.Auth.Application.Interfaces;

public interface IOtpRepository
{
    /// <summary>
    /// Generate and store a new OTP for email verification
    /// </summary>
    Task<EmailVerificationOtp> GenerateOtpAsync(string email, int expiryMinutes = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify if OTP is valid and mark as used
    /// </summary>
    Task<bool> VerifyOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest OTP for email (without marking as used)
    /// </summary>
    Task<EmailVerificationOtp?> GetLatestOtpAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete expired OTPs (cleanup)
    /// </summary>
    Task<int> DeleteExpiredOtpsAsync(CancellationToken cancellationToken = default);
}
