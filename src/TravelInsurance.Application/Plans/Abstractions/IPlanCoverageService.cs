using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface IPlanCoverageService
{
    Task<IReadOnlyCollection<PlanCoverageAssignmentResponse>> GetByPlanAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<OperationResponse> AssignAsync(Guid planId, AssignPlanCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> UpdateAsync(Guid planId, Guid coverageId, UpdatePlanCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> RemoveAsync(Guid planId, Guid coverageId, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> UpsertBenefitOverrideAsync(Guid planId, Guid coverageId, UpsertPlanBenefitOverrideRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
    Task<OperationResponse> RemoveBenefitOverrideAsync(Guid planId, Guid coverageId, Guid benefitId, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default);
}
