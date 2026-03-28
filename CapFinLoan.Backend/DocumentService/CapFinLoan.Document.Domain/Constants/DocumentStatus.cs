namespace CapFinLoan.Document.Domain.Constants;

public enum DocumentStatus
{
    /// <summary>Uploaded, awaiting admin review.</summary>
    Pending,

    /// <summary>Admin is actively reviewing the document.</summary>
    UnderReview,

    /// <summary>Admin has approved the document.</summary>
    Verified,

    /// <summary>Admin has rejected and requested a fresh upload.</summary>
    ReuploadRequired
}
