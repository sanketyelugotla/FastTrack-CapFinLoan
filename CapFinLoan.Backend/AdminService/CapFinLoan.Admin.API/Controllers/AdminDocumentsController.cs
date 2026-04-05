using CapFinLoan.Admin.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CapFinLoan.Admin.API.Controllers;

[ApiController]
[Route("api/admin/documents")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminDocumentsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminDocumentsController> _logger;

    public AdminDocumentsController(IHttpClientFactory httpClientFactory, ILogger<AdminDocumentsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateInternalClient()
    {
        var client = _httpClientFactory.CreateClient("DocumentServiceClient");
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    [HttpGet("application/{applicationId:guid}")]
    public async Task<IActionResult> GetByApplicationId(Guid applicationId, CancellationToken cancellationToken)
    {
        var client = CreateInternalClient();
        var response = await client.GetAsync($"/api/internal/documents/application/{applicationId}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Content(content, "application/json");
        }

        _logger.LogError("Failed to fetch documents from Document Service for application {ApplicationId}. Status: {StatusCode}", applicationId, response.StatusCode);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var client = CreateInternalClient();
        var query = string.IsNullOrWhiteSpace(status) ? "" : $"?status={status}";
        var response = await client.GetAsync($"/api/internal/documents/all{query}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Content(content, "application/json");
        }

        _logger.LogError("Failed to fetch all documents from Document Service. Status: {StatusCode}", response.StatusCode);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var client = CreateInternalClient();
        var response = await client.GetAsync($"/api/internal/documents/{id}/download", HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var contentDisposition = response.Content.Headers.ContentDisposition;
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName?.Trim('"') ?? "document";

            return File(stream, contentType, fileName);
        }

        _logger.LogError("Failed to download document {DocumentId} from Document Service. Status: {StatusCode}", id, response.StatusCode);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    [HttpPut("{id:guid}/verify")]
    public async Task<IActionResult> VerifyDocument(Guid id, [FromBody] object request, CancellationToken cancellationToken)
    {
        var client = CreateInternalClient();
        var response = await client.PutAsJsonAsync($"/api/internal/documents/{id}/verify", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Content(content, "application/json");
        }

        _logger.LogError("Failed to verify document in Document Service. Status: {StatusCode}", response.StatusCode);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
