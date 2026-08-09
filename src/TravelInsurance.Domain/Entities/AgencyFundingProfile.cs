using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class AgencyFundingProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgencyCompanyId { get; set; }
    public AgencyFundingMode Mode { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;
    public decimal? MaximumTransactionAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
