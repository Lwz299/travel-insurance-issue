using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class PolicyFunding
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IssuanceRequestId { get; set; }
    public Guid? PolicyId { get; set; }
    public Guid? AgencyCompanyId { get; set; }
    public Guid? PaymentId { get; set; }
    public AgencyFundingMode FundingMode { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string FundingReference { get; set; } = string.Empty;
    public PolicyFundingStatus Status { get; set; } = PolicyFundingStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FundedAt { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
}
