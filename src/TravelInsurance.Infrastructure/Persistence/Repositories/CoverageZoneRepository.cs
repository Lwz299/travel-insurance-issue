using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class CoverageZoneRepository(AppDbContext dbContext) : ICoverageZoneRepository
{
    public Task<List<CoverageZone>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.CoverageZones.AsNoTracking().OrderBy(z => z.Name).ToListAsync(cancellationToken);

    public Task<CoverageZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.CoverageZones.Include(z => z.Countries).ThenInclude(c => c.Country).FirstOrDefaultAsync(z => z.Id == id, cancellationToken);

    public async Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        return (await dbContext.CoverageZones.AsNoTracking().Where(z => idList.Contains(z.Id) && z.IsActive).Select(z => z.Id).ToListAsync(cancellationToken)).ToHashSet();
    }

    public async Task AddAsync(CoverageZone zone, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageZones.Add(zone);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(CoverageZone zone, CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
