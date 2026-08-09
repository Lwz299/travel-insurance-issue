using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class AgencySettlement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgencyCompanyId { get; set; }
    public string Currency { get; set; } = "USD";
    public string SettlementNumber { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal GrossPremium { get; set; }
    public decimal AgencyCommission { get; set; }
    public decimal Refunds { get; set; }
    public decimal NetAmountDue { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public SettlementStatus Status { get; set; } = SettlementStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? IssuedAt { get; set; }
}
