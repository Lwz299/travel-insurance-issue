using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

/// <summary>
/// Tracks an issuance journey independently from a policy. A policy is linked only after the journey succeeds.
/// </summary>
public class PolicyIssuanceRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid? QuoteId { get; set; }
    public IssuanceSource IssuanceSource { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? AgencyCompanyId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public IssuanceStatus Status { get; set; } = IssuanceStatus.Draft;
    public decimal GrossPremium { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public string? PolicyDraftJson { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public InsurancePlan? Plan { get; set; }
    public Quote? Quote { get; set; }
    public Policy? Policy { get; set; }
}
