using System.Security.Claims;
using CapFinLoan.Document.Application.Contracts.Requests;
using CapFinLoan.Document.Application.Exceptions;
using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapFinLoan.Document.API.Controllers;

[ApiController]
[Route("api/internal/documents")]
[Authorize(Roles = RoleNames.Admin)]
public class InternalDocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public InternalDocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    // Admin: Verify or reject a document.
    [HttpPut("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _documentService.VerifyAsync(id, GetUserId(), request.IsVerified, request.Remarks, cancellationToken);
        return Ok(result);
    }

    // Admin: Get all documents for a specific application.
    [HttpGet("application/{applicationId:guid}")]
    public async Task<IActionResult> GetByApplicationId(Guid applicationId, CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetByApplicationIdAsync(applicationId, cancellationToken);
        return Ok(documents);
    }

    // Admin: Get all documents across all applications
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetAllAsync(status, cancellationToken);
        return Ok(documents);
    }

    // Admin: Download/View the document contents.
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var (stream, contentType, fileName) = await _documentService.DownloadAsync(id, null, isAdmin: true, cancellationToken);
        return File(stream, contentType, fileName);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new DocumentForbiddenException("User identifier claim is missing.");
    }
}
