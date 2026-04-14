namespace CapFinLoan.Application.Application.Contracts.Responses;

public class ApplicantProfileResponse
{
    public Guid ApplicantUserId { get; set; }
    public PersonalDetailsResponse PersonalDetails { get; set; } = new();
    public EmploymentDetailsResponse EmploymentDetails { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
