using CapFinLoan.Admin.Domain.Constants;
using CapFinLoan.Admin.Domain.Entities;
using CapFinLoan.Admin.Persistence.Data;
using CapFinLoan.Messaging.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CapFinLoan.Admin.Infrastructure.Messaging;

// SAGA DATA SYNC: When a user submits a loan application in ApplicationService,
// this consumer creates a corresponding record in the AdminService's own database.
// This is essential for the database-per-service architecture — the AdminService
// cannot directly query ApplicationService's database.
public class ApplicationSubmittedConsumer : IConsumer<ApplicationSubmittedEvent>
{
    private readonly AdminDbContext _dbContext;

    public ApplicationSubmittedConsumer(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ApplicationSubmittedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"[SAGA SYNC] AdminService received ApplicationSubmittedEvent: {message.ApplicationNumber}");

        try
        {
            // Check if we already have this application (idempotency guard)
            var exists = await _dbContext.LoanApplications
                .AnyAsync(x => x.Id == message.ApplicationId);

            if (exists)
            {
                Console.WriteLine($"[SAGA SYNC] Application {message.ApplicationNumber} already exists in AdminDb — skipping duplicate.");
                return;
            }

            // Parse the applicant name into first/last
            var nameParts = message.ApplicantName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var application = new LoanApplication
            {
                Id = message.ApplicationId,
                ApplicantUserId = message.ApplicantUserId,
                ApplicationNumber = message.ApplicationNumber,
                Status = ApplicationStatuses.Submitted,
                FirstName = firstName,
                LastName = lastName,
                Email = message.Email,
                RequestedAmount = message.RequestedAmount,
                RequestedTenureMonths = message.RequestedTenureMonths,
                CreatedAtUtc = message.SubmittedAtUtc,
                UpdatedAtUtc = message.SubmittedAtUtc,
                SubmittedAtUtc = message.SubmittedAtUtc
            };

            application.StatusHistory.Add(new ApplicationStatusHistory
            {
                LoanApplicationId = application.Id,
                FromStatus = ApplicationStatuses.Draft,
                ToStatus = ApplicationStatuses.Submitted,
                Remarks = "Application submitted by applicant. Synced to AdminService via RabbitMQ.",
                ChangedByUserId = message.ApplicantUserId,
                ChangedAtUtc = message.SubmittedAtUtc
            });

            _dbContext.LoanApplications.Add(application);
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"[SAGA SYNC] Successfully created {message.ApplicationNumber} in AdminDb.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SAGA SYNC] FAILED to sync {message.ApplicationNumber} to AdminDb: {ex.Message}");
            // MassTransit will automatically retry via its retry policy
            throw;
        }
    }
}
