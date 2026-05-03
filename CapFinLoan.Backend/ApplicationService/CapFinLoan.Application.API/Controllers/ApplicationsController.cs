using System.Security.Claims;
using CapFinLoan.Application.Application.Contracts.Requests;
using CapFinLoan.Application.Application.Exceptions;
using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Application.Options;
using CapFinLoan.Application.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Application.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _loanApplicationService;
    private readonly IWalletService _walletService;
    private readonly WalletOptions _walletOptions;

    public ApplicationsController(ILoanApplicationService loanApplicationService, IWalletService walletService, IOptions<WalletOptions> walletOptions)
    {
        _loanApplicationService = loanApplicationService;
        _walletService = walletService;
        _walletOptions = walletOptions.Value;
    }

    [HttpGet("profile")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await _loanApplicationService.GetProfileAsync(GetUserId(), cancellationToken);
        return Ok(profile);
    }

    [HttpPut("profile")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> SaveProfile([FromBody] SaveApplicantProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _loanApplicationService.SaveProfileAsync(GetUserId(), request, cancellationToken);
        return Ok(profile);
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

    [HttpGet("wallet/summary")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> GetWalletSummary(CancellationToken cancellationToken)
    {
        var summary = await _walletService.GetApplicantWalletSummaryAsync(GetUserId(), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("wallet/ledger")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> GetWalletLedger([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var entries = await _walletService.GetApplicantWalletLedgerAsync(GetUserId(), take, cancellationToken);
        return Ok(entries);
    }

    [HttpPost("wallet/topup/create-order")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> CreateTopUpOrder([FromBody] CreateTopUpOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.CreateTopUpOrderAsync(GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("wallet/topup/verify")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> VerifyTopUp([FromBody] VerifyTopUpRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.VerifyTopUpAsync(GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("wallet/admin/summary")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetAdminWalletSummary(CancellationToken cancellationToken)
    {
        var summary = await _walletService.GetAdminWalletSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpPost("wallet/admin/topup/create-order")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateAdminTopUpOrder([FromBody] CreateTopUpOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.CreateAdminTopUpOrderAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("wallet/admin/topup/verify")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> VerifyAdminTopUp([FromBody] VerifyTopUpRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.VerifyAdminTopUpAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("wallet/config")]
    [Authorize(Roles = RoleNames.Applicant)]
    public IActionResult GetWalletConfig()
    {
        return Ok(new
        {
            applicationFee = _walletOptions.ApplicationFee,
            minTopUpAmount = _walletOptions.MinTopUpAmount,
            maxTopUpAmount = _walletOptions.MaxTopUpAmount,
            currency = _walletOptions.Currency
        });
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
            : throw new ApplicationForbiddenException("User identifier claim is missing.");
    }

    private bool IsAdmin()
    {
        return User.IsInRole(RoleNames.Admin);
    }
}