using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;

namespace CapFinLoan.Admin.Infrastructure.Messaging;

// Consumes NotificationFailedEvent from NotificationService.
// Non-critical — only logs the failure for monitoring/alerting purposes.
// Does NOT trigger a Saga rollback since email failure is not data-critical.
public class NotificationFailedConsumer : IConsumer<NotificationFailedEvent>
{
    public Task Consume(ConsumeContext<NotificationFailedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[SAGA WARNING] NotificationService failed to send notification:");
        Console.WriteLine($"  Application: {message.ApplicationNumber}");
        Console.WriteLine($"  Type: {message.NotificationType}");
        Console.WriteLine($"  Reason: {message.FailureReason}");
        Console.WriteLine($"  Failed at: {message.FailedAtUtc:u}");
        // In production: Send alert to Slack/PagerDuty/Admin Dashboard
        return Task.CompletedTask;
    }
}
