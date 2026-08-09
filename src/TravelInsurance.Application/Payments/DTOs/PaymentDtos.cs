using System.ComponentModel.DataAnnotations;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Payments.DTOs;

public class CreatePaymentRequest
{
    public Guid? QuoteId { get; set; }
    public Guid? PolicyId { get; set; }
    public Guid? IssuanceRequestId { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.ElectronicGateway;
    [Required, MaxLength(50)] public string ProviderCode { get; set; } = "DUMMY";
    public PaymentPurpose Purpose { get; set; } = PaymentPurpose.PolicyIssuance;
}

public class RetryPaymentRequest
{
    [Required, MaxLength(50)] public string ProviderCode { get; set; } = "DUMMY";
}

public record PaymentResponse(Guid Id, string ReferenceNumber, string BusinessReference, Guid? QuoteId, Guid? PolicyId, Guid? IssuanceRequestId, PaymentMethod Method, PaymentPurpose Purpose, PaymentStatus Status, decimal Amount, decimal PaidAmount, string Currency, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt);
public record PaymentAttemptResponse(Guid Id, string ProviderCode, string? ProviderReference, PaymentAttemptStatus Status, string? CheckoutUrl, string? FailureCode, string? FailureMessage, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt);
public record CreatePaymentResponse(PaymentResponse Payment, PaymentAttemptResponse Attempt);

public record GatewayCreateRequest(Guid PaymentId, string ReferenceNumber, decimal Amount, string Currency, string IdempotencyKey, string BusinessReference);
public record GatewayCreateResult(PaymentAttemptStatus Status, string? ProviderReference, string? CheckoutUrl, string? FailureCode, string? FailureMessage, string? SafeMetadata = null);
public record GatewayStatusResult(PaymentAttemptStatus Status, string? FailureCode, string? FailureMessage, string? SafeMetadata = null);
public record GatewayRefundRequest(string ProviderReference, decimal Amount, string Currency, string IdempotencyKey);
public record GatewayRefundResult(bool Success, string? ProviderReference, string? FailureCode, string? FailureMessage);
public record GatewayWebhookRequest(string ProviderCode, string Payload, string? Signature);
public record GatewayWebhookResult(bool IsValid, string? ProviderReference, PaymentAttemptStatus? Status, string? FailureCode, string? FailureMessage, string? SafeMetadata = null);
public record PaymentSucceeded(Guid PaymentId, Guid? QuoteId, Guid? PolicyId, Guid? IssuanceRequestId, Guid? CreatedByUserId);

public class PaymentSettlementInput
{
    public PaymentMethod Method { get; set; } = PaymentMethod.ElectronicGateway;
    [Required, MaxLength(50)] public string ProviderCode { get; set; } = "DUMMY";
}
