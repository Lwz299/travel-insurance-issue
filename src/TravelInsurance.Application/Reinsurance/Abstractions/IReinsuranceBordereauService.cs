using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Reinsurance.DTOs;

namespace TravelInsurance.Application.Reinsurance.Abstractions;

public interface IReinsuranceBordereauService
{
    Task<ReinsuranceBordereauResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(ReinsuranceBordereauResponse? Bordereau, OperationResponse Result)> GenerateAsync(GenerateBordereauRequest request, CancellationToken cancellationToken = default);
}
