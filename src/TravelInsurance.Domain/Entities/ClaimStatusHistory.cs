using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class ClaimStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClaimId { get; set; }
    public ClaimStatus OldStatus { get; set; }
    public ClaimStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid ChangedByUserId { get; set; }
    public Claim? Claim { get; set; }
}
