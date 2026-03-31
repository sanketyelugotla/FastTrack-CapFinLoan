using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Email;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

public class DocumentVerifiedConsumer : IConsumer<DocumentVerifiedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserProfileClient _userProfileClient;
    private readonly ILogger<DocumentVerifiedConsumer> _logger;

    public DocumentVerifiedConsumer(IEmailSender emailSender, IUserProfileClient userProfileClient, ILogger<DocumentVerifiedConsumer> logger)
    {
        _emailSender = emailSender;
        _userProfileClient = userProfileClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentVerifiedEvent> context)
    {
        var message = context.Message;
        var userProfile = await _userProfileClient.GetByIdAsync(message.UserId, context.CancellationToken);
        if (userProfile is null || string.IsNullOrWhiteSpace(userProfile.Email))
        {
            _logger.LogWarning("Skipped DocumentVerified email for document {DocumentId}. User email lookup failed.", message.DocumentId);
            return;
        }

        var body = EmailTemplateBuilder.BuildDocumentVerified(message, userProfile.Name);
        await _emailSender.SendHtmlAsync(userProfile.Email, "Document Verification Update", body, context.CancellationToken);
    }
}
