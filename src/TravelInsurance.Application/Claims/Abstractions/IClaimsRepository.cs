using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Claims.Abstractions;

public interface IClaimsRepository
{
    Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Policy?> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<Claim?> GetClaimAsync(Guid claimId, CancellationToken cancellationToken = default);
    Task<List<Claim>> GetClaimsAsync(Guid? insuranceCompanyId, CancellationToken cancellationToken = default);
    void AddClaim(Claim claim);
    Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
