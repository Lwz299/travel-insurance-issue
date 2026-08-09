using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface ICoverageZoneService
{
    Task<IReadOnlyCollection<CoverageZoneSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CoverageZoneResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(CoverageZoneResponse? Zone, OperationResponse Result)> CreateAsync(CreateCoverageZoneRequest request, CancellationToken cancellationToken = default);
    Task<(CoverageZoneResponse? Zone, OperationResponse Result)> UpdateAsync(Guid id, UpdateCoverageZoneRequest request, CancellationToken cancellationToken = default);
}
