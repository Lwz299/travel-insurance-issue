namespace TravelInsurance.Domain.Entities;

/// <summary>Assigns a catalog coverage to an insurance plan with optional limit overrides.</summary>
public class PlanCoverageAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid CoverageId { get; set; }
    public decimal? CustomLimit { get; set; }
    public string? Currency { get; set; }
    public decimal? CustomDeductible { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public InsurancePlan? Plan { get; set; }
    public Coverage? Coverage { get; set; }
    public ICollection<PlanCoverageBenefitOverride> BenefitOverrides { get; set; } = new List<PlanCoverageBenefitOverride>();
}
