using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Payments.DTOs;

namespace TravelInsurance.Application.Payments.Abstractions;

public interface IPaymentService
{
    Task<(CreatePaymentResponse? Payment, OperationResponse Result)> CreateAsync(CreatePaymentRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<(CreatePaymentResponse? Payment, OperationResponse Result)> CreateForPolicyAsync(Guid policyId, PaymentSettlementInput settlement, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<(CreatePaymentResponse? Payment, OperationResponse Result)> CreateForIssuanceAsync(Guid issuanceRequestId, PaymentSettlementInput settlement, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<(PaymentResponse? Payment, OperationResponse Result)> GetByIdAsync(Guid paymentId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentAttemptResponse>> GetAttemptsAsync(Guid paymentId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<(CreatePaymentResponse? Payment, OperationResponse Result)> RetryAsync(Guid paymentId, RetryPaymentRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<OperationResponse> CancelAsync(Guid paymentId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<OperationResponse> HandleWebhookAsync(string providerCode, GatewayWebhookRequest request, CancellationToken cancellationToken = default);
}
