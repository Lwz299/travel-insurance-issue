using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class CountryRepository(AppDbContext dbContext) : ICountryRepository
{
    public Task<List<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Countries.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        var found = await dbContext.Countries
            .AsNoTracking()
            .Where(c => idList.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        return found.ToHashSet();
    }
}
