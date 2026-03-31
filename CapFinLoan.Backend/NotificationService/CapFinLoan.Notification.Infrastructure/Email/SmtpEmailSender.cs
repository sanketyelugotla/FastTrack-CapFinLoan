using System.Net;
using System.Net.Mail;
using CapFinLoan.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CapFinLoan.Notification.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendHtmlAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Skipped email send because recipient is empty. Subject: {Subject}", subject);
            return;
        }

        using var message = new MailMessage
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
            From = new MailAddress(_options.FromAddress, _options.FromName)
        };
        message.To.Add(new MailAddress(toEmail));

        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        await smtpClient.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Notification email sent to {Recipient} with subject {Subject}", toEmail, subject);
    }
}
