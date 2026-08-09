using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Reinsurance.DTOs;

namespace TravelInsurance.Application.Reinsurance.Abstractions;

public interface IReinsuranceAgreementService
{
    Task<IReadOnlyCollection<ReinsuranceAgreementResponse>> GetAllAsync(Guid? companyId, CancellationToken cancellationToken = default);
    Task<ReinsuranceAgreementResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(ReinsuranceAgreementResponse? Agreement, OperationResponse Result)> CreateAsync(CreateReinsuranceAgreementRequest request, CancellationToken cancellationToken = default);
    Task<(ReinsuranceAgreementResponse? Agreement, OperationResponse Result)> UpdateAsync(Guid id, UpdateReinsuranceAgreementRequest request, CancellationToken cancellationToken = default);
}
