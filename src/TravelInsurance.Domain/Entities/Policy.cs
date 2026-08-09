using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class Policy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid PlanId { get; set; }
    public Guid? DestinationCountryId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? IssuanceRequestId { get; set; }
    public IssuanceSource IssuanceSource { get; set; }
    public Guid? IssuedByUserId { get; set; }
    public Guid? AgencyCompanyId { get; set; }
    public string? IdempotencyKey { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Active;
    public decimal GrossPremium { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    public InsurancePlan? Plan { get; set; }
    public Quote? Quote { get; set; }
    public ICollection<PolicyPassenger> Passengers { get; set; } = new List<PolicyPassenger>();
    public ICollection<PolicyCoverageSnapshot> CoverageSnapshots { get; set; } = new List<PolicyCoverageSnapshot>();
    public ICollection<ReinsuranceCession> ReinsuranceCessions { get; set; } = new List<ReinsuranceCession>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
