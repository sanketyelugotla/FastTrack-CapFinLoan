using CapFinLoan.Auth.Application.Contracts.Requests;
using CapFinLoan.Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapFinLoan.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SignupAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("signup-admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignupAdmin([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.SignupAdminAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("send-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp([FromQuery] string email, CancellationToken cancellationToken)
    {
        var result = await _authService.SendSignupOtpAsync(email, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify-otp-signup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtpAndSignup([FromBody] OtpVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyOtpAndSignupAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify-otp-signup-admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtpAndSignupAdmin([FromBody] OtpVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyOtpAndSignupAdminAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    [HttpPost("google-login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginWithGoogleAsync(request.IdToken, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }
}