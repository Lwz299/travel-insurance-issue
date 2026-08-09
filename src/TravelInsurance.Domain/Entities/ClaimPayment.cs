namespace TravelInsurance.Domain.Entities;

public class ClaimPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid RecordedByUserId { get; set; }
    public Claim? Claim { get; set; }
}
