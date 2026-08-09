namespace TravelInsurance.Domain.Entities;

/// <summary>
/// Reusable geographical area used to decide where a travel plan is valid.
/// A worldwide zone covers every country except its explicitly excluded countries.
/// A regional zone covers only its explicitly included countries.
/// </summary>
public class CoverageZone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsWorldwide { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CoverageZoneCountry> Countries { get; set; } = new List<CoverageZoneCountry>();
    public ICollection<PlanCoverageZone> PlanCoverageZones { get; set; } = new List<PlanCoverageZone>();
    public ICollection<ReinsuranceAgreementCoverageZone> ReinsuranceAgreementCoverageZones { get; set; } = new List<ReinsuranceAgreementCoverageZone>();
}
