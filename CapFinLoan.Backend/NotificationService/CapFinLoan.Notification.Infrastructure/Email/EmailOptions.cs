namespace CapFinLoan.Notification.Infrastructure.Email;

public class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "CapFinLoan";
    public string AlertRecipient { get; set; } = string.Empty;
}
