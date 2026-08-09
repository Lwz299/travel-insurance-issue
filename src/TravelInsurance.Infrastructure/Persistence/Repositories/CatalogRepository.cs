using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class CatalogRepository(AppDbContext dbContext) : ICatalogRepository
{
    public Task<List<BenefitType>> GetBenefitTypesAsync(CancellationToken cancellationToken = default) =>
        dbContext.BenefitTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<List<LimitType>> GetLimitTypesAsync(CancellationToken cancellationToken = default) =>
        dbContext.LimitTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<List<RiskType>> GetRiskTypesAsync(CancellationToken cancellationToken = default) =>
        dbContext.RiskTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<bool> BenefitTypeExistsAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.BenefitTypes.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public Task<bool> LimitTypeExistsAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.LimitTypes.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public Task<bool> RiskTypeExistsAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.RiskTypes.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public Task<List<CoverageGroup>> GetGroupsAsync(CancellationToken cancellationToken = default) =>
        dbContext.CoverageGroups.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

    public Task<CoverageGroup?> GetGroupByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.CoverageGroups.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> GroupCodeExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken = default) =>
        dbContext.CoverageGroups.AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId), cancellationToken);

    public async Task AddGroupAsync(CoverageGroup group, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageGroups.Add(group);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateGroupAsync(CoverageGroup group, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageGroups.Update(group);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGroupAsync(CoverageGroup group, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageGroups.Remove(group);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> GroupHasCoveragesAsync(Guid groupId, CancellationToken cancellationToken = default) =>
        dbContext.Coverages.AnyAsync(x => x.GroupId == groupId, cancellationToken);

    public Task<List<Coverage>> GetCoveragesAsync(Guid? groupId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Coverages.AsNoTracking().Include(x => x.Group).Include(x => x.RiskType).AsQueryable();
        if (groupId.HasValue) query = query.Where(x => x.GroupId == groupId.Value);
        return query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<Coverage?> GetCoverageByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Coverages.Include(x => x.Group).Include(x => x.RiskType).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Coverage?> GetCoverageWithBenefitsAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Coverages
            .Include(x => x.Group).Include(x => x.RiskType)
            .Include(x => x.CoverageBenefits).ThenInclude(m => m.Benefit)
            .Include(x => x.CoverageBenefits).ThenInclude(m => m.LimitType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> CoverageCodeExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken = default) =>
        dbContext.Coverages.AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId), cancellationToken);

    public async Task AddCoverageAsync(Coverage coverage, CancellationToken cancellationToken = default)
    {
        dbContext.Coverages.Add(coverage);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCoverageAsync(Coverage coverage, CancellationToken cancellationToken = default)
    {
        dbContext.Coverages.Update(coverage);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CoverageIsAssignedToPlansAsync(Guid coverageId, CancellationToken cancellationToken = default) =>
        dbContext.PlanCoverageAssignments.AnyAsync(x => x.CoverageId == coverageId, cancellationToken);

    public Task<List<Benefit>> GetBenefitsAsync(CancellationToken cancellationToken = default) =>
        dbContext.Benefits.AsNoTracking().Include(x => x.BenefitType).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Benefit?> GetBenefitByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Benefits.Include(x => x.BenefitType).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> BenefitCodeExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken = default) =>
        dbContext.Benefits.AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId), cancellationToken);

    public async Task AddBenefitAsync(Benefit benefit, CancellationToken cancellationToken = default)
    {
        dbContext.Benefits.Add(benefit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBenefitAsync(Benefit benefit, CancellationToken cancellationToken = default)
    {
        dbContext.Benefits.Update(benefit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> BenefitIsInUseAsync(Guid benefitId, CancellationToken cancellationToken = default) =>
        await dbContext.CoverageBenefits.AnyAsync(x => x.BenefitId == benefitId, cancellationToken)
        || await dbContext.PlanCoverageBenefitOverrides.AnyAsync(x => x.BenefitId == benefitId, cancellationToken);

    public Task<CoverageBenefit?> GetCoverageBenefitAsync(Guid coverageId, Guid benefitId, CancellationToken cancellationToken = default) =>
        dbContext.CoverageBenefits.FirstOrDefaultAsync(x => x.CoverageId == coverageId && x.BenefitId == benefitId, cancellationToken);

    public async Task AddCoverageBenefitAsync(CoverageBenefit mapping, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageBenefits.Add(mapping);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCoverageBenefitAsync(CoverageBenefit mapping, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageBenefits.Update(mapping);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveCoverageBenefitAsync(CoverageBenefit mapping, CancellationToken cancellationToken = default)
    {
        dbContext.CoverageBenefits.Remove(mapping);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
