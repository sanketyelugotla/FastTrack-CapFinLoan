namespace CapFinLoan.Application.Application.Contracts.Requests;

public class WithdrawRequest
{
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
}
