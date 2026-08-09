using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class Quote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string QuoteNumber { get; set; } = string.Empty;
    public Guid PlanId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? OwnerCompanyId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? DestinationCountryId { get; set; }
    public DateOnly TravelStart { get; set; }
    public DateOnly TravelEnd { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Calculated;
    public decimal GrossPremium { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset CalculatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public InsurancePlan? Plan { get; set; }
    public ICollection<QuotePassenger> Passengers { get; set; } = new List<QuotePassenger>();
    public ICollection<QuoteCoverageSnapshot> CoverageSnapshots { get; set; } = new List<QuoteCoverageSnapshot>();
}
