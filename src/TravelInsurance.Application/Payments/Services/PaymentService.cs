using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Payments.Abstractions;
using TravelInsurance.Application.Payments.DTOs;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Payments.Services;

public class PaymentService(IInsuranceWorkflowRepository workflowRepository, IPaymentRepository paymentRepository, IPaymentGatewayResolver gatewayResolver, IPaymentEventPublisher eventPublisher, IAuditLogRepository auditLogRepository, INumberSequenceService numberSequenceService) : IPaymentService
{
    public Task<(CreatePaymentResponse? Payment, OperationResponse Result)> CreateForPolicyAsync(Guid policyId, PaymentSettlementInput settlement, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default) =>
        CreateAsync(new CreatePaymentRequest { PolicyId = policyId, Method = settlement.Method, ProviderCode = settlement.ProviderCode, Purpose = PaymentPurpose.PolicyIssuance }, actorUserId, idempotencyKey, cancellationToken);

    public Task<(CreatePaymentResponse? Payment, OperationResponse Result)> CreateForIssuanceAsync(Guid issuanceRequestId, PaymentSettlementInput settlement, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default) =>
        CreateAsync(new CreatePaymentRequest { IssuanceRequestId = issuanceRequestId, Method = settlement.Method, ProviderCode = settlement.ProviderCode, Purpose = PaymentPurpose.PolicyIssuance }, actorUserId, idempotencyKey, cancellationToken);

    public async Task<(CreatePaymentResponse? Payment, OperationResponse Result)> CreateAsync(CreatePaymentRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return (null, Fail("Idempotency-Key header is required."));
        if (new[] { request.QuoteId, request.PolicyId, request.IssuanceRequestId }.Count(x => x.HasValue) != 1) return (null, Fail("Exactly one payment target is required."));
        if (request.Method is PaymentMethod.AgencyWallet or PaymentMethod.PrepaidCredit or PaymentMethod.PostpaidCredit)
            return (null, Fail("Agency wallet and postpaid credit must be authorized through policy funding."));
        var existing = await paymentRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null) return (ToCreated(existing), Ok("Existing payment returned."));
        var actor = await workflowRepository.GetActorAsync(actorUserId, cancellationToken);
        if (actor is null) return (null, Fail("Active user account is required."));
        var target = await ResolveTargetAsync(request, actor, cancellationToken);
        if (!target.Success) return (null, Fail(target.Error!));
        var gateway = gatewayResolver.Resolve(request.ProviderCode);
        if (gateway is null) return (null, Fail("Requested payment provider is unavailable."));

        var payment = new Payment { QuoteId = request.QuoteId, PolicyId = request.PolicyId, IssuanceRequestId = request.IssuanceRequestId, ReferenceNumber = await numberSequenceService.NextAsync(actor.CompanyId ?? target.CompanyId, NumberSequenceType.Payment, cancellationToken), BusinessReference = target.BusinessReference!, Amount = target.Amount, Currency = target.Currency!, Method = request.Method, Purpose = request.Purpose, Status = PaymentStatus.Pending, IdempotencyKey = idempotencyKey, CreatedByUserId = actor.UserId };
        var attempt = new PaymentAttempt { PaymentId = payment.Id, ProviderCode = gateway.ProviderCode, IdempotencyKey = $"attempt:{idempotencyKey}", Status = PaymentAttemptStatus.Created };
        payment.Attempts.Add(attempt); paymentRepository.Add(payment);
        if (await paymentRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Payment creation conflicted. Please retry."));
        await AuditAsync(actor.UserId, "PAYMENT.CREATED", payment.Id, cancellationToken);
        var gatewayResult = await gateway.CreateAsync(new GatewayCreateRequest(payment.Id, payment.ReferenceNumber, payment.Amount, payment.Currency, attempt.IdempotencyKey, payment.BusinessReference), cancellationToken);
        await ApplyGatewayResultAsync(payment, attempt, gatewayResult, cancellationToken);
        return (ToCreated(payment), Ok("Payment created."));
    }

    public async Task<(PaymentResponse? Payment, OperationResponse Result)> GetByIdAsync(Guid paymentId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var payment = await AuthorizedPaymentAsync(paymentId, actorUserId, cancellationToken);
        return payment is null ? (null, Fail("Payment not found.")) : (Map(payment), Ok("Payment found."));
    }

    public async Task<IReadOnlyCollection<PaymentAttemptResponse>> GetAttemptsAsync(Guid paymentId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        if (await AuthorizedPaymentAsync(paymentId, actorUserId, cancellationToken) is null) return Array.Empty<PaymentAttemptResponse>();
        return (await paymentRepository.GetAttemptsAsync(paymentId, cancellationToken)).Select(Map).ToArray();
    }

    public async Task<(CreatePaymentResponse? Payment, OperationResponse Result)> RetryAsync(Guid paymentId, RetryPaymentRequest request, Guid actorUserId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return (null, Fail("Idempotency-Key header is required."));
        var payment = await AuthorizedPaymentAsync(paymentId, actorUserId, cancellationToken);
        if (payment is null) return (null, Fail("Payment not found."));
        if (payment.Status is PaymentStatus.Paid or PaymentStatus.Refunded) return (null, Fail("Paid payments cannot be retried."));
        if (payment.Attempts.Any(a => a.ProviderCode == request.ProviderCode && a.IdempotencyKey == idempotencyKey)) return (ToCreated(payment), Ok("Existing attempt returned."));
        var gateway = gatewayResolver.Resolve(request.ProviderCode); if (gateway is null) return (null, Fail("Requested payment provider is unavailable."));
        var attempt = new PaymentAttempt { PaymentId = payment.Id, ProviderCode = gateway.ProviderCode, IdempotencyKey = idempotencyKey, Status = PaymentAttemptStatus.Created };
        paymentRepository.AddAttempt(attempt);
        if (await paymentRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return (null, Fail("Payment retry conflicted. Please retry."));
        var result = await gateway.CreateAsync(new GatewayCreateRequest(payment.Id, payment.ReferenceNumber, payment.Amount - payment.PaidAmount, payment.Currency, attempt.IdempotencyKey, payment.BusinessReference), cancellationToken);
        await ApplyGatewayResultAsync(payment, attempt, result, cancellationToken);
        return (ToCreated(payment, attempt), Ok("Payment retry created."));
    }

    public async Task<OperationResponse> CancelAsync(Guid paymentId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var payment = await AuthorizedPaymentAsync(paymentId, actorUserId, cancellationToken);
        if (payment is null) return Fail("Payment not found.");
        if (payment.Status is PaymentStatus.Paid or PaymentStatus.Refunded) return Fail("Completed payments cannot be cancelled.");
        payment.Status = PaymentStatus.Cancelled;
        if (await paymentRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) return Fail("Payment cancellation conflicted. Please retry.");
        await AuditAsync(actorUserId, "PAYMENT.CANCELLED", payment.Id, cancellationToken); return Ok("Payment cancelled.");
    }

    public async Task<OperationResponse> HandleWebhookAsync(string providerCode, GatewayWebhookRequest request, CancellationToken cancellationToken = default)
    {
        var gateway = gatewayResolver.Resolve(providerCode); if (gateway is null) return Fail("Payment provider is unavailable.");
        var verified = await gateway.VerifyWebhookAsync(request with { ProviderCode = providerCode }, cancellationToken);
        if (!verified.IsValid || string.IsNullOrWhiteSpace(verified.ProviderReference) || verified.Status is null) return Fail("Webhook verification failed.");
        var attempt = await paymentRepository.GetAttemptByProviderReferenceAsync(providerCode, verified.ProviderReference, cancellationToken);
        if (attempt is null) return Fail("Payment attempt not found.");
        await ApplyGatewayResultAsync(attempt.Payment!, attempt, new GatewayCreateResult(verified.Status.Value, verified.ProviderReference, null, verified.FailureCode, verified.FailureMessage, verified.SafeMetadata), cancellationToken);
        return Ok("Webhook processed.");
    }

    private async Task ApplyGatewayResultAsync(Payment payment, PaymentAttempt attempt, GatewayCreateResult result, CancellationToken cancellationToken)
    {
        attempt.ProviderReference = result.ProviderReference ?? attempt.ProviderReference; attempt.Status = result.Status; attempt.CheckoutUrl = result.CheckoutUrl; attempt.FailureCode = result.FailureCode; attempt.FailureMessage = result.FailureMessage; attempt.SafeMetadata = result.SafeMetadata;
        if (result.Status is PaymentAttemptStatus.Succeeded or PaymentAttemptStatus.Failed or PaymentAttemptStatus.Cancelled or PaymentAttemptStatus.Expired) attempt.CompletedAt = DateTimeOffset.UtcNow;
        if (result.Status == PaymentAttemptStatus.Succeeded)
        {
            payment.Status = PaymentStatus.Paid; payment.PaidAmount = payment.Amount; payment.CompletedAt = DateTimeOffset.UtcNow;
        }
        else if (result.Status is PaymentAttemptStatus.Pending or PaymentAttemptStatus.RequiresAction) payment.Status = PaymentStatus.Processing;
        else if (result.Status == PaymentAttemptStatus.Failed) payment.Status = PaymentStatus.Failed;
        if (await paymentRepository.SaveChangesAsync(cancellationToken) != WorkflowSaveResult.Saved) throw new InvalidOperationException("Payment update conflicted.");
        if (payment.Status == PaymentStatus.Paid) await eventPublisher.PublishSucceededAsync(new PaymentSucceeded(payment.Id, payment.QuoteId, payment.PolicyId, payment.IssuanceRequestId, payment.CreatedByUserId), cancellationToken);
    }

    private async Task<(bool Success, string? Error, decimal Amount, string? Currency, string? BusinessReference, Guid CompanyId)> ResolveTargetAsync(CreatePaymentRequest request, WorkflowActor actor, CancellationToken ct)
    {
        if (request.QuoteId.HasValue)
        {
            var quote = await workflowRepository.GetQuoteForIssuanceAsync(request.QuoteId.Value, ct);
            if (quote is null || !WorkflowAccess.CanAccess(actor, quote)) return (false, "Quote not found.", 0, null, null, Guid.Empty);
            if (quote.Status != QuoteStatus.Accepted || quote.ExpiresAt <= DateTimeOffset.UtcNow) return (false, "Quote is not payable.", 0, null, null, Guid.Empty);
            return (true, null, quote.GrossPremium, quote.Currency, quote.QuoteNumber, quote.CompanyId);
        }
        if (request.IssuanceRequestId.HasValue)
        {
            var issuance = await workflowRepository.GetIssuanceRequestAsync(request.IssuanceRequestId.Value, ct);
            if (issuance is null || !CanAccess(actor, issuance)) return (false, "Issuance request not found.", 0, null, null, Guid.Empty);
            if (issuance.Status != IssuanceStatus.PendingPayment) return (false, "Issuance request is not awaiting payment.", 0, null, null, Guid.Empty);
            var plan = await workflowRepository.GetPlanForIssuanceAsync(issuance.PlanId, ct);
            if (plan is null) return (false, "Issuance plan was not found.", 0, null, null, Guid.Empty);
            return (true, null, issuance.GrossPremium, issuance.Currency, issuance.Id.ToString("N"), plan.CompanyId);
        }
        var policy = await workflowRepository.GetPolicyForWorkflowAsync(request.PolicyId!.Value, ct);
        if (policy is null || !WorkflowAccess.CanAccess(actor, policy)) return (false, "Policy not found.", 0, null, null, Guid.Empty);
        if (policy.Status != PolicyStatus.PendingPayment) return (false, "Policy is not awaiting payment.", 0, null, null, Guid.Empty);
        return (true, null, policy.GrossPremium, policy.Currency, policy.PolicyNumber, policy.Plan!.CompanyId);
    }

    private async Task<Payment?> AuthorizedPaymentAsync(Guid paymentId, Guid actorUserId, CancellationToken ct)
    {
        var actor = await workflowRepository.GetActorAsync(actorUserId, ct); var payment = await paymentRepository.GetByIdAsync(paymentId, ct);
        if (actor is null || payment is null) return null;
        if (payment.QuoteId.HasValue) { var quote = await workflowRepository.GetQuoteForIssuanceAsync(payment.QuoteId.Value, ct); return quote is not null && WorkflowAccess.CanAccess(actor, quote) ? payment : null; }
        if (payment.IssuanceRequestId.HasValue)
        {
            var issuance = await workflowRepository.GetIssuanceRequestAsync(payment.IssuanceRequestId.Value, ct);
            return issuance is not null && CanAccess(actor, issuance) ? payment : null;
        }
        var policy = await workflowRepository.GetPolicyForWorkflowAsync(payment.PolicyId!.Value, ct);
        return policy is not null && WorkflowAccess.CanAccess(actor, policy) ? payment : null;
    }

    private Task AuditAsync(Guid actorId, string action, Guid entityId, CancellationToken ct) => auditLogRepository.AddAsync(new AuditLog { ActorUserId = actorId, Action = action, EntityType = "payments", EntityId = entityId }, ct);
    private static bool CanAccess(WorkflowActor actor, PolicyIssuanceRequest request) => actor.IsSuperAdmin || request.RequestedByUserId == actor.UserId || (actor.CompanyId.HasValue && request.AgencyCompanyId == actor.CompanyId);
    private static PaymentResponse Map(Payment p) => new(p.Id, p.ReferenceNumber, p.BusinessReference, p.QuoteId, p.PolicyId, p.IssuanceRequestId, p.Method, p.Purpose, p.Status, p.Amount, p.PaidAmount, p.Currency, p.CreatedAt, p.CompletedAt);
    private static PaymentAttemptResponse Map(PaymentAttempt a) => new(a.Id, a.ProviderCode, a.ProviderReference, a.Status, a.CheckoutUrl, a.FailureCode, a.FailureMessage, a.CreatedAt, a.CompletedAt);
    private static CreatePaymentResponse ToCreated(Payment p, PaymentAttempt? attempt = null) => new(Map(p), Map(attempt ?? p.Attempts.OrderByDescending(a => a.CreatedAt).First()));
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}


