namespace CapFinLoan.Auth.Application.Contracts.Responses;

public class PasswordResetResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}