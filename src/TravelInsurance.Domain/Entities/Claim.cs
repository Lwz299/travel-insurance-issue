using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class Claim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PolicyId { get; set; }
    public Guid PolicyPassengerId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public ClaimType ClaimType { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Registered;
    public DateOnly IncidentDate { get; set; }
    public Guid? IncidentCountryId { get; set; }
    public string IncidentDescription { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? DecisionReason { get; set; }
    public string? InternalNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public Guid? DecidedByUserId { get; set; }

    public Policy? Policy { get; set; }
    public PolicyPassenger? PolicyPassenger { get; set; }
    public Country? IncidentCountry { get; set; }
    public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    public ICollection<ClaimPayment> Payments { get; set; } = new List<ClaimPayment>();
    public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
}
