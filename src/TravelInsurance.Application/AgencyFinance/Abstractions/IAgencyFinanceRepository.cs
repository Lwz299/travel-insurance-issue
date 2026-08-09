using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.AgencyFinance.Abstractions;

public interface IAgencyFinanceRepository
{
    Task<AgencyFundingProfile?> GetFundingProfileAsync(Guid agencyCompanyId, TravelInsurance.Domain.Enums.AgencyFundingMode mode, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AgencyFundingProfile>> GetFundingProfilesAsync(Guid agencyCompanyId, CancellationToken cancellationToken = default);
    Task<AgencyWallet?> GetWalletAsync(Guid agencyCompanyId, string currency, CancellationToken cancellationToken = default);
    Task<AgencyCreditAccount?> GetCreditAccountAsync(Guid agencyCompanyId, string currency, CancellationToken cancellationToken = default);
    Task<PolicyFunding?> GetFundingByIssuanceAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default);
    Task<PolicyFunding?> GetFundingByPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<PolicyIssuanceRequest?> GetIssuanceAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default);
    Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsAgencyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AgencyCreditTransaction>> GetUnsettledCreditTransactionsAsync(Guid agencyCompanyId, string currency, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<AgencySettlement?> GetSettlementAsync(Guid settlementId, CancellationToken cancellationToken = default);
    void Add(object entity);
    Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
