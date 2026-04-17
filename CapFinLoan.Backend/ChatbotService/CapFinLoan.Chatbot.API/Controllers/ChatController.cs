using System.Security.Claims;
using CapFinLoan.Chatbot.Application.Contracts.Requests;
using CapFinLoan.Chatbot.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapFinLoan.Chatbot.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatOrchestrationService _chatService;

    public ChatController(IChatOrchestrationService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        var bearerToken = GetBearerToken();

        var response = await _chatService.ProcessMessageAsync(request, userRole, userId, bearerToken, cancellationToken);
        return Ok(response);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ChatbotService", timestampUtc = DateTime.UtcNow });
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User identifier claim is missing.");
    }

    private string GetUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "APPLICANT";
    }

    private string GetBearerToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : string.Empty;
    }
}
