using TravelInsurance.Application.Reinsurance.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Reinsurance.Abstractions;

public interface IReinsuranceCessionService
{
    Task CreateForIssuedPolicyAsync(Policy policy, Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReinsuranceCessionResponse>> GetAllAsync(Guid? policyId, Guid? agreementId, CancellationToken cancellationToken = default);
}
