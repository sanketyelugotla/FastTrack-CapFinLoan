using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;

namespace CapFinLoan.Application.Infrastructure.Messaging;

public class ApplicationStatusChangedConsumer : IConsumer<ApplicationStatusChangedEvent>
{
    public Task Consume(ConsumeContext<ApplicationStatusChangedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[RabbitMQ] Application Service received ApplicationStatusChangedEvent:");
        Console.WriteLine($"  Application: {message.ApplicationNumber}");
        Console.WriteLine($"  Status: {message.PreviousStatus} → {message.NewStatus}");
        Console.WriteLine($"  Changed by: {message.ChangedByUserId} at {message.ChangedAtUtc:u}");
        if (!string.IsNullOrWhiteSpace(message.Remarks))
            Console.WriteLine($"  Remarks: {message.Remarks}");
        return Task.CompletedTask;
    }
}
