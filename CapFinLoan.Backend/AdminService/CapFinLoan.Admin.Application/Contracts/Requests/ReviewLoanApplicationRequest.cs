using System.ComponentModel.DataAnnotations;

namespace CapFinLoan.Admin.Application.Contracts.Requests;

public class ReviewLoanApplicationRequest
{
    [Required]
    public string TargetStatus { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Remarks { get; set; } = string.Empty;

    /// Rate of interest (%) — required when approving a loan
    [Range(0.1, 50.0)]
    public decimal? InterestRate { get; set; }

    /// Approved sanction amount — defaults to requested amount if not specified.
    [Range(1000, 50000000)]
    public decimal? SanctionAmount { get; set; }
}