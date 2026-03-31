namespace CapFinLoan.Notification.Infrastructure.Clients;

public class NotificationDependencyOptions
{
    public string AuthServiceBaseUrl { get; set; } = "http://auth-service:8080";
    public string InternalApiKey { get; set; } = string.Empty;
}
