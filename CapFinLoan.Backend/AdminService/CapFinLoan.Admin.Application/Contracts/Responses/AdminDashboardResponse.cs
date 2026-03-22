namespace CapFinLoan.Admin.Application.Contracts.Responses;

public class AdminDashboardResponse
{
    public int TotalApplications { get; set; }
    public int SubmittedCount { get; set; }
    public int DocsPendingCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}