using CapFinLoan.Chatbot.Application.Interfaces;

namespace CapFinLoan.Chatbot.Application.FunctionCalling;

/// <summary>
/// Provides Gemini function declarations for the applicant role.
/// </summary>
public static class ApplicantFunctions
{
    public static List<GeminiFunctionDeclaration> GetDeclarations() =>
    [
        new()
        {
            Name = "get_my_applications",
            Description = "Gets the list of loan applications belonging to the currently logged-in applicant, including their statuses.",
            Parameters = new GeminiSchema { Type = "OBJECT", Properties = new(), Required = new() }
        },

        new()
        {
            Name = "get_application_status",
            Description = "Gets the detailed status timeline for a specific loan application. Use this when the user asks about the status or progress of a specific application.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["applicationId"] = new() { Type = "STRING", Description = "The GUID ID of the loan application" }
                },
                Required = new() { "applicationId" }
            }
        },

        new()
        {
            Name = "get_my_profile",
            Description = "Gets the applicant's saved profile information (personal and employment details). Use this when the user asks about their saved info.",
            Parameters = new GeminiSchema { Type = "OBJECT", Properties = new(), Required = new() }
        },

        new()
        {
            Name = "get_my_documents",
            Description = "Gets all documents uploaded by the current applicant across all applications.",
            Parameters = new GeminiSchema { Type = "OBJECT", Properties = new(), Required = new() }
        },

        new()
        {
            Name = "calculate_emi",
            Description = "Calculates the estimated monthly EMI for a loan. Use when the user asks about EMI, monthly payments, or affordability.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["principalAmount"] = new() { Type = "NUMBER", Description = "Loan principal amount in INR" },
                    ["annualInterestRate"] = new() { Type = "NUMBER", Description = "Annual interest rate as percentage (e.g. 12 for 12%)" },
                    ["tenureMonths"] = new() { Type = "INTEGER", Description = "Loan tenure in months" }
                },
                Required = new() { "principalAmount", "annualInterestRate", "tenureMonths" }
            }
        },

        new()
        {
            Name = "get_form_requirements",
            Description = "Returns the complete form field requirements including validation rules for each step of the loan application form. Use when the user asks what fields are needed or what information to prepare.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["step"] = new()
                    {
                        Type = "STRING",
                        Description = "Which step to get requirements for",
                        Enum = new() { "personal", "employment", "loan", "documents", "all" }
                    }
                },
                Required = new() { "step" }
            }
        }
    ];
}
