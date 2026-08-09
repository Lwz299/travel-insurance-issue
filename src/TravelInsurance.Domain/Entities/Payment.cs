using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? QuoteId { get; set; }
    public Guid? PolicyId { get; set; }
    public Guid? IssuanceRequestId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BusinessReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod Method { get; set; }
    public PaymentPurpose Purpose { get; set; } = PaymentPurpose.PolicyIssuance;
    public string? IdempotencyKey { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public ICollection<PaymentAttempt> Attempts { get; set; } = new List<PaymentAttempt>();
}
