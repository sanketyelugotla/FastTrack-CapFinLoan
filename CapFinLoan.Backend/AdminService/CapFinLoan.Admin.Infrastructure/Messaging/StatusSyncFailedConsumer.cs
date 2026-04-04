using CapFinLoan.Admin.Domain.Entities;
using CapFinLoan.Admin.Persistence.Data;
using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Admin.Infrastructure.Messaging;

/// <summary>
/// SAGA COMPENSATOR: Consumes StatusSyncFailedEvent from ApplicationService.
/// When ApplicationService fails to sync a status change, this consumer
/// rolls the AdminService's loan application back to the previous status.
/// This is the core compensating transaction of the Saga pattern.
/// </summary>
public class StatusSyncFailedConsumer : IConsumer<StatusSyncFailedEvent>
{
    private readonly AdminDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public StatusSyncFailedConsumer(AdminDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<StatusSyncFailedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[SAGA COMPENSATOR] AdminService received StatusSyncFailedEvent for {message.ApplicationNumber}");
        Console.WriteLine($"  Attempted: {message.AttemptedStatus}, Rolling back to: {message.PreviousStatus}");
        Console.WriteLine($"  Reason: {message.FailureReason}");

        try
        {
            var application = await _dbContext.LoanApplications
                .Include(x => x.StatusHistory)
                .Include(x => x.Decisions)
                .FirstOrDefaultAsync(x => x.Id == message.ApplicationId);

            if (application == null)
            {
                Console.WriteLine($"[SAGA COMPENSATOR] Application {message.ApplicationId} not found in AdminDb — cannot compensate.");
                return;
            }

            // Only rollback if the application is still in the attempted status
            if (!string.Equals(application.Status, message.AttemptedStatus, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[SAGA COMPENSATOR] Application status is '{application.Status}', not '{message.AttemptedStatus}' — skipping rollback (already changed).");
                return;
            }

            var now = DateTime.UtcNow;

            // COMPENSATING TRANSACTION: Roll back the status
            application.Status = message.PreviousStatus;
            application.UpdatedAtUtc = now;
            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                LoanApplicationId = application.Id,
                FromStatus = message.AttemptedStatus,
                ToStatus = message.PreviousStatus,
                Remarks = $"[SAGA ROLLBACK] Auto-rollback triggered. ApplicationService sync failed: {message.FailureReason}",
                ChangedByUserId = Guid.Empty, // System action
                ChangedAtUtc = now
            });

            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[SAGA COMPENSATOR] Successfully rolled back {message.ApplicationNumber}: {message.AttemptedStatus} → {message.PreviousStatus}");

            // Notify other services about the rollback
            await _publishEndpoint.Publish(new ApplicationStatusChangedEvent
            {
                ApplicationId = application.Id,
                ApplicantUserId = application.ApplicantUserId,
                ApplicationNumber = application.ApplicationNumber,
                PreviousStatus = message.AttemptedStatus,
                NewStatus = message.PreviousStatus,
                Remarks = $"[SAGA ROLLBACK] Status reverted due to sync failure.",
                ChangedByUserId = Guid.Empty,
                ChangedAtUtc = now
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SAGA COMPENSATOR] CRITICAL: Failed to compensate for {message.ApplicationNumber}: {ex.Message}");
            // In production, this would alert the ops team via PagerDuty/Slack
        }
    }
}
