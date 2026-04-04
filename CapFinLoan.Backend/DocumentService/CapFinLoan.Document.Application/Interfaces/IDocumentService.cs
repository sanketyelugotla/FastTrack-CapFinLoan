using CapFinLoan.Document.Application.Contracts.Responses;
using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponse> UploadAsync(Guid userId, Guid applicationId, string documentType, IFormFile file, CancellationToken cancellationToken = default);
    Task<DocumentResponse> ReplaceAsync(Guid userId, Guid documentId, IFormFile file, string? documentType = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentResponse>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DocumentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentResponse> VerifyAsync(Guid documentId, Guid adminUserId, bool isVerified, string? remarks, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentResponse>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default);
}
