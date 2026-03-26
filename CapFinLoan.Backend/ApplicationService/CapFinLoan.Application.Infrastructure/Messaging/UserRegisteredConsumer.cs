using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;

namespace CapFinLoan.Application.Infrastructure.Messaging;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[RabbitMQ] Application Service received UserRegisteredEvent:");
        Console.WriteLine($"  User: {message.FullName} ({message.Email})");
        Console.WriteLine($"  Role: {message.Role}");
        Console.WriteLine($"  Registered at: {message.RegisteredAtUtc:u}");
        return Task.CompletedTask;
    }
}
