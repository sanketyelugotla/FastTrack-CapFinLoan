using CapFinLoan.Document.Application.Contracts.Responses;
using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Domain.Constants;
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
            Status = DocumentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document, cancellationToken);
        return MapToResponse(document);
    }

    public async Task<DocumentResponse> ReplaceAsync(Guid userId, Guid documentId, IFormFile file, string? documentType = null, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("File is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException("File type is not supported. Allowed types: PDF, JPG, PNG.");

        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken)
                       ?? throw new KeyNotFoundException("Document not found.");

        if (document.UserId != userId)
            throw new UnauthorizedAccessException("You are not allowed to edit this document.");

        await using var stream = file.OpenReadStream();
        var storedFileName = await _fileStorageService.SaveFileAsync(stream, file.FileName, cancellationToken);

        document.FileName = file.FileName;
        document.StoredFileName = storedFileName;
        document.ContentType = file.ContentType;
        document.FileSizeBytes = file.Length;
        document.DocumentType = string.IsNullOrWhiteSpace(documentType) ? document.DocumentType : documentType;
        document.Status = DocumentStatus.Pending;  // Reset to pending on re-upload
        document.VerifiedByUserId = null;
        document.VerifiedAtUtc = null;
        document.Remarks = null;
        document.UpdatedAtUtc = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);
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

    public async Task<IReadOnlyCollection<DocumentResponse>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetAllAsync(status, cancellationToken);
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

        document.Status = isVerified ? DocumentStatus.Verified : DocumentStatus.ReuploadRequired;
        document.VerifiedByUserId = adminUserId;
        document.VerifiedAtUtc = isVerified ? DateTime.UtcNow : null;
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

    public async Task<DocumentResponse> LinkAsync(Guid userId, Guid documentId, Guid targetApplicationId, CancellationToken cancellationToken = default)
    {
        var originalDocument = await _documentRepository.GetByIdAsync(documentId, cancellationToken)
                               ?? throw new KeyNotFoundException("Original document not found.");

        if (originalDocument.UserId != userId)
            throw new UnauthorizedAccessException("You are not allowed to link this document.");

        // Create a new document entity pointing to the exact same stored file
        var newDocument = new LoanDocument
        {
            ApplicationId = targetApplicationId,
            UserId = userId,
            FileName = originalDocument.FileName,
            StoredFileName = originalDocument.StoredFileName,
            ContentType = originalDocument.ContentType,
            FileSizeBytes = originalDocument.FileSizeBytes,
            DocumentType = originalDocument.DocumentType,
            Status = DocumentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(newDocument, cancellationToken);
        return MapToResponse(newDocument);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(Guid documentId, Guid? userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken)
                       ?? throw new KeyNotFoundException("Document not found.");

        if (!isAdmin && document.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to download this document.");
        }

        var stream = await _fileStorageService.GetFileStreamAsync(document.StoredFileName, cancellationToken)
                     ?? throw new KeyNotFoundException("Physical file not found in storage.");

        return (stream, document.ContentType, document.FileName);
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
            Status = document.Status.ToString(),
            IsVerified = document.IsVerified,
            VerifiedByUserId = document.VerifiedByUserId,
            VerifiedAtUtc = document.VerifiedAtUtc,
            Remarks = document.Remarks,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc
        };
    }
}
