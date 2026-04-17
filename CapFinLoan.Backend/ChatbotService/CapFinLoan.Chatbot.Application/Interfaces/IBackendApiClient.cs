namespace CapFinLoan.Chatbot.Application.Interfaces;

/// <summary>
/// Executes HTTP calls against the other microservices on behalf of the authenticated user.
/// </summary>
public interface IBackendApiClient
{
    // ─── Application Service ──────────────────────────────────────────────────
    Task<string> GetMyApplicationsAsync(string bearerToken, CancellationToken ct = default);
    Task<string> GetApplicationByIdAsync(string applicationId, string bearerToken, CancellationToken ct = default);
    Task<string> GetApplicationStatusAsync(string applicationId, string bearerToken, CancellationToken ct = default);
    Task<string> GetProfileAsync(string bearerToken, CancellationToken ct = default);

    // ─── Document Service ─────────────────────────────────────────────────────
    Task<string> GetMyDocumentsAsync(string bearerToken, CancellationToken ct = default);
    Task<string> GetDocumentsByApplicationAsync(string applicationId, string bearerToken, CancellationToken ct = default);

    // ─── Admin Service ────────────────────────────────────────────────────────
    Task<string> GetAdminDashboardAsync(string bearerToken, CancellationToken ct = default);
    Task<string> GetAdminQueueAsync(string? status, string bearerToken, CancellationToken ct = default);
    Task<string> GetAdminApplicationByIdAsync(string applicationId, string bearerToken, CancellationToken ct = default);
    Task<string> UpdateApplicationStatusAsync(string applicationId, string targetStatus, string remarks, decimal? interestRate, decimal? sanctionAmount, string bearerToken, CancellationToken ct = default);
    Task<string> GetAdminDocumentsByApplicationAsync(string applicationId, string bearerToken, CancellationToken ct = default);
    Task<string> GetAllAdminDocumentsAsync(string? status, string bearerToken, CancellationToken ct = default);
    Task<string> VerifyDocumentAsync(string documentId, bool isVerified, string remarks, string bearerToken, CancellationToken ct = default);
    Task<string> GetUsersAsync(string bearerToken, CancellationToken ct = default);
}
