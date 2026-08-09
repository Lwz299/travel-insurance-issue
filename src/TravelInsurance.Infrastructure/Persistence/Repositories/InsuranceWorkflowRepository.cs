using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class InsuranceWorkflowRepository(AppDbContext dbContext) : IInsuranceWorkflowRepository
{
    public Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.Status == UserStatus.Active)
            .Select(u => new WorkflowActor(u.Id, u.CompanyId, u.Company != null ? u.Company.Type : null, u.UserRoles.Any(ur => ur.Role!.Name == "SUPER_ADMIN")))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<InsurancePlan?> GetPlanForIssuanceAsync(Guid planId, CancellationToken cancellationToken = default) =>
        dbContext.Plans
            .Include(p => p.PricingRules)
            .Include(p => p.Countries)
            .Include(p => p.CoverageZones).ThenInclude(z => z.CoverageZone).ThenInclude(z => z!.Countries)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.Coverage).ThenInclude(c => c!.CoverageBenefits).ThenInclude(cb => cb.Benefit)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.Coverage).ThenInclude(c => c!.CoverageBenefits).ThenInclude(cb => cb.LimitType)
            .Include(p => p.CoverageAssignments).ThenInclude(a => a.BenefitOverrides).ThenInclude(o => o.Benefit)
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

    public Task<Quote?> GetQuoteForIssuanceAsync(Guid quoteId, CancellationToken cancellationToken = default) =>
        dbContext.Quotes
            .Include(q => q.Passengers)
            .Include(q => q.CoverageSnapshots).ThenInclude(s => s.BenefitSnapshots)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

    public Task<bool> HasPaidQuotePaymentAsync(Guid quoteId, decimal amount, string currency, CancellationToken cancellationToken = default) =>
        dbContext.Payments.AnyAsync(p => p.QuoteId == quoteId && p.Status == PaymentStatus.Paid && p.Amount == amount && p.Currency == currency, cancellationToken);

    public Task<bool> HasPolicyForQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default) =>
        dbContext.Policies.AnyAsync(p => p.QuoteId == quoteId, cancellationToken);

    public Task<Policy?> GetPolicyForWorkflowAsync(Guid policyId, CancellationToken cancellationToken = default) =>
        dbContext.Policies.Include(p => p.Plan).Include(p => p.Passengers)
            .Include(p => p.CoverageSnapshots).ThenInclude(s => s.BenefitSnapshots)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

    public Task<List<Policy>> GetPoliciesForWorkflowAsync(Guid? companyId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policies.AsNoTracking().Include(p => p.Plan).AsQueryable();
        if (companyId.HasValue) query = query.Where(p => p.AgencyCompanyId == companyId || p.Plan!.CompanyId == companyId);
        return query.OrderByDescending(p => p.IssuedAt).ToListAsync(cancellationToken);
    }

    public Task<Payment?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default) =>
        dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

    public Task<Payment?> GetPaymentByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Payments.FirstOrDefaultAsync(p => p.IdempotencyKey == key, cancellationToken);

    public Task<Policy?> GetPolicyByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Policies.Include(p => p.Passengers).Include(p => p.CoverageSnapshots).ThenInclude(s => s.BenefitSnapshots)
            .FirstOrDefaultAsync(p => p.IdempotencyKey == key, cancellationToken);

    public Task<PolicyIssuanceRequest?> GetIssuanceRequestAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default) =>
        dbContext.PolicyIssuanceRequests.Include(x => x.Policy).FirstOrDefaultAsync(x => x.Id == issuanceRequestId, cancellationToken);

    public Task<PolicyIssuanceRequest?> GetIssuanceRequestReadOnlyAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default) =>
        dbContext.PolicyIssuanceRequests.AsNoTracking().Include(x => x.Policy).FirstOrDefaultAsync(x => x.Id == issuanceRequestId, cancellationToken);

    public Task<PolicyIssuanceRequest?> GetIssuanceRequestByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.PolicyIssuanceRequests.Include(x => x.Policy).FirstOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);

    public Task<PolicyIssuanceRequest?> GetIssuanceRequestByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default) =>
        dbContext.PolicyIssuanceRequests.FirstOrDefaultAsync(x => x.QuoteId == quoteId, cancellationToken);

    public async Task<int> ExpireOutstandingQuotesAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        return await dbContext.Quotes
            .Where(q => q.ExpiresAt <= now && (q.Status == QuoteStatus.Calculated || q.Status == QuoteStatus.Sent || q.Status == QuoteStatus.Accepted))
            .ExecuteUpdateAsync(s => s.SetProperty(q => q.Status, QuoteStatus.Expired).SetProperty(q => q.UpdatedAt, now), cancellationToken);
    }

    public void AddQuote(Quote quote) => dbContext.Quotes.Add(quote);
    public void AddPayment(Payment payment) => dbContext.Payments.Add(payment);
    public void AddPolicy(Policy policy) => dbContext.Policies.Add(policy);
    public void AddIssuanceRequest(PolicyIssuanceRequest issuanceRequest) => dbContext.PolicyIssuanceRequests.Add(issuanceRequest);
    public async Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); return WorkflowSaveResult.Saved; }
        catch (DbUpdateException) { return WorkflowSaveResult.Conflict; }
    }
}
