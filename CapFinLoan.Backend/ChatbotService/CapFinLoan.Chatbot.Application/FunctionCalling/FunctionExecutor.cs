using System.Text.Json;
using CapFinLoan.Chatbot.Application.Interfaces;

namespace CapFinLoan.Chatbot.Application.FunctionCalling;

/// <summary>
/// Executes function calls returned by Gemini by dispatching to the appropriate backend API or local logic.
/// </summary>
public class FunctionExecutor
{
    private readonly IBackendApiClient _api;

    public FunctionExecutor(IBackendApiClient api)
    {
        _api = api;
    }

    public async Task<(string result, string actionSummary)> ExecuteAsync(
        string functionName, JsonElement? args, string userRole, string bearerToken, CancellationToken ct)
    {
        try
        {
            return functionName switch
            {
                // ─── Applicant functions ──────────────────────────────────────
                "get_my_applications" => (await _api.GetMyApplicationsAsync(bearerToken, ct), "Retrieved your applications"),
                "get_application_status" => (await _api.GetApplicationStatusAsync(GetArg(args, "applicationId"), bearerToken, ct), "Retrieved application status"),
                "get_my_profile" => (await _api.GetProfileAsync(bearerToken, ct), "Retrieved your profile"),
                "get_my_documents" => (await _api.GetMyDocumentsAsync(bearerToken, ct), "Retrieved your documents"),
                "calculate_emi" => (CalculateEmi(args), "Calculated EMI"),
                "get_form_requirements" => (GetFormRequirements(GetArg(args, "step")), "Retrieved form requirements"),

                // ─── Admin functions ──────────────────────────────────────────
                "get_dashboard_summary" => (await _api.GetAdminDashboardAsync(bearerToken, ct), "Retrieved dashboard summary"),
                "get_application_queue" => (await _api.GetAdminQueueAsync(GetArgOrNull(args, "status"), bearerToken, ct), "Retrieved application queue"),
                "get_application_detail" => (await _api.GetAdminApplicationByIdAsync(GetArg(args, "applicationId"), bearerToken, ct), "Retrieved application details"),
                "update_application_status" => (await ExecuteStatusUpdate(args, bearerToken, ct), FormatStatusUpdateAction(args)),
                "get_application_documents" => (await _api.GetAdminDocumentsByApplicationAsync(GetArg(args, "applicationId"), bearerToken, ct), "Retrieved application documents"),
                "verify_document" => (await ExecuteDocVerify(args, bearerToken, ct), FormatDocVerifyAction(args)),
                "get_all_documents" => (await _api.GetAllAdminDocumentsAsync(GetArgOrNull(args, "status"), bearerToken, ct), "Retrieved all documents"),
                "get_users_list" => (await _api.GetUsersAsync(bearerToken, ct), "Retrieved users list"),

                _ => ($"{{\"error\": \"Unknown function: {functionName}\"}}", $"Unknown function: {functionName}")
            };
        }
        catch (Exception ex)
        {
            return ($"{{\"error\": \"{EscapeJson(ex.Message)}\"}}", $"Failed: {functionName} — {ex.Message}");
        }
    }

    private async Task<string> ExecuteStatusUpdate(JsonElement? args, string bearerToken, CancellationToken ct)
    {
        var applicationId = GetArg(args, "applicationId");
        var targetStatus = GetArg(args, "targetStatus");
        var remarks = GetArgOrNull(args, "remarks") ?? string.Empty;
        decimal? interestRate = GetDecimalOrNull(args, "interestRate");
        decimal? sanctionAmount = GetDecimalOrNull(args, "sanctionAmount");

        return await _api.UpdateApplicationStatusAsync(applicationId, targetStatus, remarks, interestRate, sanctionAmount, bearerToken, ct);
    }

    private async Task<string> ExecuteDocVerify(JsonElement? args, string bearerToken, CancellationToken ct)
    {
        var documentId = GetArg(args, "documentId");
        var isVerified = args?.TryGetProperty("isVerified", out var v) == true && v.GetBoolean();
        var remarks = GetArgOrNull(args, "remarks") ?? string.Empty;

        return await _api.VerifyDocumentAsync(documentId, isVerified, remarks, bearerToken, ct);
    }

    // ─── Local functions (no API calls) ──────────────────────────────────────────

    private static string CalculateEmi(JsonElement? args)
    {
        var principal = GetDecimalOrNull(args, "principalAmount") ?? 0;
        var annualRate = GetDecimalOrNull(args, "annualInterestRate") ?? 0;
        var months = (int)(GetDecimalOrNull(args, "tenureMonths") ?? 0);

        if (principal <= 0 || annualRate <= 0 || months <= 0)
            return "{\"error\": \"Principal, interest rate, and tenure must all be positive.\"}";

        var monthlyRate = annualRate / 12 / 100;
        var power = Math.Pow((double)(1 + monthlyRate), months);
        var emi = (double)principal * (double)monthlyRate * power / (power - 1);
        var totalPayment = emi * months;
        var totalInterest = totalPayment - (double)principal;

        return JsonSerializer.Serialize(new
        {
            emi = Math.Round(emi, 2),
            totalPayment = Math.Round(totalPayment, 2),
            totalInterest = Math.Round(totalInterest, 2),
            principal,
            annualInterestRate = annualRate,
            tenureMonths = months
        });
    }

    private static string GetFormRequirements(string step) => step.ToLowerInvariant() switch
    {
        "personal" => JsonSerializer.Serialize(new
        {
            step = "Personal Details",
            fields = new object[]
            {
                new { name = "First Name", required = true, type = "text" },
                new { name = "Last Name", required = true, type = "text" },
                new { name = "Date of Birth", required = true, type = "date" },
                new { name = "Gender", required = true, type = "select" },
                new { name = "Email", required = true, type = "email" },
                new { name = "Phone", required = true, type = "tel" },
                new { name = "Address Line 1", required = true, type = "text" },
                new { name = "Address Line 2", required = false, type = "text" },
                new { name = "City", required = true, type = "text" },
                new { name = "State", required = true, type = "text" },
                new { name = "Postal Code", required = true, type = "text" }
            },
            notes = "Gender options: Male, Female, Other."
        }),
        "employment" => JsonSerializer.Serialize(new
        {
            step = "Employment Details",
            fields = new object[]
            {
                new { name = "Employer Name", required = true, type = "text", validation = "" },
                new { name = "Employment Type", required = true, type = "select", validation = "Salaried, Self-Employed, Business Owner, Freelancer, Government Employee, Others" },
                new { name = "Monthly Income", required = true, type = "number", validation = "Must be > 0 and greater than existing EMI" },
                new { name = "Annual Income", required = true, type = "number", validation = "Must be > 0" },
                new { name = "Existing EMI Amount", required = false, type = "number", validation = "Must be >= 0 and less than monthly income" }
            }
        }),
        "loan" => JsonSerializer.Serialize(new
        {
            step = "Loan Details",
            fields = new object[]
            {
                new { name = "Loan Purpose", required = true, type = "select", validation = "Home, Education, Personal, Vehicle, Business, Medical, Debt Consolidation, Agriculture, Gold, Other" },
                new { name = "Requested Amount", required = true, type = "number", validation = "₹10,000 - ₹50,00,000" },
                new { name = "Requested Tenure", required = true, type = "number", validation = "6 - 360 months" },
                new { name = "Remarks", required = false, type = "textarea", validation = "" }
            }
        }),
        "documents" => JsonSerializer.Serialize(new
        {
            step = "Documents",
            requiredDocuments = new[]
            {
                new { type = "IdProof", label = "ID Proof", description = "Aadhaar, PAN, Passport, or Voter ID" },
                new { type = "AddressProof", label = "Address Proof", description = "Utility bill, Aadhaar, or Passport" },
                new { type = "IncomeProof", label = "Income Proof", description = "Salary slips (last 3 months) or ITR" },
                new { type = "BankStatement", label = "Bank Statement", description = "Last 6 months bank statement" }
            },
            notes = "Documents should be clear, legible, and not expired. Supported formats: PDF, JPG, PNG."
        }),
        _ => JsonSerializer.Serialize(new
        {
            steps = new[] { "personal", "employment", "loan", "documents", "review" },
            totalRequiredFields = 16,
            tip = "Save your profile first — it auto-fills personal and employment details for new applications."
        })
    };

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    private static string FormatStatusUpdateAction(JsonElement? args)
    {
        var status = GetArgOrNull(args, "targetStatus") ?? "unknown";
        return $"Application status updated to {status}";
    }

    private static string FormatDocVerifyAction(JsonElement? args)
    {
        var isVerified = args?.TryGetProperty("isVerified", out var v) == true && v.GetBoolean();
        return isVerified ? "Document verified" : "Document rejected for re-upload";
    }

    private static string GetArg(JsonElement? args, string name)
    {
        if (args?.TryGetProperty(name, out var value) == true)
            return value.ToString();
        throw new ArgumentException($"Missing required argument: {name}");
    }

    private static string? GetArgOrNull(JsonElement? args, string name)
    {
        if (args?.TryGetProperty(name, out var value) == true)
        {
            var str = value.ToString();
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }
        return null;
    }

    private static decimal? GetDecimalOrNull(JsonElement? args, string name)
    {
        if (args?.TryGetProperty(name, out var value) != true)
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDecimal(),
            JsonValueKind.String when decimal.TryParse(value.GetString(), out var d) => d,
            _ => null
        };
    }

    private static string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ");
}
