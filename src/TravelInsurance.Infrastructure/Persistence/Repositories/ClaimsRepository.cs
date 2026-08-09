using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Claims.Abstractions;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class ClaimsRepository(AppDbContext dbContext) : IClaimsRepository
{
    public Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking().Where(u => u.Id == userId && u.Status == UserStatus.Active)
            .Select(u => new WorkflowActor(u.Id, u.CompanyId, u.Company != null ? u.Company.Type : null, u.UserRoles.Any(ur => ur.Role!.Name == "SUPER_ADMIN")))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Policy?> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default) =>
        dbContext.Policies.Include(p => p.Plan).Include(p => p.Passengers).FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

    public Task<Claim?> GetClaimAsync(Guid claimId, CancellationToken cancellationToken = default) =>
        ClaimGraph(dbContext.Claims).FirstOrDefaultAsync(c => c.Id == claimId, cancellationToken);

    public Task<List<Claim>> GetClaimsAsync(Guid? insuranceCompanyId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Claims.AsNoTracking().Include(c => c.Policy).ThenInclude(p => p!.Plan).AsQueryable();
        if (insuranceCompanyId.HasValue) query = query.Where(c => c.Policy!.Plan!.CompanyId == insuranceCompanyId.Value);
        return query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public void AddClaim(Claim claim) => dbContext.Claims.Add(claim);
    public async Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); return WorkflowSaveResult.Saved; }
        catch (DbUpdateException) { return WorkflowSaveResult.Conflict; }
    }

    private static IQueryable<Claim> ClaimGraph(IQueryable<Claim> query) => query
        .Include(c => c.Policy).ThenInclude(p => p!.Plan)
        .Include(c => c.PolicyPassenger)
        .Include(c => c.Documents)
        .Include(c => c.Payments)
        .Include(c => c.StatusHistory);
}
