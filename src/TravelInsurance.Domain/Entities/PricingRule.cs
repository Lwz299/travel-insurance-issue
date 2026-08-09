namespace TravelInsurance.Domain.Entities;

public class PricingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public int DurationFrom { get; set; }
    public int DurationTo { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public InsurancePlan? Plan { get; set; }
}
