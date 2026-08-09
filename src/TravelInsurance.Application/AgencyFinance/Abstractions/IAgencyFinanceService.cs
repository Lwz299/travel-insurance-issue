using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.AgencyFinance.DTOs;
using TravelInsurance.Application.Payments.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.AgencyFinance.Abstractions;

public interface IAgencyFinanceService
{
    Task<(FundingAuthorizationResponse? Funding, OperationResponse Result)> AuthorizeForIssuanceAsync(PolicyIssuanceRequest issuance, PaymentSettlementInput settlement, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<OperationResponse> ConfirmElectronicPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<OperationResponse> CaptureForIssuedPolicyAsync(Guid issuanceRequestId, Guid policyId, CancellationToken cancellationToken = default);
    Task<OperationResponse> ReleaseForFailedIssuanceAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<FundingProfileResponse> Profiles, OperationResponse Result)> GetProfilesAsync(Guid agencyCompanyId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> ConfigureProfileAsync(Guid agencyCompanyId, ConfigureFundingProfileRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(WalletResponse? Wallet, OperationResponse Result)> GetWalletAsync(Guid agencyCompanyId, string currency, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(WalletResponse? Wallet, OperationResponse Result)> TopUpWalletAsync(Guid agencyCompanyId, WalletTopUpRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(CreditAccountResponse? Account, OperationResponse Result)> ConfigureCreditAsync(Guid agencyCompanyId, ConfigureCreditAccountRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(CreditAccountResponse? Account, OperationResponse Result)> GetCreditAsync(Guid agencyCompanyId, string currency, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(SettlementResponse? Settlement, OperationResponse Result)> CreateSettlementAsync(Guid agencyCompanyId, CreateSettlementRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(SettlementResponse? Settlement, OperationResponse Result)> RecordSettlementPaymentAsync(Guid settlementId, RecordSettlementPaymentRequest request, Guid actorUserId, CancellationToken cancellationToken = default);
}
