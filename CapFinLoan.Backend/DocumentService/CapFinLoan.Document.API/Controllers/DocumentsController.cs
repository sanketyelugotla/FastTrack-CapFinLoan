using System.Security.Claims;
using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Domain.Constants;
using CapFinLoan.Document.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapFinLoan.Document.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    // Upload a document for a loan application.
    [HttpPost("upload")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid applicationId,
        [FromForm] string documentType,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.UploadAsync(GetUserId(), applicationId, documentType, file, cancellationToken);
        return Ok(result);
    }

    // Get all documents for a specific application.
    [HttpGet("application/{applicationId:guid}")]
    public async Task<IActionResult> GetByApplicationId(Guid applicationId, CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetByApplicationIdAsync(applicationId, cancellationToken);
        return Ok(documents);
    }

    // Get all documents uploaded by the current user.
    [HttpGet("my")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetByUserIdAsync(GetUserId(), cancellationToken);
        return Ok(documents);
    }

    // Get a single document's metadata by ID.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetByIdAsync(id, cancellationToken);
        return Ok(document);
    }

    // Replace/edit a previously uploaded document.
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Replace(
        Guid id,
        IFormFile file,
        [FromForm] string? documentType,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.ReplaceAsync(GetUserId(), id, file, documentType, cancellationToken);
        return Ok(result);
    }

    // Link an already uploaded Document to a target Application without re-uploading file bytes.
    [HttpPost("{id:guid}/link")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Link(
        Guid id,
        [FromBody] CapFinLoan.Document.Application.Contracts.Requests.LinkDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.LinkAsync(GetUserId(), id, request.ApplicationId, cancellationToken);
        return Ok(result);
    }

    // Download/View the document contents.
    [HttpGet("{id:guid}/download")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var (stream, contentType, fileName) = await _documentService.DownloadAsync(id, GetUserId(), isAdmin: false, cancellationToken);
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
