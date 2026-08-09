namespace TravelInsurance.Domain.Entities;

public class PlanReinsuranceAgreement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public Guid ReinsuranceAgreementId { get; set; }
    public InsurancePlan? Plan { get; set; }
    public ReinsuranceAgreement? ReinsuranceAgreement { get; set; }
}
