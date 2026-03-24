using CapFinLoan.Document.Application.Contracts.Responses;
using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponse> UploadAsync(Guid userId, Guid applicationId, string documentType, IFormFile file, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentResponse>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DocumentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentResponse> VerifyAsync(Guid documentId, Guid adminUserId, bool isVerified, string? remarks, CancellationToken cancellationToken = default);
}
