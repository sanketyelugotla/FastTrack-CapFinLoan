namespace CapFinLoan.Auth.Domain.Entities;

public class EmailVerificationOtp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Check if OTP is still valid (not expired and not used)
    public bool IsValid => !IsUsed && DateTime.UtcNow <= ExpiresAtUtc;
}
