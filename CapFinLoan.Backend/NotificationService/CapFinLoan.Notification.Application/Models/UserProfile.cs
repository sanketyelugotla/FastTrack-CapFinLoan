namespace CapFinLoan.Notification.Application.Models;

public record UserProfile
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
