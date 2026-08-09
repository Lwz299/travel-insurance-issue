using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface IPlanCoverageRepository
{
    Task<InsurancePlan?> GetPlanWithCoveragesAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<PlanCoverageAssignment?> GetAssignmentAsync(Guid planId, Guid coverageId, CancellationToken cancellationToken = default);
    Task<PlanCoverageAssignment?> GetAssignmentWithOverridesAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    Task<bool> PlanExistsAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<bool> CoverageExistsAndActiveAsync(Guid coverageId, CancellationToken cancellationToken = default);
    Task<bool> AssignmentExistsAsync(Guid planId, Guid coverageId, CancellationToken cancellationToken = default);
    Task AddAssignmentAsync(PlanCoverageAssignment assignment, CancellationToken cancellationToken = default);
    Task RemoveAssignmentAsync(PlanCoverageAssignment assignment, CancellationToken cancellationToken = default);
    Task<PlanCoverageBenefitOverride?> GetOverrideAsync(Guid assignmentId, Guid benefitId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
