using System.Security.Claims;
using CapFinLoan.Wallet.Application.Contracts.Requests;
using CapFinLoan.Wallet.Application.Exceptions;
using CapFinLoan.Wallet.Application.Interfaces;
using CapFinLoan.Wallet.Application.Options;
using CapFinLoan.Wallet.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Wallet.API.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly WalletOptions _walletOptions;

    public WalletController(IWalletService walletService, IOptions<WalletOptions> walletOptions)
    {
        _walletService = walletService;
        _walletOptions = walletOptions.Value;
    }

    [HttpGet("summary")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> GetWalletSummary(CancellationToken cancellationToken)
    {
        var summary = await _walletService.GetApplicantWalletSummaryAsync(GetUserId(), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("ledger")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> GetWalletLedger([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var entries = await _walletService.GetApplicantWalletLedgerAsync(GetUserId(), take, cancellationToken);
        return Ok(entries);
    }

    [HttpPost("topup/create-order")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> CreateTopUpOrder([FromBody] CreateTopUpOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.CreateTopUpOrderAsync(GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("topup/verify")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> VerifyTopUp([FromBody] VerifyTopUpRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.VerifyTopUpAsync(GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("withdraw")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, CancellationToken cancellationToken)
    {
        var summary = await _walletService.WithdrawAsync(GetUserId(), request.Amount, request.Remarks, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("admin/summary")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetAdminWalletSummary(CancellationToken cancellationToken)
    {
        var summary = await _walletService.GetAdminWalletSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpPost("admin/topup/create-order")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateAdminTopUpOrder([FromBody] CreateTopUpOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.CreateAdminTopUpOrderAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("admin/topup/verify")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> VerifyAdminTopUp([FromBody] VerifyTopUpRequest request, CancellationToken cancellationToken)
    {
        var response = await _walletService.VerifyAdminTopUpAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("config")]
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

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new WalletServiceException("User identifier claim is missing.", 403, "FORBIDDEN");
    }

    private bool IsAdmin()
    {
        return User.IsInRole(RoleNames.Admin);
    }
}
