using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class PlanCoverageRepository(AppDbContext dbContext) : IPlanCoverageRepository
{
    public Task<InsurancePlan?> GetPlanWithCoveragesAsync(Guid planId, CancellationToken cancellationToken = default) =>
        dbContext.Plans
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.Coverage).ThenInclude(c => c!.Group)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.BenefitOverrides).ThenInclude(o => o.Benefit)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.BenefitOverrides).ThenInclude(o => o.LimitType)
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

    public Task<PlanCoverageAssignment?> GetAssignmentAsync(Guid planId, Guid coverageId, CancellationToken cancellationToken = default) =>
        dbContext.PlanCoverageAssignments
            .Include(a => a.BenefitOverrides).ThenInclude(o => o.Benefit)
            .Include(a => a.BenefitOverrides).ThenInclude(o => o.LimitType)
            .Include(a => a.Coverage).ThenInclude(c => c!.Group)
            .FirstOrDefaultAsync(x => x.PlanId == planId && x.CoverageId == coverageId, cancellationToken);

    public Task<PlanCoverageAssignment?> GetAssignmentWithOverridesAsync(Guid assignmentId, CancellationToken cancellationToken = default) =>
        dbContext.PlanCoverageAssignments
            .Include(a => a.BenefitOverrides)
            .FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);

    public Task<bool> PlanExistsAsync(Guid planId, CancellationToken cancellationToken = default) =>
        dbContext.Plans.AnyAsync(x => x.Id == planId, cancellationToken);

    public Task<bool> CoverageExistsAndActiveAsync(Guid coverageId, CancellationToken cancellationToken = default) =>
        dbContext.Coverages.AnyAsync(x => x.Id == coverageId && x.IsActive, cancellationToken);

    public Task<bool> AssignmentExistsAsync(Guid planId, Guid coverageId, CancellationToken cancellationToken = default) =>
        dbContext.PlanCoverageAssignments.AnyAsync(x => x.PlanId == planId && x.CoverageId == coverageId, cancellationToken);

    public async Task AddAssignmentAsync(PlanCoverageAssignment assignment, CancellationToken cancellationToken = default)
    {
        dbContext.PlanCoverageAssignments.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAssignmentAsync(PlanCoverageAssignment assignment, CancellationToken cancellationToken = default)
    {
        dbContext.PlanCoverageAssignments.Remove(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PlanCoverageBenefitOverride?> GetOverrideAsync(Guid assignmentId, Guid benefitId, CancellationToken cancellationToken = default) =>
        dbContext.PlanCoverageBenefitOverrides.FirstOrDefaultAsync(x => x.PlanCoverageId == assignmentId && x.BenefitId == benefitId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
