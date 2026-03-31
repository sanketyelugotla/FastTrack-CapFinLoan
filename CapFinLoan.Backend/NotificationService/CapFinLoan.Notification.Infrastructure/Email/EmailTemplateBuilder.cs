using System.Globalization;
using System.Text;
using CapFinLoan.Messaging.Contracts.Events;
using CapFinLoan.Notification.Application.Models;

namespace CapFinLoan.Notification.Infrastructure.Email;

public static class EmailTemplateBuilder
{
    public static string BuildWelcome(UserRegisteredEvent message)
    {
        return BuildLayout(
            $"Welcome to CapFinLoan, {Escape(message.FullName)}",
            $"<p>Hello {Escape(message.FullName)},</p><p>Your account has been created successfully with role <strong>{Escape(message.Role)}</strong>.</p><p>Thank you for choosing CapFinLoan.</p>");
    }

    public static string BuildApplicationSubmitted(ApplicationSubmittedEvent message)
    {
        var amount = message.RequestedAmount.ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));
        return BuildLayout(
            $"Application Submitted: {Escape(message.ApplicationNumber)}",
            $"<p>Hello {Escape(message.ApplicantName)},</p><p>Your loan application <strong>{Escape(message.ApplicationNumber)}</strong> has been submitted successfully.</p><p>Amount: <strong>{Escape(amount)}</strong><br/>Tenure: <strong>{message.RequestedTenureMonths} months</strong></p><p>We will notify you when the review progresses.</p>");
    }

    public static string BuildApplicationStatusChanged(ApplicationStatusChangedEvent message, string recipientName)
    {
        var remarks = string.IsNullOrWhiteSpace(message.Remarks) ? string.Empty : $"<p><strong>Remarks:</strong> {Escape(message.Remarks)}</p>";
        return BuildLayout(
            $"Application Status Updated: {Escape(message.ApplicationNumber)}",
            $"<p>Hello {Escape(recipientName)},</p><p>Your application <strong>{Escape(message.ApplicationNumber)}</strong> moved from <strong>{Escape(message.PreviousStatus)}</strong> to <strong>{Escape(message.NewStatus)}</strong>.</p>{remarks}<p>Updated at {message.ChangedAtUtc:u} UTC.</p>");
    }

    public static string BuildDocumentVerified(DocumentVerifiedEvent message, string recipientName)
    {
        var status = message.IsVerified ? "Verified" : "Reupload Required";
        var remarks = string.IsNullOrWhiteSpace(message.Remarks) ? string.Empty : $"<p><strong>Remarks:</strong> {Escape(message.Remarks)}</p>";
        return BuildLayout(
            $"Document Review Update: {Escape(message.DocumentType)}",
            $"<p>Hello {Escape(recipientName)},</p><p>Your document <strong>{Escape(message.DocumentType)}</strong> ({Escape(message.FileName)}) has been reviewed.</p><p>Status: <strong>{status}</strong></p>{remarks}<p>Reviewed at {message.VerifiedAtUtc:u} UTC.</p>");
    }

    public static string BuildOpsAlert(ApplicationSubmittedEvent message)
    {
        var amount = message.RequestedAmount.ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));
        return BuildLayout(
            $"New Loan Application: {Escape(message.ApplicationNumber)}",
            $"<p>A new application has been submitted.</p><p>Applicant: <strong>{Escape(message.ApplicantName)}</strong><br/>Email: <strong>{Escape(message.Email)}</strong><br/>Amount: <strong>{Escape(amount)}</strong><br/>Tenure: <strong>{message.RequestedTenureMonths} months</strong></p>");
    }

    private static string BuildLayout(string title, string bodyHtml)
    {
        return $"<html><body style='font-family:Segoe UI,Arial,sans-serif;background:#f4f7fb;padding:24px;'><div style='max-width:640px;margin:auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:10px;padding:24px;'><h2 style='margin-top:0;color:#0f172a;'>{title}</h2>{bodyHtml}<hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0;'/><p style='margin:0;color:#475569;font-size:13px;'>CapFinLoan Notification Service</p></div></body></html>";
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '<' => "&lt;",
                '>' => "&gt;",
                '&' => "&amp;",
                '"' => "&quot;",
                '\'' => "&#39;",
                _ => ch.ToString()
            });
        }

        return builder.ToString();
    }
}
