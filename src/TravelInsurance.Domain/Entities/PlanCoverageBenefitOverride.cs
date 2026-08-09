namespace TravelInsurance.Domain.Entities;

public class PlanCoverageBenefitOverride
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanCoverageId { get; set; }
    public Guid BenefitId { get; set; }
    public int LimitTypeId { get; set; }
    public decimal? LimitOverride { get; set; }
    public string? Currency { get; set; }
    public decimal? DeductibleOverride { get; set; }
    public bool IsEnabled { get; set; } = true;

    public PlanCoverageAssignment? PlanCoverage { get; set; }
    public Benefit? Benefit { get; set; }
    public LimitType? LimitType { get; set; }
}
