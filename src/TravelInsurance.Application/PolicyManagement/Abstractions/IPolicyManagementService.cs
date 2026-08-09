using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.PolicyManagement.DTOs;

namespace TravelInsurance.Application.PolicyManagement.Abstractions;

public interface IPolicyManagementService
{
    Task<(PolicyResponse? Policy, OperationResponse Result)> GetByIdAsync(Guid policyId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PolicySummaryResponse>> GetAllAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> CancelAsync(Guid policyId, CancelPolicyRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> ExpireAsync(Guid policyId, Guid actorUserId, CancellationToken cancellationToken = default);
}
