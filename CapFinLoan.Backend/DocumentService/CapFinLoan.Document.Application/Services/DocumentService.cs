using CapFinLoan.Document.Application.Contracts.Responses;
using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;
using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEventPublisher _eventPublisher;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public DocumentService(IDocumentRepository documentRepository, IFileStorageService fileStorageService, IEventPublisher eventPublisher)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<DocumentResponse> UploadAsync(Guid userId, Guid applicationId, string documentType, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("File is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException("File type is not supported. Allowed types: PDF, JPG, PNG.");

        await using var stream = file.OpenReadStream();
        var storedFileName = await _fileStorageService.SaveFileAsync(stream, file.FileName, cancellationToken);

        var document = new LoanDocument
        {
            ApplicationId = applicationId,
            UserId = userId,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            DocumentType = documentType,
            IsVerified = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document, cancellationToken);
        return MapToResponse(document);
    }

    public async Task<IReadOnlyCollection<DocumentResponse>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByApplicationIdAsync(applicationId, cancellationToken);
        return documents.Select(MapToResponse).ToArray();
    }

    public async Task<IReadOnlyCollection<DocumentResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId, cancellationToken);
        return documents.Select(MapToResponse).ToArray();
    }

    public async Task<DocumentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken)
                       ?? throw new KeyNotFoundException("Document not found.");
        return MapToResponse(document);
    }

    public async Task<DocumentResponse> VerifyAsync(Guid documentId, Guid adminUserId, bool isVerified, string? remarks, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken)
                       ?? throw new KeyNotFoundException("Document not found.");

        document.IsVerified = isVerified;
        document.VerifiedByUserId = adminUserId;
        document.VerifiedAtUtc = DateTime.UtcNow;
        document.Remarks = remarks;
        document.UpdatedAtUtc = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);

        await _eventPublisher.PublishAsync(new DocumentVerifiedEvent
        {
            DocumentId = document.Id,
            ApplicationId = document.ApplicationId,
            UserId = document.UserId,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            IsVerified = document.IsVerified,
            Remarks = document.Remarks,
            VerifiedByUserId = adminUserId,
            VerifiedAtUtc = document.VerifiedAtUtc ?? DateTime.UtcNow
        }, cancellationToken);

        return MapToResponse(document);
    }

    private static DocumentResponse MapToResponse(LoanDocument document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            ApplicationId = document.ApplicationId,
            UserId = document.UserId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            DocumentType = document.DocumentType,
            IsVerified = document.IsVerified,
            VerifiedByUserId = document.VerifiedByUserId,
            VerifiedAtUtc = document.VerifiedAtUtc,
            Remarks = document.Remarks,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc
        };
    }
}
