using CapFinLoan.Auth.Application.Contracts.Requests;
using CapFinLoan.Auth.Application.Contracts.Responses;
using CapFinLoan.Auth.Application.Interfaces;
using CapFinLoan.Auth.Domain.Constants;
using CapFinLoan.Auth.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;

namespace CapFinLoan.Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEventPublisher _eventPublisher;
    private readonly IOtpRepository _otpRepository;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IEventPublisher eventPublisher, IOtpRepository otpRepository)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _eventPublisher = eventPublisher;
        _otpRepository = otpRepository;
    }

    public async Task<OtpSendResponse> SendSignupOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("An account already exists with this email.");
        }

        // Generate OTP (10 minutes validity)
        var otp = await _otpRepository.GenerateOtpAsync(normalizedEmail, expiryMinutes: 10, cancellationToken);

        // Publish event to send OTP email
        await _eventPublisher.PublishAsync(new OtpSendEvent
        {
            Email = normalizedEmail,
            OtpCode = otp.OtpCode,
            ExpiresAtUtc = otp.ExpiresAtUtc,
            SentAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return new OtpSendResponse
        {
            Success = true,
            Message = "OTP sent to your email. Please verify within 10 minutes.",
            Email = normalizedEmail,
            ExpiryMinutes = 10
        };
    }

    public async Task<AuthResponse> VerifyOtpAndSignupAsync(OtpVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Verify OTP
        if (!await _otpRepository.VerifyOtpAsync(email, request.OtpCode.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("Invalid or expired OTP.");
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            Name = request.Name.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, request.Password, cancellationToken);
        await _userRepository.AddToRoleAsync(user, RoleNames.Applicant, cancellationToken);

        var userRoles = await _userRepository.GetRolesAsync(user, cancellationToken);
        var primaryRole = userRoles.FirstOrDefault() ?? RoleNames.Applicant;

        // Publish welcome email event
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.Name,
            Role = primaryRole,
            RegisteredAtUtc = user.CreatedAtUtc
        }, cancellationToken);

        var (token, expiresAtUtc) = await _jwtTokenGenerator.GenerateTokenAsync(user, userRoles);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Role = primaryRole,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email!
        };
    }

    public async Task<AuthResponse> VerifyOtpAndSignupAdminAsync(OtpVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Verify OTP
        if (!await _otpRepository.VerifyOtpAsync(email, request.OtpCode.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("Invalid or expired OTP.");
        }

        // Create admin user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            Name = request.Name.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, request.Password, cancellationToken);
        await _userRepository.AddToRoleAsync(user, RoleNames.Admin, cancellationToken);

        var userRoles = await _userRepository.GetRolesAsync(user, cancellationToken);
        var primaryRole = userRoles.FirstOrDefault() ?? RoleNames.Admin;

        // Publish welcome email event
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.Name,
            Role = primaryRole,
            RegisteredAtUtc = user.CreatedAtUtc
        }, cancellationToken);

        var (token, expiresAtUtc) = await _jwtTokenGenerator.GenerateTokenAsync(user, userRoles);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Role = primaryRole,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email!
        };
    }

    public async Task<AuthResponse> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("An account already exists with this email.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            Name = request.Name.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, request.Password, cancellationToken);

        // Assign user to Applicant role
        await _userRepository.AddToRoleAsync(user, RoleNames.Applicant, cancellationToken);

        // Get user roles for JWT token and event
        var userRoles = await _userRepository.GetRolesAsync(user, cancellationToken);
        var primaryRole = userRoles.FirstOrDefault() ?? RoleNames.Applicant;

        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.Name,
            Role = primaryRole,
            RegisteredAtUtc = user.CreatedAtUtc
        }, cancellationToken);

        var (token, expiresAtUtc) = await _jwtTokenGenerator.GenerateTokenAsync(user, userRoles);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Role = primaryRole,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email!
        };
    }

    public async Task<AuthResponse> SignupAdminAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("An account already exists with this email.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            Name = request.Name.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, request.Password, cancellationToken);

        // Assign user to Admin role
        await _userRepository.AddToRoleAsync(user, RoleNames.Admin, cancellationToken);

        // Get user roles for JWT token and event
        var userRoles = await _userRepository.GetRolesAsync(user, cancellationToken);
        var primaryRole = userRoles.FirstOrDefault() ?? RoleNames.Admin;

        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.Name,
            Role = primaryRole,
            RegisteredAtUtc = user.CreatedAtUtc
        }, cancellationToken);

        var (token, expiresAtUtc) = await _jwtTokenGenerator.GenerateTokenAsync(user, userRoles);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Role = primaryRole,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email!
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !await _userRepository.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User is deactivated.");
        }

        // Get user roles for JWT token
        var userRoles = await _userRepository.GetRolesAsync(user, cancellationToken);
        var primaryRole = userRoles.FirstOrDefault() ?? "UNKNOWN";

        var (token, expiresAtUtc) = await _jwtTokenGenerator.GenerateTokenAsync(user, userRoles);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Role = primaryRole,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email!
        };
    }

    public async Task<IReadOnlyCollection<UserSummaryResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var result = new List<UserSummaryResponse>();

        foreach (var user in users.OrderByDescending(x => x.CreatedAtUtc))
        {
            var roles = await _userRepository.GetRolesAsync(user, cancellationToken);
            var role = roles.FirstOrDefault() ?? "UNKNOWN";
            result.Add(MapUser(user, role));
        }

        return result;
    }

    public async Task<UserNotificationInfoResponse> GetUserNotificationInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException("User not found.");

        return new UserNotificationInfoResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty
        };
    }

    public async Task<UserSummaryResponse> UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException("User not found.");

        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var roles = await _userRepository.GetRolesAsync(user, cancellationToken);
        var role = roles.FirstOrDefault() ?? "UNKNOWN";
        return MapUser(user, role);
    }

    private static UserSummaryResponse MapUser(ApplicationUser user, string role)
    {
        return new UserSummaryResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            Role = role,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}