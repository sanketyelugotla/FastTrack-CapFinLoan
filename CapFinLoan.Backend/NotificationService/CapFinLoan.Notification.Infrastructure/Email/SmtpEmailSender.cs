using CapFinLoan.Notification.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

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

        _logger.LogInformation("Sending email via MailKit: SMTP Host={SmtpHost}, Port={Port}, Username={Username}",
            _options.SmtpHost, _options.Port, _options.Username);

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

        using var smtp = new SmtpClient();
        try
        {
            // Auto: tries STARTTLS on 587, SSL on 465
            await smtp.ConnectAsync(_options.SmtpHost, _options.Port, SecureSocketOptions.Auto, cancellationToken);
            await smtp.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Recipient}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}: {Error}", toEmail, ex.Message);
            throw;
        }
    }
}
