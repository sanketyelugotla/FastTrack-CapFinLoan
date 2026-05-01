namespace CapFinLoan.Auth.Application.Contracts.Requests;

public class ResetPasswordWithOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}