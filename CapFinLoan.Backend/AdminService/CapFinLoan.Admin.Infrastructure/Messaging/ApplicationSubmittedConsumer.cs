using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;

namespace CapFinLoan.Admin.Infrastructure.Messaging;

public class ApplicationSubmittedConsumer : IConsumer<ApplicationSubmittedEvent>
{
    public Task Consume(ConsumeContext<ApplicationSubmittedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[RabbitMQ] Admin Service received ApplicationSubmittedEvent:");
        Console.WriteLine($"  Application: {message.ApplicationNumber}");
        Console.WriteLine($"  Applicant: {message.ApplicantName} ({message.Email})");
        Console.WriteLine($"  Amount: {message.RequestedAmount:C}, Tenure: {message.RequestedTenureMonths} months");
        Console.WriteLine($"  Submitted at: {message.SubmittedAtUtc:u}");
        return Task.CompletedTask;
    }
}
