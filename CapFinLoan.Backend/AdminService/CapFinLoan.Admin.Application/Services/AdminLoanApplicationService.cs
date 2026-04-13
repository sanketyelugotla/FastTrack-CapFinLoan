using CapFinLoan.Admin.Application.Contracts.Requests;
using CapFinLoan.Admin.Application.Contracts.Responses;
using CapFinLoan.Admin.Application.Exceptions;
using CapFinLoan.Admin.Application.Interfaces;
using CapFinLoan.Admin.Domain.Constants;
using CapFinLoan.Admin.Domain.Entities;
using CapFinLoan.Messaging.Contracts.Events;

namespace CapFinLoan.Admin.Application.Services;

public class AdminLoanApplicationService : IAdminLoanApplicationService
{
    private static readonly HashSet<string> AllowedDecisionStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ApplicationStatuses.DocsPending,
        ApplicationStatuses.UnderReview,
        ApplicationStatuses.Approved,
        ApplicationStatuses.Rejected
    };

    private readonly IAdminLoanApplicationRepository _adminLoanApplicationRepository;
    private readonly IEventPublisher _eventPublisher;

    public AdminLoanApplicationService(IAdminLoanApplicationRepository adminLoanApplicationRepository, IEventPublisher eventPublisher)
    {
        _adminLoanApplicationRepository = adminLoanApplicationRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IReadOnlyCollection<AdminApplicationSummaryResponse>> GetQueueAsync(string? status, CancellationToken cancellationToken = default)
    {
        var applications = await _adminLoanApplicationRepository.GetQueueAsync(status, cancellationToken);
        return applications.Select(MapSummary).ToArray();
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var applications = await _adminLoanApplicationRepository.GetAllAsync(cancellationToken);
        return new AdminDashboardResponse
        {
            TotalApplications = applications.Count,
            SubmittedCount = CountByStatus(applications, ApplicationStatuses.Submitted),
            DocsPendingCount = CountByStatus(applications, ApplicationStatuses.DocsPending),
            UnderReviewCount = CountByStatus(applications, ApplicationStatuses.UnderReview),
            ApprovedCount = CountByStatus(applications, ApplicationStatuses.Approved),
            RejectedCount = CountByStatus(applications, ApplicationStatuses.Rejected)
        };
    }

    public async Task<AdminApplicationDetailResponse> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _adminLoanApplicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new AdminNotFoundException();

        return MapDetail(application);
    }

    public async Task<AdminApplicationDetailResponse> UpdateStatusAsync(Guid applicationId, Guid reviewerUserId, ReviewLoanApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var application = await _adminLoanApplicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new AdminNotFoundException();

        var targetStatus = NormalizeStatus(request.TargetStatus);
        ValidateTransition(application.Status, targetStatus, request.Remarks);

        var now = DateTime.UtcNow;
        var previousStatus = application.Status;

        application.Status = targetStatus;
        application.UpdatedAtUtc = now;
        application.StatusHistory.Add(new ApplicationStatusHistory
        {
            LoanApplicationId = application.Id,
            FromStatus = previousStatus,
            ToStatus = targetStatus,
            Remarks = request.Remarks.Trim(),
            ChangedByUserId = reviewerUserId,
            ChangedAtUtc = now
        });

        application.Decisions.Add(new Decision
        {
            LoanApplicationId = application.Id,
            AdminUserId = reviewerUserId,
            DecisionStatus = targetStatus,
            Remarks = request.Remarks.Trim(),
            SanctionAmount = string.Equals(targetStatus, ApplicationStatuses.Approved, StringComparison.OrdinalIgnoreCase)
                ? (request.SanctionAmount ?? application.RequestedAmount)
                : null,
            InterestRate = string.Equals(targetStatus, ApplicationStatuses.Approved, StringComparison.OrdinalIgnoreCase)
                ? request.InterestRate
                : null,
            DecisionAtUtc = now
        });

        await _adminLoanApplicationRepository.UpdateAsync(application, cancellationToken);

        await _eventPublisher.PublishAsync(new ApplicationStatusChangedEvent
        {
            ApplicationId = application.Id,
            ApplicantUserId = application.ApplicantUserId,
            ApplicationNumber = application.ApplicationNumber,
            PreviousStatus = previousStatus,
            NewStatus = targetStatus,
            Remarks = request.Remarks.Trim(),
            ChangedByUserId = reviewerUserId,
            ChangedAtUtc = now
        }, cancellationToken);

        return MapDetail(application);
    }

    private static void ValidateTransition(string currentStatus, string targetStatus, string remarks)
    {
        if (!AllowedDecisionStatuses.Contains(targetStatus))
        {
            throw new AdminConflictException("Target status is not supported by the admin workflow.");
        }

        if (string.Equals(currentStatus, ApplicationStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new AdminConflictException("Draft applications cannot be reviewed by admin.");
        }

        if (string.Equals(currentStatus, targetStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new AdminConflictException("Application is already in the requested status.");
        }

        if (string.Equals(targetStatus, ApplicationStatuses.Rejected, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(remarks))
        {
            throw new AdminValidationException("Remarks are required when rejecting an application.");
        }

        if (string.Equals(targetStatus, ApplicationStatuses.DocsPending, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(remarks))
        {
            throw new AdminValidationException("Remarks are required when requesting document re-upload.");
        }

        var allowedFromStatuses = targetStatus switch
        {
            ApplicationStatuses.DocsPending => new[] { ApplicationStatuses.Submitted, ApplicationStatuses.DocsVerified, ApplicationStatuses.UnderReview },
            ApplicationStatuses.UnderReview => new[] { ApplicationStatuses.Submitted, ApplicationStatuses.DocsPending, ApplicationStatuses.DocsVerified },
            ApplicationStatuses.Approved => new[] { ApplicationStatuses.Submitted, ApplicationStatuses.DocsPending, ApplicationStatuses.DocsVerified, ApplicationStatuses.UnderReview },
            ApplicationStatuses.Rejected => new[] { ApplicationStatuses.Submitted, ApplicationStatuses.DocsPending, ApplicationStatuses.DocsVerified, ApplicationStatuses.UnderReview },
            _ => Array.Empty<string>()
        };

        if (!allowedFromStatuses.Contains(currentStatus, StringComparer.OrdinalIgnoreCase))
        {
            throw new AdminConflictException($"Status cannot be changed from {currentStatus} to {targetStatus}.");
        }
    }

    private static string NormalizeStatus(string status)
    {
        var compact = status.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty);

        return compact switch
        {
            var value when value.Equals("DocsPending", StringComparison.OrdinalIgnoreCase) => ApplicationStatuses.DocsPending,
            var value when value.Equals("DocsVerified", StringComparison.OrdinalIgnoreCase) => ApplicationStatuses.DocsVerified,
            var value when value.Equals("UnderReview", StringComparison.OrdinalIgnoreCase) => ApplicationStatuses.UnderReview,
            var value when value.Equals("Approved", StringComparison.OrdinalIgnoreCase) => ApplicationStatuses.Approved,
            var value when value.Equals("Rejected", StringComparison.OrdinalIgnoreCase) => ApplicationStatuses.Rejected,
            _ => status.Trim()
        };
    }

    private static int CountByStatus(IEnumerable<LoanApplication> applications, string status)
    {
        return applications.Count(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase));
    }

    private static AdminApplicationSummaryResponse MapSummary(LoanApplication application)
    {
        return new AdminApplicationSummaryResponse
        {
            Id = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            ApplicantUserId = application.ApplicantUserId,
            ApplicantName = $"{application.FirstName} {application.LastName}".Trim(),
            Email = application.Email,
            Phone = application.Phone,
            RequestedAmount = application.RequestedAmount,
            RequestedTenureMonths = application.RequestedTenureMonths,
            Status = application.Status,
            CreatedAtUtc = application.CreatedAtUtc,
            UpdatedAtUtc = application.UpdatedAtUtc,
            SubmittedAtUtc = application.SubmittedAtUtc
        };
    }

    private static AdminApplicationDetailResponse MapDetail(LoanApplication application)
    {
        return new AdminApplicationDetailResponse
        {
            Id = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            ApplicantUserId = application.ApplicantUserId,
            Status = application.Status,
            FirstName = application.FirstName,
            LastName = application.LastName,
            DateOfBirth = application.DateOfBirth,
            Gender = application.Gender,
            Email = application.Email,
            Phone = application.Phone,
            AddressLine1 = application.AddressLine1,
            AddressLine2 = application.AddressLine2,
            City = application.City,
            State = application.State,
            PostalCode = application.PostalCode,
            EmployerName = application.EmployerName,
            EmploymentType = application.EmploymentType,
            MonthlyIncome = application.MonthlyIncome,
            AnnualIncome = application.AnnualIncome,
            ExistingEmiAmount = application.ExistingEmiAmount,
            RequestedAmount = application.RequestedAmount,
            RequestedTenureMonths = application.RequestedTenureMonths,
            LoanPurpose = application.LoanPurpose,
            Remarks = application.Remarks,
            CreatedAtUtc = application.CreatedAtUtc,
            UpdatedAtUtc = application.UpdatedAtUtc,
            SubmittedAtUtc = application.SubmittedAtUtc,
            Timeline = application.StatusHistory
                .OrderByDescending(x => x.ChangedAtUtc)
                .Select(x => new AdminApplicationStatusHistoryResponse
                {
                    Status = x.ToStatus,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Remarks = x.Remarks,
                    ChangedByUserId = x.ChangedByUserId,
                    CreatedAtUtc = x.ChangedAtUtc,
                    ChangedAtUtc = x.ChangedAtUtc
                })
                .ToArray()
        };
    }
}