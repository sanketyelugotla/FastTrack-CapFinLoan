using CapFinLoan.Chatbot.Application.Interfaces;

namespace CapFinLoan.Chatbot.Application.FunctionCalling;

/// <summary>
/// Provides Gemini function declarations for the admin role.
/// </summary>
public static class AdminFunctions
{
    public static List<GeminiFunctionDeclaration> GetDeclarations() =>
    [
        new()
        {
            Name = "get_dashboard_summary",
            Description = "Gets the admin dashboard summary with counts of applications by status (submitted, docs pending, under review, approved, rejected).",
            Parameters = new GeminiSchema { Type = "OBJECT", Properties = new(), Required = new() }
        },

        new()
        {
            Name = "get_application_queue",
            Description = "Gets the list of loan applications, optionally filtered by status. Use this to show pending work, find specific applications, or answer questions about the queue.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["status"] = new()
                    {
                        Type = "STRING",
                        Description = "Filter by application status. Leave empty for all.",
                        Enum = new() { "", "Submitted", "DocsPending", "DocsVerified", "UnderReview", "Approved", "Rejected" }
                    }
                },
                Required = new()
            }
        },

        new()
        {
            Name = "get_application_detail",
            Description = "Gets full details of a specific loan application including personal info, employment, loan details, and status timeline. Use when admin asks about a specific application.",
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
            Name = "update_application_status",
            Description = "Changes the status of a loan application. IMPORTANT: Only call this AFTER the admin has confirmed the action. Requires interest rate when approving. Requires remarks when rejecting or requesting docs re-upload.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["applicationId"] = new() { Type = "STRING", Description = "The GUID ID of the loan application" },
                    ["targetStatus"] = new()
                    {
                        Type = "STRING",
                        Description = "The new status to set",
                        Enum = new() { "DocsPending", "UnderReview", "Approved", "Rejected" }
                    },
                    ["remarks"] = new() { Type = "STRING", Description = "Remarks or reason for the status change. Required for rejection and docs re-upload." },
                    ["interestRate"] = new() { Type = "NUMBER", Description = "Interest rate percentage. Required when approving (e.g. 12.5 for 12.5%)." },
                    ["sanctionAmount"] = new() { Type = "NUMBER", Description = "Approved sanction amount in INR. Defaults to requested amount if not specified." }
                },
                Required = new() { "applicationId", "targetStatus", "remarks" }
            }
        },

        new()
        {
            Name = "get_application_documents",
            Description = "Gets all documents uploaded for a specific loan application. Use to check document status before making approval decisions.",
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
            Name = "verify_document",
            Description = "Verifies or rejects a specific document. IMPORTANT: Only call this AFTER the admin has confirmed. When rejecting (isVerified=false), remarks explaining the issue are required.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["documentId"] = new() { Type = "STRING", Description = "The GUID ID of the document" },
                    ["isVerified"] = new() { Type = "BOOLEAN", Description = "true to verify/approve the document, false to reject it" },
                    ["remarks"] = new() { Type = "STRING", Description = "Reason for rejection. Required when isVerified is false." }
                },
                Required = new() { "documentId", "isVerified", "remarks" }
            }
        },

        new()
        {
            Name = "get_all_documents",
            Description = "Gets all documents across all applications, optionally filtered by verification status.",
            Parameters = new GeminiSchema
            {
                Type = "OBJECT",
                Properties = new()
                {
                    ["status"] = new()
                    {
                        Type = "STRING",
                        Description = "Filter by document status. Leave empty for all.",
                        Enum = new() { "", "Pending", "UnderReview", "Verified", "ReuploadRequired" }
                    }
                },
                Required = new()
            }
        },

        new()
        {
            Name = "get_users_list",
            Description = "Gets the list of all registered users in the system with their roles and active status.",
            Parameters = new GeminiSchema { Type = "OBJECT", Properties = new(), Required = new() }
        }
    ];
}
