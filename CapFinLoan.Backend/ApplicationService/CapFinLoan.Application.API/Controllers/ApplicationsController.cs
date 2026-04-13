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
        var application = await _loanApplicationService.GetByIdAsync(id, GetUserId(), IsAdmin(), cancellationToken);
        return Ok(application);
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
        var application = await _loanApplicationService.UpdateDraftAsync(id, GetUserId(), IsAdmin(), request, cancellationToken);
        return Ok(application);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var application = await _loanApplicationService.SubmitAsync(id, GetUserId(), IsAdmin(), cancellationToken);
        return Ok(application);
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        var status = await _loanApplicationService.GetStatusAsync(id, GetUserId(), IsAdmin(), cancellationToken);
        return Ok(status);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> DeleteDraft(Guid id, CancellationToken cancellationToken)
    {
        await _loanApplicationService.DeleteDraftAsync(id, GetUserId(), IsAdmin(), cancellationToken);
        return NoContent();
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