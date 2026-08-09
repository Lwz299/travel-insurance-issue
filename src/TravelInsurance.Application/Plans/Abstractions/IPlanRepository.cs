using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface IPlanRepository
{
    Task<List<InsurancePlan>> GetAllAsync(Guid? companyId, bool? isActive, CancellationToken cancellationToken = default);
    Task<InsurancePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InsurancePlan?> GetByIdWithGraphAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(InsurancePlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(InsurancePlan plan, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICountryRepository
{
    Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
