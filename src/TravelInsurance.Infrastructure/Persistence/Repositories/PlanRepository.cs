using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class PlanRepository(AppDbContext dbContext) : IPlanRepository
{
    public Task<List<InsurancePlan>> GetAllAsync(Guid? companyId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Plans.AsNoTracking().Include(p => p.Company).AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        return query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<InsurancePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<InsurancePlan?> GetByIdWithGraphAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Plans
            .Include(p => p.Company)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.Coverage).ThenInclude(c => c!.Group)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.BenefitOverrides).ThenInclude(o => o.Benefit)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.BenefitOverrides).ThenInclude(o => o.LimitType)
            .Include(p => p.Countries).ThenInclude(c => c.Country)
            .Include(p => p.CoverageZones).ThenInclude(z => z.CoverageZone).ThenInclude(z => z!.Countries).ThenInclude(c => c.Country)
            .Include(p => p.PricingRules)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(InsurancePlan plan, CancellationToken cancellationToken = default)
    {
        dbContext.Plans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(InsurancePlan plan, CancellationToken cancellationToken = default)
    {
        plan.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
