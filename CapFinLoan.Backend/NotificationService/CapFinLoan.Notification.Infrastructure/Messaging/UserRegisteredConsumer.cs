using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Email;
using MassTransit;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailSender _emailSender;

    public UserRegisteredConsumer(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        var subject = "Welcome to CapFinLoan";
        var body = EmailTemplateBuilder.BuildWelcome(message);
        await _emailSender.SendHtmlAsync(message.Email, subject, body, context.CancellationToken);
    }
}
