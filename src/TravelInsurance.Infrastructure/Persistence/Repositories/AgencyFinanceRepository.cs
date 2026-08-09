using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.AgencyFinance.Abstractions;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class AgencyFinanceRepository(AppDbContext dbContext) : IAgencyFinanceRepository
{
    public Task<AgencyFundingProfile?> GetFundingProfileAsync(Guid agencyCompanyId, AgencyFundingMode mode, CancellationToken cancellationToken = default) =>
        dbContext.AgencyFundingProfiles.FirstOrDefaultAsync(x => x.AgencyCompanyId == agencyCompanyId && x.Mode == mode, cancellationToken);

    public async Task<IReadOnlyCollection<AgencyFundingProfile>> GetFundingProfilesAsync(Guid agencyCompanyId, CancellationToken cancellationToken = default) =>
        await dbContext.AgencyFundingProfiles.Where(x => x.AgencyCompanyId == agencyCompanyId).OrderBy(x => x.Mode).ToListAsync(cancellationToken);

    public Task<AgencyWallet?> GetWalletAsync(Guid agencyCompanyId, string currency, CancellationToken cancellationToken = default) =>
        dbContext.AgencyWallets.Include(x => x.Transactions).FirstOrDefaultAsync(x => x.AgencyCompanyId == agencyCompanyId && x.Currency == currency, cancellationToken);

    public Task<AgencyCreditAccount?> GetCreditAccountAsync(Guid agencyCompanyId, string currency, CancellationToken cancellationToken = default) =>
        dbContext.AgencyCreditAccounts.Include(x => x.Transactions).FirstOrDefaultAsync(x => x.AgencyCompanyId == agencyCompanyId && x.Currency == currency, cancellationToken);

    public Task<PolicyFunding?> GetFundingByIssuanceAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default) =>
        dbContext.PolicyFundings.FirstOrDefaultAsync(x => x.IssuanceRequestId == issuanceRequestId, cancellationToken);

    public Task<PolicyFunding?> GetFundingByPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default) =>
        dbContext.PolicyFundings.FirstOrDefaultAsync(x => x.PaymentId == paymentId, cancellationToken);

    public Task<PolicyIssuanceRequest?> GetIssuanceAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default) =>
        dbContext.PolicyIssuanceRequests.FirstOrDefaultAsync(x => x.Id == issuanceRequestId, cancellationToken);

    public Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking().Where(u => u.Id == userId && u.Status == UserStatus.Active)
            .Select(u => new WorkflowActor(u.Id, u.CompanyId, u.Company != null ? u.Company.Type : null, u.UserRoles.Any(ur => ur.Role!.Name == "SUPER_ADMIN")))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> IsAgencyAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        dbContext.Companies.AnyAsync(x => x.Id == companyId && x.Type == CompanyType.Agency && x.IsActive, cancellationToken);

    public async Task<IReadOnlyCollection<AgencyCreditTransaction>> GetUnsettledCreditTransactionsAsync(Guid agencyCompanyId, string currency, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toExclusiveUtc = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return await dbContext.AgencyCreditTransactions
            .Include(x => x.CreditAccount)
            .Where(x => x.CreditAccount!.AgencyCompanyId == agencyCompanyId && x.CreditAccount.Currency == currency && x.Type == CreditTransactionType.PolicyIssued && x.SettlementId == null && x.CreatedAt >= fromUtc && x.CreatedAt < toExclusiveUtc)
            .OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<AgencySettlement?> GetSettlementAsync(Guid settlementId, CancellationToken cancellationToken = default) =>
        dbContext.AgencySettlements.FirstOrDefaultAsync(x => x.Id == settlementId, cancellationToken);

    public void Add(object entity) => dbContext.Add(entity);

    public async Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); return WorkflowSaveResult.Saved; }
        catch (DbUpdateException) { return WorkflowSaveResult.Conflict; }
    }
}


