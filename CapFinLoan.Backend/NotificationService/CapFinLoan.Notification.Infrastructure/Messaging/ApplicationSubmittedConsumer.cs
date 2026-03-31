using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Email;
using MassTransit;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

public class ApplicationSubmittedConsumer : IConsumer<ApplicationSubmittedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;

    public ApplicationSubmittedConsumer(IEmailSender emailSender, IOptions<EmailOptions> emailOptions)
    {
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
    }

    public async Task Consume(ConsumeContext<ApplicationSubmittedEvent> context)
    {
        var message = context.Message;
        var applicantBody = EmailTemplateBuilder.BuildApplicationSubmitted(message);
        await _emailSender.SendHtmlAsync(message.Email, "Application Submitted", applicantBody, context.CancellationToken);

        if (!string.IsNullOrWhiteSpace(_emailOptions.AlertRecipient))
        {
            var opsBody = EmailTemplateBuilder.BuildOpsAlert(message);
            await _emailSender.SendHtmlAsync(_emailOptions.AlertRecipient, "New Application Submitted", opsBody, context.CancellationToken);
        }
    }
}
