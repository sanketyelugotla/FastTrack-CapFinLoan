using CapFinLoan.Admin.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace CapFinLoan.Admin.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminUsersController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(IHttpClientFactory httpClientFactory, ILogger<AdminUsersController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateInternalClient()
    {
        var client = _httpClientFactory.CreateClient("AuthServiceClient");
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var client = CreateInternalClient();
        var response = await client.GetAsync("/api/internal/users", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Content(content, "application/json");
        }
        
        _logger.LogError("Failed to get users from Auth Service. Status: {StatusCode}", response.StatusCode);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] object request, CancellationToken cancellationToken)
    {
        var client = CreateInternalClient();
        var response = await client.PutAsJsonAsync($"/api/internal/users/{id}/status", request, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Content(content, "application/json");
        }
        
        _logger.LogError("Failed to update user status in Auth Service. Status: {StatusCode}", response.StatusCode);
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
