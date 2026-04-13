using System.Net;
using CapFinLoan.Document.Application.Exceptions;
using CapFinLoan.Document.Application.Interfaces;
using CapFinLoan.Document.Application.Services;
using CapFinLoan.Document.Domain.Constants;
using CapFinLoan.Document.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CapFinLoan.Document.UnitTests.Services;

[TestFixture]
public class DocumentServiceTests
{
    private Mock<IDocumentRepository> _repositoryMock;
    private Mock<IFileStorageService> _fileStorageMock;
    private Mock<IEventPublisher> _eventPublisherMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private DocumentService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        // Default HTTP mock to return Draft status (i.e., allowed to upload)
        SetupHttpClientMock("Draft");

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _service = new DocumentService(
            _repositoryMock.Object,
            _fileStorageMock.Object,
            _eventPublisherMock.Object,
            _httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    private void SetupHttpClientMock(string statusString, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent($"{{\"currentStatus\":\"{statusString}\"}}")
            });
    }

    private Mock<IFormFile> CreateFileMock(long length, string contentType, string fileName = "test.pdf")
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(m => m.Length).Returns(length);
        fileMock.Setup(m => m.ContentType).Returns(contentType);
        fileMock.Setup(m => m.FileName).Returns(fileName);
        fileMock.Setup(m => m.OpenReadStream()).Returns(new MemoryStream());
        return fileMock;
    }

    [Test]
    public async Task UploadAsync_ZeroBytes_ThrowsDocumentValidationException()
    {
        // Arrange
        var fileMock = CreateFileMock(0, "application/pdf");

        // Act & Assert
        var action = async () => await _service.UploadAsync(Guid.NewGuid(), Guid.NewGuid(), "IDProof", fileMock.Object);

        await action.Should().ThrowAsync<DocumentValidationException>()
            .WithMessage("File is empty.");
    }

    [Test]
    public async Task UploadAsync_FileTooLarge_ThrowsDocumentValidationException()
    {
        // Arrange
        long size = 6L * 1024 * 1024; // 6MB
        var fileMock = CreateFileMock(size, "application/pdf");

        // Act & Assert
        var action = async () => await _service.UploadAsync(Guid.NewGuid(), Guid.NewGuid(), "IDProof", fileMock.Object);

        await action.Should().ThrowAsync<DocumentValidationException>()
            .WithMessage("File size exceeds the maximum allowed size of 5 MB.");
    }

    [Test]
    public async Task UploadAsync_InvalidContentType_ThrowsDocumentValidationException()
    {
        // Arrange
        var fileMock = CreateFileMock(1024, "application/x-msdownload", "virus.exe");

        // Act & Assert
        var action = async () => await _service.UploadAsync(Guid.NewGuid(), Guid.NewGuid(), "IDProof", fileMock.Object);

        await action.Should().ThrowAsync<DocumentValidationException>()
            .WithMessage("File type is not supported. Allowed types: PDF, JPG, PNG.");
    }

    [TestCase("Approved")]
    [TestCase("Rejected")]
    public async Task UploadAsync_ApplicationInTerminalState_ThrowsDocumentConflictException(string status)
    {
        // Arrange
        var fileMock = CreateFileMock(1024, "application/pdf");
        SetupHttpClientMock(status);

        // Act & Assert
        var action = async () => await _service.UploadAsync(Guid.NewGuid(), Guid.NewGuid(), "IDProof", fileMock.Object);

        await action.Should().ThrowAsync<DocumentConflictException>()
            .WithMessage($"Documents cannot be updated because the application is already {status}.");
    }

    [Test]
    public async Task UploadAsync_ValidFile_ReturnsStoredDocument()
    {
        // Arrange
        var fileMock = CreateFileMock(1024, "image/jpeg", "passport.jpg");
        var appId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _fileStorageMock.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("random-guid-passport.jpg");

        // Act
        var result = await _service.UploadAsync(userId, appId, "IDProof", fileMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("passport.jpg");
        result.Status.Should().Be("Pending");

        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<LoanDocument>(d =>
            d.ApplicationId == appId &&
            d.UserId == userId &&
            d.DocumentType == "IDProof"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task VerifyAsync_Rejection_RequiresReuploadAndEmitsEvent()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var document = new LoanDocument
        {
            Id = documentId,
            ApplicationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = DocumentStatus.Pending
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _service.VerifyAsync(documentId, adminId, false, "Image is blurry");

        // Assert
        result.Status.Should().Be(DocumentStatus.ReuploadRequired.ToString());
        result.IsVerified.Should().BeFalse();
        result.Remarks.Should().Be("Image is blurry");
        result.VerifiedAtUtc.Should().BeNull(); // Null because it wasn't approved

        _eventPublisherMock.Verify(pub => pub.PublishAsync(It.Is<DocumentVerifiedEvent>(e =>
            e.IsVerified == false &&
            e.Remarks == "Image is blurry"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
