using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Email;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

/// <summary>
/// SAGA COMPENSATION: Sends corrective email when application status is rolled back.
/// This handles the "negative notification" to inform the customer of the rollback.
/// </summary>
public class ApplicationStatusRolledBackConsumer : IConsumer<ApplicationStatusRolledBackEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserProfileClient _userProfileClient;
    private readonly ILogger<ApplicationStatusRolledBackConsumer> _logger;

    public ApplicationStatusRolledBackConsumer(IEmailSender emailSender, IUserProfileClient userProfileClient, ILogger<ApplicationStatusRolledBackConsumer> logger)
    {
        _emailSender = emailSender;
        _userProfileClient = userProfileClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ApplicationStatusRolledBackEvent> context)
    {
        var message = context.Message;

        try
        {
            var userProfile = await _userProfileClient.GetByIdAsync(message.ApplicantUserId, context.CancellationToken);
            if (userProfile is null || string.IsNullOrWhiteSpace(userProfile.Email))
            {
                _logger.LogWarning("Skipped ApplicationStatusRolledBack email for application {ApplicationId}. Applicant email lookup failed.", message.ApplicationId);
                return;
            }

            var body = EmailTemplateBuilder.BuildApplicationStatusRolledBack(message, userProfile.Name);
            await _emailSender.SendHtmlAsync(userProfile.Email, "Application Status Update - Correction", body, context.CancellationToken);
            _logger.LogInformation("[SAGA COMPENSATION] Successfully sent rollback correction email to {Email} for {AppNumber}", userProfile.Email, message.ApplicationNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SAGA COMPENSATION] Failed to send rollback correction email for {AppNumber}", message.ApplicationNumber);

            // Publish non-critical failure event — does NOT trigger saga rollback
            await context.Publish(new NotificationFailedEvent
            {
                ApplicationId = message.ApplicationId,
                ApplicantUserId = message.ApplicantUserId,
                ApplicationNumber = message.ApplicationNumber,
                NotificationType = "ApplicationStatusRolledBack",
                FailureReason = ex.Message,
                FailedAtUtc = DateTime.UtcNow
            });

            // Do NOT rethrow — email failure should not block the saga
        }
    }
}