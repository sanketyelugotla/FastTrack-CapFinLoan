using CapFinLoan.Auth.Application.Contracts.Requests;
using CapFinLoan.Auth.Application.Contracts.Responses;

namespace CapFinLoan.Auth.Application.Interfaces;

public interface IAuthService
{
    // OTP-based signup flow
    Task<OtpSendResponse> SendSignupOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthResponse> VerifyOtpAndSignupAsync(OtpVerificationRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> VerifyOtpAndSignupAdminAsync(OtpVerificationRequest request, CancellationToken cancellationToken = default);

    // Original methods (kept for backward compatibility)
    Task<AuthResponse> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> SignupAdminAsync(SignupRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserSummaryResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserNotificationInfoResponse> GetUserNotificationInfoAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSummaryResponse> UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
}