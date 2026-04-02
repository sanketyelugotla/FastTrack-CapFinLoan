using System.ComponentModel.DataAnnotations;

namespace CapFinLoan.Auth.Application.Contracts.Requests;

public class GoogleLoginRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
