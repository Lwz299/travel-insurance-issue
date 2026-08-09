namespace TravelInsurance.Domain.Entities;

public class PlanCoverageZone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid CoverageZoneId { get; set; }

    public InsurancePlan? Plan { get; set; }
    public CoverageZone? CoverageZone { get; set; }
}
