using CapFinLoan.Auth.Application.Contracts.Responses;
using CapFinLoan.Auth.Application.Contracts.Requests;
using CapFinLoan.Auth.Application.Exceptions;
using CapFinLoan.Auth.Application.Interfaces;
using CapFinLoan.Auth.Application.Services;
using CapFinLoan.Auth.Domain.Constants;
using CapFinLoan.Auth.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace CapFinLoan.Auth.UnitTests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private Mock<IEventPublisher> _eventPublisherMock;
    private Mock<IOtpRepository> _otpRepositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _otpRepositoryMock = new Mock<IOtpRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object,
            _eventPublisherMock.Object,
            _otpRepositoryMock.Object,
            _configurationMock.Object);
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsJwtResponse()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com",
            IsActive = true
        };

        var request = new LoginRequest
        {
            Email = "john@example.com",
            Password = "Password@123"
        };

        var expectedExpiry = DateTime.UtcNow.AddHours(1);
        var roles = new List<string> { RoleNames.Applicant };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.CheckPasswordAsync(user, "Password@123"))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(repo => repo.GetRolesAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _jwtTokenGeneratorMock.Setup(generator => generator.GenerateTokenAsync(user, It.IsAny<IList<string>>()))
            .ReturnsAsync(("jwt-token", expectedExpiry));

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.ExpiresAtUtc.Should().Be(expectedExpiry);
        result.Role.Should().Be(RoleNames.Applicant);
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("john@example.com");

        _jwtTokenGeneratorMock.Verify(generator => generator.GenerateTokenAsync(
            user,
            It.Is<IList<string>>(r => r.Count == 1 && r[0] == RoleNames.Applicant)), Times.Once);
    }

    [Test]
    public async Task SendSignupOtpAsync_UserAlreadyExists_ThrowsAccountConflictException()
    {
        // Arrange
        var email = "test@example.com";
        _userRepositoryMock.Setup(repo => repo.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var action = async () => await _authService.SendSignupOtpAsync(email);

        await action.Should().ThrowAsync<AccountConflictException>()
            .WithMessage("An account already exists with this email.");

        // Verify OTP is NOT generated and Event is NOT published
        _otpRepositoryMock.Verify(repo => repo.GenerateOtpAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisherMock.Verify(pub => pub.PublishAsync(It.IsAny<OtpSendEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SendSignupOtpAsync_ValidEmail_GeneratesOtpAndPublishesEvent()
    {
        // Arrange
        var email = "newuser@example.com";
        _userRepositoryMock.Setup(repo => repo.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedOtp = new EmailVerificationOtp
        {
            OtpCode = "123456",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        };

        _otpRepositoryMock.Setup(repo => repo.GenerateOtpAsync(email, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOtp);

        // Act
        var result = await _authService.SendSignupOtpAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Email.Should().Be(email);
        result.ExpiryMinutes.Should().Be(10);

        _otpRepositoryMock.Verify(repo => repo.GenerateOtpAsync(email, 10, It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(pub => pub.PublishAsync(It.Is<OtpSendEvent>(e =>
            e.Email == email &&
            e.OtpCode == "123456"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
