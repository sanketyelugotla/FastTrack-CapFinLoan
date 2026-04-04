using System.Security.Claims;
using CapFinLoan.Document.Application.Contracts.Requests;
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

    /// <summary>
    /// Admin: Verify or reject a document.
    /// </summary>
    [HttpPut("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _documentService.VerifyAsync(id, GetUserId(), request.IsVerified, request.Remarks, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Admin: Get all documents for a specific application.
    /// </summary>
    [HttpGet("application/{applicationId:guid}")]
    public async Task<IActionResult> GetByApplicationId(Guid applicationId, CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetByApplicationIdAsync(applicationId, cancellationToken);
        return Ok(documents);
    }

    /// <summary>
    /// Admin: Get all documents across all applications
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetAllAsync(status, cancellationToken);
        return Ok(documents);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User identifier claim is missing.");
    }
}
