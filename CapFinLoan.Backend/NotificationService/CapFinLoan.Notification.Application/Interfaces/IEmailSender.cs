namespace CapFinLoan.Notification.Application.Interfaces;

public interface IEmailSender
{
    Task SendHtmlAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
