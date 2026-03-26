using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;

namespace CapFinLoan.Admin.Infrastructure.Messaging;

public class DocumentVerifiedConsumer : IConsumer<DocumentVerifiedEvent>
{
    public Task Consume(ConsumeContext<DocumentVerifiedEvent> context)
    {
        var message = context.Message;
        var status = message.IsVerified ? "VERIFIED" : "REJECTED";
        Console.WriteLine($"[RabbitMQ] Admin Service received DocumentVerifiedEvent:");
        Console.WriteLine($"  Document: {message.FileName} ({message.DocumentType}) — {status}");
        Console.WriteLine($"  Application ID: {message.ApplicationId}");
        Console.WriteLine($"  Verified by: {message.VerifiedByUserId} at {message.VerifiedAtUtc:u}");
        if (!string.IsNullOrWhiteSpace(message.Remarks))
            Console.WriteLine($"  Remarks: {message.Remarks}");
        return Task.CompletedTask;
    }
}
