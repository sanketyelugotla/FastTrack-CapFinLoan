namespace CapFinLoan.Application.Application.Contracts.Requests;

public class SaveApplicantProfileRequest
{
    public PersonalDetailsRequest PersonalDetails { get; set; } = new();
    public EmploymentDetailsRequest EmploymentDetails { get; set; } = new();
}
