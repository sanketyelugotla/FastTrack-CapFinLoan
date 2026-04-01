using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;
using MassTransit;

namespace CapFinLoan.Notification.Infrastructure.Messaging;

public class OtpSendConsumer : IConsumer<OtpSendEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OtpSendConsumer> _logger;

    public OtpSendConsumer(IEmailSender emailSender, ILogger<OtpSendConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OtpSendEvent> context)
    {
        try
        {
            _logger.LogInformation("Sending OTP email to {Email}", context.Message.Email);

            var htmlContent = $@"
            <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #f9f9f9; padding: 20px; border-radius: 8px;'>
                        <h2 style='color: #2c3e50; text-align: center;'>Email Verification Code</h2>
                        
                        <p>Hello,</p>
                        
                        <p>Your one-time password (OTP) to verify your email for CapFinLoan account signup is:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <p style='font-size: 36px; font-weight: bold; letter-spacing: 5px; color: #27ae60; background-color: #ecf0f1; padding: 20px; border-radius: 5px;'>
                                {context.Message.OtpCode}
                            </p>
                        </div>
                        
                        <p style='color: #e74c3c;'><strong>⚠️ Important:</strong> This code expires at {context.Message.ExpiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC</p>
                        
                        <p>If you did not request this code, please ignore this email.</p>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;' />
                        
                        <p style='font-size: 12px; color: #7f8c8d;'>
                            This is an automated message. Please do not reply to this email.
                        </p>
                        
                        <p style='font-size: 12px; color: #7f8c8d;'>
                            &copy; CapFinLoan Inc. All rights reserved.
                        </p>
                    </div>
                </body>
            </html>";


            await _emailSender.SendHtmlAsync(
                toEmail: context.Message.Email,
                subject: "Email Verification Code - CapFinLoan",
                htmlBody: htmlContent
            );
            _logger.LogInformation("OTP email sent successfully to {Email}", context.Message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", context.Message.Email);
            throw;
        }
    }
}
