using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class ReinsuranceRepository(AppDbContext dbContext) : IReinsuranceRepository
{
    public Task<List<ReinsuranceAgreement>> GetAgreementsAsync(Guid? companyId, CancellationToken cancellationToken = default)
    {
        var query = AgreementGraph(dbContext.ReinsuranceAgreements).AsNoTracking();
        if (companyId.HasValue) query = query.Where(a => a.CompanyId == companyId.Value);
        return query.OrderBy(a => a.CompanyId).ThenBy(a => a.Priority).ThenBy(a => a.AgreementNumber).ToListAsync(cancellationToken);
    }

    public Task<ReinsuranceAgreement?> GetAgreementAsync(Guid id, CancellationToken cancellationToken = default) =>
        AgreementGraph(dbContext.ReinsuranceAgreements).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<ReinsuranceAgreement>> GetAutomaticAgreementsForPlanAsync(Guid companyId, Guid planId, CancellationToken cancellationToken = default) =>
        AgreementGraph(dbContext.ReinsuranceAgreements)
            .Where(a => a.CompanyId == companyId && a.IsActive && a.Type == ReinsuranceAgreementType.Treaty && !a.RequiresPreApproval && a.PlanAgreements.Any(p => p.PlanId == planId))
            .OrderBy(a => a.Priority).ThenByDescending(a => a.EffectiveFrom)
            .ToListAsync(cancellationToken);

    public Task<bool> HasCessionForPolicyAsync(Guid policyId, CancellationToken cancellationToken = default) =>
        dbContext.ReinsuranceCessions.AnyAsync(c => c.PolicyId == policyId, cancellationToken);

    public Task<List<ReinsuranceCession>> GetCessionsAsync(Guid? policyId, Guid? agreementId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReinsuranceCessions.AsNoTracking().AsQueryable();
        if (policyId.HasValue) query = query.Where(c => c.PolicyId == policyId.Value);
        if (agreementId.HasValue) query = query.Where(c => c.ReinsuranceAgreementId == agreementId.Value);
        return query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<ReinsuranceBordereauBatch?> GetBordereauAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ReinsuranceBordereauBatches.AsNoTracking().Include(b => b.Cessions).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<ReinsuranceCession>> GetPendingBordereauCessionsAsync(Guid agreementId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return dbContext.ReinsuranceCessions
            .Where(c => c.ReinsuranceAgreementId == agreementId && c.Status == ReinsuranceCessionStatus.PendingBordereau && c.CreatedAt >= start && c.CreatedAt < end)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasBordereauForPeriodAsync(Guid agreementId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        dbContext.ReinsuranceBordereauBatches.AnyAsync(b => b.ReinsuranceAgreementId == agreementId && b.PeriodFrom == from && b.PeriodTo == to, cancellationToken);

    public void AddAgreement(ReinsuranceAgreement agreement) => dbContext.ReinsuranceAgreements.Add(agreement);
    public void AddCession(ReinsuranceCession cession) => dbContext.ReinsuranceCessions.Add(cession);
    public void AddBordereau(ReinsuranceBordereauBatch batch) => dbContext.ReinsuranceBordereauBatches.Add(batch);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<ReinsuranceAgreement> AgreementGraph(IQueryable<ReinsuranceAgreement> query) =>
        query.Include(a => a.PlanAgreements)
            .Include(a => a.CoverageZones).ThenInclude(z => z.CoverageZone).ThenInclude(z => z!.Countries);
}
