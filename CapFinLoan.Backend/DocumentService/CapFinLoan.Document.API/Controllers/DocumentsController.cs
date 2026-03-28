using System.Security.Claims;
using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Domain.Constants;
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

    /// <summary>
    /// Upload a document for a loan application.
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Upload(
        [FromForm] Guid applicationId,
        [FromForm] string documentType,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _documentService.UploadAsync(GetUserId(), applicationId, documentType, file, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Get all documents for a specific application.
    /// </summary>
    [HttpGet("application/{applicationId:guid}")]
    public async Task<IActionResult> GetByApplicationId(Guid applicationId, CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetByApplicationIdAsync(applicationId, cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Get all documents uploaded by the current user.
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetByUserIdAsync(GetUserId(), cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Get a single document's metadata by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.GetByIdAsync(id, cancellationToken);
            return Ok(document);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Replace/edit a previously uploaded document.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = RoleNames.Applicant)]
    public async Task<IActionResult> Replace(
        Guid id,
        IFormFile file,
        [FromForm] string? documentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _documentService.ReplaceAsync(GetUserId(), id, file, documentType, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User identifier claim is missing.");
    }
}
