using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface IPlanService
{
    Task<IReadOnlyCollection<PlanSummaryResponse>> GetAllAsync(Guid? companyId, bool? isActive, CancellationToken cancellationToken = default);
    Task<PlanResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(PlanResponse? Plan, OperationResponse Result)> CreateAsync(CreatePlanRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<(PlanResponse? Plan, OperationResponse Result)> UpdateAsync(Guid id, UpdatePlanRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> SetStatusAsync(Guid id, bool isActive, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
