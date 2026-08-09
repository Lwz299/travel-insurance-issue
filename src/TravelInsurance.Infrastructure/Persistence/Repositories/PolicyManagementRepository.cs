using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.PolicyManagement.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class PolicyManagementRepository(AppDbContext dbContext) : IPolicyManagementRepository
{
    public Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking().Where(u => u.Id == userId && u.Status == UserStatus.Active)
            .Select(u => new WorkflowActor(u.Id, u.CompanyId, u.Company != null ? u.Company.Type : null, u.UserRoles.Any(ur => ur.Role!.Name == "SUPER_ADMIN")))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Policy?> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default) =>
        dbContext.Policies.Include(p => p.Plan).Include(p => p.Passengers)
            .Include(p => p.CoverageSnapshots).ThenInclude(s => s.BenefitSnapshots)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

    public Task<List<Policy>> GetPoliciesAsync(Guid? companyId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policies.AsNoTracking().Include(p => p.Plan).AsQueryable();
        if (companyId.HasValue) query = query.Where(p => p.AgencyCompanyId == companyId || p.Plan!.CompanyId == companyId);
        return query.OrderByDescending(p => p.IssuedAt).ToListAsync(cancellationToken);
    }

    public async Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); return WorkflowSaveResult.Saved; }
        catch (DbUpdateException) { return WorkflowSaveResult.Conflict; }
    }
}
