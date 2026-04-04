using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Email;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

/// <summary>
/// SAGA PARTICIPANT: Sends email notifications on status changes.
/// On failure → publishes NotificationFailedEvent (non-critical, no rollback).
/// </summary>
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

        try
        {
            var userProfile = await _userProfileClient.GetByIdAsync(message.ApplicantUserId, context.CancellationToken);
            if (userProfile is null || string.IsNullOrWhiteSpace(userProfile.Email))
            {
                _logger.LogWarning("Skipped ApplicationStatusChanged email for application {ApplicationId}. Applicant email lookup failed.", message.ApplicationId);
                return;
            }

            var body = EmailTemplateBuilder.BuildApplicationStatusChanged(message, userProfile.Name);
            await _emailSender.SendHtmlAsync(userProfile.Email, "Application Status Updated", body, context.CancellationToken);
            _logger.LogInformation("[SAGA] Successfully sent status change email to {Email} for {AppNumber}", userProfile.Email, message.ApplicationNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SAGA] Failed to send status change email for {AppNumber}", message.ApplicationNumber);

            // Publish non-critical failure event — does NOT trigger saga rollback
            await context.Publish(new NotificationFailedEvent
            {
                ApplicationId = message.ApplicationId,
                ApplicantUserId = message.ApplicantUserId,
                ApplicationNumber = message.ApplicationNumber,
                NotificationType = "ApplicationStatusChanged",
                FailureReason = ex.Message,
                FailedAtUtc = DateTime.UtcNow
            });

            // Do NOT rethrow — email failure should not block the saga
        }
    }
}
