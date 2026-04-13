using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Application.Contracts.Requests;
using CapFinLoan.Application.Application.Exceptions;
using CapFinLoan.Application.Application.Services;
using CapFinLoan.Application.Domain.Constants;
using CapFinLoan.Application.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CapFinLoan.Application.UnitTests.Services;

[TestFixture]
public class LoanApplicationServiceTests
{
    private Mock<ILoanApplicationRepository> _repositoryMock;
    private Mock<IEventPublisher> _eventPublisherMock;
    private LoanApplicationService _service;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<ILoanApplicationRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _service = new LoanApplicationService(_repositoryMock.Object, _eventPublisherMock.Object);
    }

    private static LoanApplication CreateValidDraftApplication(Guid applicantId)
    {
        return new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicantUserId = applicantId,
            ApplicationNumber = "APP-1234",
            Status = ApplicationStatuses.Draft,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "john@example.com",
            Phone = "1234567890",
            AddressLine1 = "123 Main St",
            City = "Anytown",
            State = "NY",
            PostalCode = "10001",
            EmployerName = "Tech Corp",
            EmploymentType = "Salaried",
            MonthlyIncome = 100000,
            AnnualIncome = 1200000,
            ExistingEmiAmount = 10000,
            RequestedAmount = 500000,
            RequestedTenureMonths = 60,
            LoanPurpose = "Home Renovation",
            StatusHistory = new List<ApplicationStatusHistory>()
        };
    }

    [Test]
    public async Task CreateDraftAsync_ValidRequest_CreatesDraftWithApplicationId()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var generatedId = Guid.NewGuid();

        var request = new SaveLoanApplicationRequest
        {
            PersonalDetails = new PersonalDetailsRequest
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                Email = "john@example.com",
                Phone = "1234567890",
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 101",
                City = "Mumbai",
                State = "MH",
                PostalCode = "400001"
            },
            EmploymentDetails = new EmploymentDetailsRequest
            {
                EmployerName = "Tech Corp",
                EmploymentType = "Salaried",
                MonthlyIncome = 100000,
                AnnualIncome = 1200000,
                ExistingEmiAmount = 5000
            },
            LoanDetails = new LoanDetailsRequest
            {
                RequestedAmount = 500000,
                RequestedTenureMonths = 60,
                LoanPurpose = "Home Renovation",
                Remarks = "Need loan quickly"
            }
        };

        _repositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .Callback<LoanApplication, CancellationToken>((application, _) => application.Id = generatedId)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateDraftAsync(applicantId, request);

        // Assert
        result.Id.Should().Be(generatedId);
        result.ApplicantUserId.Should().Be(applicantId);
        result.Status.Should().Be(ApplicationStatuses.Draft);
        result.ApplicationNumber.Should().StartWith("APP-");

        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<LoanApplication>(application =>
            application.ApplicantUserId == applicantId &&
            application.Status == ApplicationStatuses.Draft &&
            application.StatusHistory.Any()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SubmitAsync_NotDraft_ThrowsApplicationConflictException()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var application = CreateValidDraftApplication(applicantId);
        application.Status = ApplicationStatuses.Submitted; // Invalid state for Submit

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.SubmitAsync(application.Id, applicantId, false);

        await action.Should().ThrowAsync<ApplicationConflictException>()
            .WithMessage("Only draft applications can be submitted.");
    }

    [Test]
    public async Task SubmitAsync_MissingRequiredField_ThrowsApplicationValidationException()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var application = CreateValidDraftApplication(applicantId);
        application.FirstName = string.Empty; // Missing First Name

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.SubmitAsync(application.Id, applicantId, false);

        await action.Should().ThrowAsync<ApplicationValidationException>()
            .WithMessage("*Application is incomplete*FirstName*");
    }

    [TestCase(5000, "Requested amount must be between 10,000 and 5,000,000.")] // Too low
    [TestCase(6000000, "Requested amount must be between 10,000 and 5,000,000.")] // Too high
    public async Task SubmitAsync_InvalidRequestedAmount_ThrowsApplicationValidationException(decimal amount, string expectedMessage)
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var application = CreateValidDraftApplication(applicantId);
        application.RequestedAmount = amount;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.SubmitAsync(application.Id, applicantId, false);

        await action.Should().ThrowAsync<ApplicationValidationException>()
            .WithMessage(expectedMessage);
    }

    [Test]
    public async Task SubmitAsync_EmiExceedsIncome_ThrowsApplicationValidationException()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var application = CreateValidDraftApplication(applicantId);
        application.MonthlyIncome = 50000;
        application.ExistingEmiAmount = 55000; // EMI greater than income

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act & Assert
        var action = async () => await _service.SubmitAsync(application.Id, applicantId, false);

        await action.Should().ThrowAsync<ApplicationValidationException>()
            .WithMessage("Monthly income must be greater than existing EMI obligations.");
    }

    [Test]
    public async Task SubmitAsync_Valid_PublishesEventAndUpdatesStatus()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var application = CreateValidDraftApplication(applicantId);

        _repositoryMock.Setup(repo => repo.GetByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var result = await _service.SubmitAsync(application.Id, applicantId, false);

        // Assert
        result.Status.Should().Be(ApplicationStatuses.Submitted);

        _repositoryMock.Verify(repo => repo.UpdateAsync(It.Is<LoanApplication>(a =>
            a.Status == ApplicationStatuses.Submitted &&
            a.SubmittedAtUtc != null),
            It.IsAny<CancellationToken>()), Times.Once);

        _eventPublisherMock.Verify(pub => pub.PublishAsync(It.Is<ApplicationSubmittedEvent>(e =>
            e.ApplicationId == application.Id &&
            e.RequestedAmount == application.RequestedAmount),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
