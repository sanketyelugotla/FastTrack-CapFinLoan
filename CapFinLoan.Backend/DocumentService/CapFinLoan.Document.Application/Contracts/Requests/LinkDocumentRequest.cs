using System.ComponentModel.DataAnnotations;

namespace CapFinLoan.Document.Application.Contracts.Requests;

public class LinkDocumentRequest
{
    [Required]
    public Guid ApplicationId { get; set; }
}
