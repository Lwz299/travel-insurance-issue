using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Issuance.Abstractions;

public interface IInsuranceWorkflowRepository
{
    Task<WorkflowActor?> GetActorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InsurancePlan?> GetPlanForIssuanceAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<Quote?> GetQuoteForIssuanceAsync(Guid quoteId, CancellationToken cancellationToken = default);
    Task<bool> HasPaidQuotePaymentAsync(Guid quoteId, decimal amount, string currency, CancellationToken cancellationToken = default);
    Task<bool> HasPolicyForQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
    Task<Policy?> GetPolicyForWorkflowAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<List<Policy>> GetPoliciesForWorkflowAsync(Guid? companyId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Policy?> GetPolicyByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PolicyIssuanceRequest?> GetIssuanceRequestAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default);
    Task<PolicyIssuanceRequest?> GetIssuanceRequestReadOnlyAsync(Guid issuanceRequestId, CancellationToken cancellationToken = default);
    Task<PolicyIssuanceRequest?> GetIssuanceRequestByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PolicyIssuanceRequest?> GetIssuanceRequestByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default);
    Task<int> ExpireOutstandingQuotesAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    void AddQuote(Quote quote);
    void AddPayment(Payment payment);
    void AddPolicy(Policy policy);
    void AddIssuanceRequest(PolicyIssuanceRequest issuanceRequest);
    Task<WorkflowSaveResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
