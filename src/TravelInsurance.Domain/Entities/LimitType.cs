namespace TravelInsurance.Domain.Entities;

public class LimitType
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<CoverageBenefit> CoverageBenefits { get; set; } = new List<CoverageBenefit>();
    public ICollection<PlanCoverageBenefitOverride> PlanBenefitOverrides { get; set; } = new List<PlanCoverageBenefitOverride>();
}
