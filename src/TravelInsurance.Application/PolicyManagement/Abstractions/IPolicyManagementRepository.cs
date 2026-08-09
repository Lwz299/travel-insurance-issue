using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.PolicyManagement.Abstractions;

public interface IPolicyManagementRepository
{
    Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Policy?> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<List<Policy>> GetPoliciesAsync(Guid? companyId, CancellationToken cancellationToken = default);
    Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
