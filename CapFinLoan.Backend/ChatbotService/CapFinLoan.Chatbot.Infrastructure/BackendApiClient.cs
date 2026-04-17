using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CapFinLoan.Chatbot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Chatbot.Infrastructure;

/// <summary>
/// Calls the existing microservices (Application, Admin, Document) on behalf of the authenticated user.
/// The user's JWT token is forwarded so that the downstream services enforce authorization.
/// </summary>
public class BackendApiClient : IBackendApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BackendApiClient> _logger;
    private readonly string _applicationServiceUrl;
    private readonly string _adminServiceUrl;
    private readonly string _documentServiceUrl;

    public BackendApiClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<BackendApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _applicationServiceUrl = configuration["BackendServices:ApplicationServiceUrl"] ?? "http://localhost:5022";
        _adminServiceUrl = configuration["BackendServices:AdminServiceUrl"] ?? "http://localhost:5024";
        _documentServiceUrl = configuration["BackendServices:DocumentServiceUrl"] ?? "http://localhost:5023";
    }

    // ─── Application Service ──────────────────────────────────────────────────

    public Task<string> GetMyApplicationsAsync(string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_applicationServiceUrl}/api/applications/my", bearerToken, ct);

    public Task<string> GetApplicationByIdAsync(string applicationId, string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_applicationServiceUrl}/api/applications/{applicationId}", bearerToken, ct);

    public Task<string> GetApplicationStatusAsync(string applicationId, string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_applicationServiceUrl}/api/applications/{applicationId}/status", bearerToken, ct);

    public Task<string> GetProfileAsync(string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_applicationServiceUrl}/api/applications/profile", bearerToken, ct);

    // ─── Document Service ─────────────────────────────────────────────────────

    public Task<string> GetMyDocumentsAsync(string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_documentServiceUrl}/api/documents/my", bearerToken, ct);

    public Task<string> GetDocumentsByApplicationAsync(string applicationId, string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_documentServiceUrl}/api/documents/application/{applicationId}", bearerToken, ct);

    // ─── Admin Service ────────────────────────────────────────────────────────

    public Task<string> GetAdminDashboardAsync(string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_adminServiceUrl}/api/admin/applications/dashboard", bearerToken, ct);

    public Task<string> GetAdminQueueAsync(string? status, string bearerToken, CancellationToken ct = default)
    {
        var query = string.IsNullOrWhiteSpace(status) ? "" : $"?status={status}";
        return GetAsync($"{_adminServiceUrl}/api/admin/applications{query}", bearerToken, ct);
    }

    public Task<string> GetAdminApplicationByIdAsync(string applicationId, string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_adminServiceUrl}/api/admin/applications/{applicationId}", bearerToken, ct);

    public async Task<string> UpdateApplicationStatusAsync(string applicationId, string targetStatus, string remarks,
        decimal? interestRate, decimal? sanctionAmount, string bearerToken, CancellationToken ct = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["targetStatus"] = targetStatus,
            ["remarks"] = remarks
        };

        if (interestRate.HasValue) body["interestRate"] = interestRate.Value;
        if (sanctionAmount.HasValue) body["sanctionAmount"] = sanctionAmount.Value;

        return await PutAsync($"{_adminServiceUrl}/api/admin/applications/{applicationId}/status", body, bearerToken, ct);
    }

    public Task<string> GetAdminDocumentsByApplicationAsync(string applicationId, string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_adminServiceUrl}/api/admin/documents/application/{applicationId}", bearerToken, ct);

    public Task<string> GetAllAdminDocumentsAsync(string? status, string bearerToken, CancellationToken ct = default)
    {
        var query = string.IsNullOrWhiteSpace(status) ? "" : $"?status={status}";
        return GetAsync($"{_adminServiceUrl}/api/admin/documents{query}", bearerToken, ct);
    }

    public Task<string> VerifyDocumentAsync(string documentId, bool isVerified, string remarks, string bearerToken, CancellationToken ct = default)
    {
        var body = new { isVerified, remarks };
        return PutAsync($"{_adminServiceUrl}/api/admin/documents/{documentId}/verify", body, bearerToken, ct);
    }

    public Task<string> GetUsersAsync(string bearerToken, CancellationToken ct = default)
        => GetAsync($"{_adminServiceUrl}/api/admin/users", bearerToken, ct);

    // ─── HTTP helpers ─────────────────────────────────────────────────────────

    private async Task<string> GetAsync(string url, string bearerToken, CancellationToken ct)
    {
        var client = CreateClient(bearerToken);
        _logger.LogDebug("GET {Url}", url);

        var response = await client.GetAsync(url, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Backend API error GET {Url}: {StatusCode} — {Body}", url, response.StatusCode, content);
            return JsonSerializer.Serialize(new { error = $"Service returned {(int)response.StatusCode}", details = TryExtractMessage(content) });
        }

        return content;
    }

    private async Task<string> PutAsync(string url, object body, string bearerToken, CancellationToken ct)
    {
        var client = CreateClient(bearerToken);
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("PUT {Url}: {Body}", url, json);

        var response = await client.PutAsync(url, httpContent, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Backend API error PUT {Url}: {StatusCode} — {Body}", url, response.StatusCode, content);
            return JsonSerializer.Serialize(new { error = $"Service returned {(int)response.StatusCode}", details = TryExtractMessage(content) });
        }

        return content;
    }

    private HttpClient CreateClient(string bearerToken)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    private static string TryExtractMessage(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? responseBody;
        }
        catch { }
        return responseBody;
    }
}
