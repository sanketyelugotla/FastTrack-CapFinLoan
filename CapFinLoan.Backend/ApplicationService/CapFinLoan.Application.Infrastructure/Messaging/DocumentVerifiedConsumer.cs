using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Domain.Constants;
using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CapFinLoan.Application.Infrastructure.Messaging;

public class DocumentVerifiedConsumer : IConsumer<DocumentVerifiedEvent>
{
    private readonly ILoanApplicationRepository _repository;
    private readonly ILogger<DocumentVerifiedConsumer> _logger;

    public DocumentVerifiedConsumer(ILoanApplicationRepository repository, ILogger<DocumentVerifiedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentVerifiedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing DocumentVerifiedEvent for Application {ApplicationId}", message.ApplicationId);

        var application = await _repository.GetByIdAsync(message.ApplicationId);
        if (application != null && message.IsVerified && application.Status == ApplicationStatuses.DocsPending)
        {
            application.Status = ApplicationStatuses.DocsVerified;
            application.UpdatedAtUtc = DateTime.UtcNow;

            application.StatusHistory.Add(new CapFinLoan.Application.Domain.Entities.ApplicationStatusHistory
            {
                LoanApplicationId = application.Id,
                FromStatus = ApplicationStatuses.DocsPending,
                ToStatus = ApplicationStatuses.DocsVerified,
                Remarks = $"System: Document {message.FileName} verified by admin.",
                ChangedByUserId = message.VerifiedByUserId,
                ChangedAtUtc = DateTime.UtcNow
            });

            await _repository.UpdateAsync(application);
            _logger.LogInformation("Application {ApplicationId} promoted to Docs Verified.", application.Id);
        }
    }
}
