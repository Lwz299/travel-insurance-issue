using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface ICoverageZoneRepository
{
    Task<List<CoverageZone>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CoverageZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AddAsync(CoverageZone zone, CancellationToken cancellationToken = default);
    Task UpdateAsync(CoverageZone zone, CancellationToken cancellationToken = default);
}
