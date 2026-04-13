using CapFinLoan.Application.Domain.Constants;
using CapFinLoan.Application.Persistence.Data;
using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Application.Infrastructure.Messaging;

/// SAGA PARTICIPANT: Consumes status changes from AdminService and syncs them
/// into the ApplicationService's own database.
/// On success → publishes LoanApprovedEvent / LoanRejectedEvent
/// On failure → publishes StatusSyncFailedEvent (triggers compensation in AdminService)
public class ApplicationStatusChangedConsumer : IConsumer<ApplicationStatusChangedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ApplicationStatusChangedConsumer(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ApplicationStatusChangedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[SAGA] ApplicationService received status change: {message.ApplicationNumber} → {message.NewStatus}");

        try
        {
            // Step 1: Find the application in our own database
            var application = await _dbContext.LoanApplications
                .FirstOrDefaultAsync(x => x.Id == message.ApplicationId);

            if (application == null)
            {
                Console.WriteLine($"[SAGA] WARNING: Application {message.ApplicationId} not found in ApplicationDb. Publishing StatusSyncFailedEvent.");
                await _publishEndpoint.Publish(new StatusSyncFailedEvent
                {
                    ApplicationId = message.ApplicationId,
                    ApplicantUserId = message.ApplicantUserId,
                    ApplicationNumber = message.ApplicationNumber,
                    AttemptedStatus = message.NewStatus,
                    PreviousStatus = message.PreviousStatus,
                    FailureReason = $"Application {message.ApplicationId} not found in ApplicationService database.",
                    FailedAtUtc = DateTime.UtcNow
                });
                return;
            }

            // Step 2: Update the status in our own database
            var previousStatus = application.Status;
            application.Status = message.NewStatus;
            application.UpdatedAtUtc = DateTime.UtcNow;
            application.StatusHistory.Add(new Domain.Entities.ApplicationStatusHistory
            {
                LoanApplicationId = application.Id,
                FromStatus = previousStatus,
                ToStatus = message.NewStatus,
                Remarks = $"Status synced from AdminService. {message.Remarks}".Trim(),
                ChangedByUserId = message.ChangedByUserId,
                ChangedAtUtc = message.ChangedAtUtc
            });

            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[SAGA] Successfully synced status to ApplicationDb: {message.ApplicationNumber} → {message.NewStatus}");

            // Step 3: Publish domain events based on the new status
            if (string.Equals(message.NewStatus, ApplicationStatuses.Approved, StringComparison.OrdinalIgnoreCase))
            {
                await _publishEndpoint.Publish(new LoanApprovedEvent
                {
                    ApplicationId = application.Id,
                    ApplicantUserId = application.ApplicantUserId,
                    ApplicationNumber = application.ApplicationNumber,
                    ApplicantName = $"{application.FirstName} {application.LastName}".Trim(),
                    Email = application.Email,
                    RequestedAmount = application.RequestedAmount,
                    SanctionAmount = application.RequestedAmount, // Could be different in future
                    RequestedTenureMonths = application.RequestedTenureMonths,
                    Remarks = message.Remarks,
                    ApprovedByUserId = message.ChangedByUserId,
                    ApprovedAtUtc = message.ChangedAtUtc
                });
                Console.WriteLine($"[SAGA] Published LoanApprovedEvent for {message.ApplicationNumber}");
            }
            else if (string.Equals(message.NewStatus, ApplicationStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                await _publishEndpoint.Publish(new LoanRejectedEvent
                {
                    ApplicationId = application.Id,
                    ApplicantUserId = application.ApplicantUserId,
                    ApplicationNumber = application.ApplicationNumber,
                    ApplicantName = $"{application.FirstName} {application.LastName}".Trim(),
                    Email = application.Email,
                    RequestedAmount = application.RequestedAmount,
                    Remarks = message.Remarks,
                    RejectedByUserId = message.ChangedByUserId,
                    RejectedAtUtc = message.ChangedAtUtc
                });
                Console.WriteLine($"[SAGA] Published LoanRejectedEvent for {message.ApplicationNumber}");
            }
        }
        catch (Exception ex)
        {
            // SAGA COMPENSATION: If we fail to sync, tell AdminService to roll back
            Console.WriteLine($"[SAGA] FAILURE syncing status for {message.ApplicationNumber}: {ex.Message}");
            await _publishEndpoint.Publish(new StatusSyncFailedEvent
            {
                ApplicationId = message.ApplicationId,
                ApplicantUserId = message.ApplicantUserId,
                ApplicationNumber = message.ApplicationNumber,
                AttemptedStatus = message.NewStatus,
                PreviousStatus = message.PreviousStatus,
                FailureReason = ex.Message,
                FailedAtUtc = DateTime.UtcNow
            });
            Console.WriteLine($"[SAGA] Published StatusSyncFailedEvent → AdminService will compensate");
        }
    }
}
