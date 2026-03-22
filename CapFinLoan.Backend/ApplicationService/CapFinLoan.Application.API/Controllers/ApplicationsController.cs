using System.Security.Claims;
using CapFinLoan.Application.Application.Contracts.Requests;
using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapFinLoan.Application.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _loanApplicationService;

    public ApplicationsController(ILoanApplicationService loanApplicationService)
    {
        _loanApplicationService = loanApplicationService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var applications = await _loanApplicationService.GetMineAsync(userId, cancellationToken);
        return Ok(applications);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _loanApplicationService.GetByIdAsync(id, GetUserId(), IsAdmin(), cancellationToken);
            return Ok(application);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> CreateDraft([FromBody] SaveLoanApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await _loanApplicationService.CreateDraftAsync(GetUserId(), request, cancellationToken);
        return Ok(application);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> UpdateDraft(Guid id, [FromBody] SaveLoanApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _loanApplicationService.UpdateDraftAsync(id, GetUserId(), false, request, cancellationToken);
            return Ok(application);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _loanApplicationService.SubmitAsync(id, GetUserId(), cancellationToken);
            return Ok(application);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _loanApplicationService.GetStatusAsync(id, GetUserId(), IsAdmin(), cancellationToken);
            return Ok(status);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User identifier claim is missing.");
    }

    private bool IsAdmin()
    {
        return User.IsInRole(RoleNames.Admin);
    }
}