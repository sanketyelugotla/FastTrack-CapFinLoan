using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Email;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

public class ApplicationStatusChangedConsumer : IConsumer<ApplicationStatusChangedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserProfileClient _userProfileClient;
    private readonly ILogger<ApplicationStatusChangedConsumer> _logger;

    public ApplicationStatusChangedConsumer(IEmailSender emailSender, IUserProfileClient userProfileClient, ILogger<ApplicationStatusChangedConsumer> logger)
    {
        _emailSender = emailSender;
        _userProfileClient = userProfileClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ApplicationStatusChangedEvent> context)
    {
        var message = context.Message;
        var userProfile = await _userProfileClient.GetByIdAsync(message.ApplicantUserId, context.CancellationToken);
        if (userProfile is null || string.IsNullOrWhiteSpace(userProfile.Email))
        {
            _logger.LogWarning("Skipped ApplicationStatusChanged email for application {ApplicationId}. Applicant email lookup failed.", message.ApplicationId);
            return;
        }

        var body = EmailTemplateBuilder.BuildApplicationStatusChanged(message, userProfile.Name);
        await _emailSender.SendHtmlAsync(userProfile.Email, "Application Status Updated", body, context.CancellationToken);
    }
}
