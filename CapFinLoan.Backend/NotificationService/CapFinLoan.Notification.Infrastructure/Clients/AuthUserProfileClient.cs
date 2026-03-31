using System.Net.Http.Json;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Notification.Infrastructure.Clients;

public class AuthUserProfileClient : IUserProfileClient
{
    private readonly HttpClient _httpClient;
    private readonly NotificationDependencyOptions _options;
    private readonly ILogger<AuthUserProfileClient> _logger;

    public AuthUserProfileClient(HttpClient httpClient, IOptions<NotificationDependencyOptions> options, ILogger<AuthUserProfileClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/internal/users/{userId}/notification-info");

        if (!string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            request.Headers.Add("X-Internal-Api-Key", _options.InternalApiKey);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Auth user lookup failed for user {UserId} with status {StatusCode}", userId, response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserProfile>(cancellationToken: cancellationToken);
    }
}
