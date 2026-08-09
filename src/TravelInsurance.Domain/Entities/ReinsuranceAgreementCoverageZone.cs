namespace TravelInsurance.Domain.Entities;

public class ReinsuranceAgreementCoverageZone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReinsuranceAgreementId { get; set; }
    public Guid CoverageZoneId { get; set; }
    public ReinsuranceAgreement? ReinsuranceAgreement { get; set; }
    public CoverageZone? CoverageZone { get; set; }
}
