using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class PaymentAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PaymentId { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string? ProviderReference { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public PaymentAttemptStatus Status { get; set; } = PaymentAttemptStatus.Created;
    public string? CheckoutUrl { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public string? SafeMetadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public Payment? Payment { get; set; }
}
