using CapFinLoan.Admin.Application.Contracts.Requests;
using CapFinLoan.Admin.Application.Interfaces;
using CapFinLoan.Admin.Application.Services;
using CapFinLoan.Admin.Domain.Constants;
using CapFinLoan.Admin.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CapFinLoan.Admin.UnitTests.Services;

[TestFixture]
public class AdminLoanApplicationServiceTests
{
    private Mock<IAdminLoanApplicationRepository> _repositoryMock;
    private Mock<IEventPublisher> _eventPublisherMock;
    private AdminLoanApplicationService _service;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IAdminLoanApplicationRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _service = new AdminLoanApplicationService(_repositoryMock.Object, _eventPublisherMock.Object);
    }

    private static LoanApplication CreateApplication(string status)
    {
        return new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicantUserId = Guid.NewGuid(),
            ApplicationNumber = "APP-123",
            Status = status,
            RequestedAmount = 500000,
            StatusHistory = new List<ApplicationStatusHistory>(),
            Decisions = new List<Decision>()
        };
    }

    [Test]
    public async Task UpdateStatusAsync_ReviewDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var application = CreateApplication(ApplicationStatuses.Draft);
        var request = new ReviewLoanApplicationRequest { TargetStatus = ApplicationStatuses.UnderReview, Remarks = "Starting review" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.UpdateStatusAsync(application.Id, Guid.NewGuid(), request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Draft applications cannot be reviewed by admin.");
    }

    [Test]
    public async Task UpdateStatusAsync_DocsPendingMissingRemarks_ThrowsInvalidOperationException()
    {
        // Arrange
        var application = CreateApplication(ApplicationStatuses.Submitted);
        var request = new ReviewLoanApplicationRequest { TargetStatus = ApplicationStatuses.DocsPending, Remarks = "" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.UpdateStatusAsync(application.Id, Guid.NewGuid(), request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Remarks are required when requesting document re-upload.");
    }

    [Test]
    public async Task UpdateStatusAsync_SameStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var application = CreateApplication(ApplicationStatuses.UnderReview);
        var request = new ReviewLoanApplicationRequest { TargetStatus = ApplicationStatuses.UnderReview, Remarks = "Still reviewing" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.UpdateStatusAsync(application.Id, Guid.NewGuid(), request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Application is already in the requested status.");
    }

    [Test]
    public async Task UpdateStatusAsync_ValidApproval_SetsSanctionAmountAndPublishesEvent()
    {
        // Arrange
        var application = CreateApplication(ApplicationStatuses.UnderReview);
        var reviewerId = Guid.NewGuid();
        var request = new ReviewLoanApplicationRequest 
        { 
            TargetStatus = ApplicationStatuses.Approved, 
            Remarks = "Looks good",
            SanctionAmount = 450000,
            InterestRate = 12.5m
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var result = await _service.UpdateStatusAsync(application.Id, reviewerId, request);

        // Assert
        result.Status.Should().Be(ApplicationStatuses.Approved);
        
        // Ensure Decisions list handles the specific Decision info
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.Is<LoanApplication>(a => 
            a.Status == ApplicationStatuses.Approved &&
            a.Decisions.Any(d => d.SanctionAmount == 450000 && d.InterestRate == 12.5m)), 
            It.IsAny<CancellationToken>()), Times.Once);

        _eventPublisherMock.Verify(pub => pub.PublishAsync(It.Is<ApplicationStatusChangedEvent>(e =>
            e.ApplicationId == application.Id &&
            e.NewStatus == ApplicationStatuses.Approved), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
