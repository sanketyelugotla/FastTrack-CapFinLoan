namespace CapFinLoan.Document.Domain.Constants;

public enum DocumentStatus
{
    // Uploaded, awaiting admin review.
    Pending,

    // Admin is actively reviewing the document.
    UnderReview,

    // Admin has approved the document.
    Verified,

    // Admin has rejected and requested a fresh upload.
    ReuploadRequired
}
