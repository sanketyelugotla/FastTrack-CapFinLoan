using CapFinLoan.Auth.Application.Contracts.Requests;
using CapFinLoan.Auth.Application.Interfaces;
using CapFinLoan.Auth.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapFinLoan.Auth.API.Controllers;

[ApiController]
[Route("api/internal/users")]
[Authorize(Roles = RoleNames.Admin)]
public class InternalUsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public InternalUsersController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _authService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}/notification-info")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationInfo(Guid id, CancellationToken cancellationToken)
    {
        var expectedKey = _configuration["InternalApi:Key"];
        if (!string.IsNullOrWhiteSpace(expectedKey))
        {
            var providedKey = Request.Headers["X-Internal-Api-Key"].ToString();
            if (!string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
            {
                return Unauthorized(new { message = "Invalid internal API key." });
            }
        }

        try
        {
            var user = await _authService.GetUserNotificationInfoAsync(id, cancellationToken);
            return Ok(user);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authService.UpdateUserStatusAsync(id, request.IsActive, cancellationToken);
            return Ok(user);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}