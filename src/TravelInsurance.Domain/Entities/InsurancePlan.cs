using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class InsurancePlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CoverageType CoverageType { get; set; }
    public int? DefaultDurationDays { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int? MaxTripDurationDays { get; set; }

    // A plan must support at least one issuance path.
    public bool AllowsQuotation { get; set; } = true;
    public bool AllowsDirectIssuance { get; set; } = true;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Company? Company { get; set; }
    public ICollection<PlanCoverageAssignment> CoverageAssignments { get; set; } = new List<PlanCoverageAssignment>();
    public ICollection<PlanCountry> Countries { get; set; } = new List<PlanCountry>();
    public ICollection<PlanCoverageZone> CoverageZones { get; set; } = new List<PlanCoverageZone>();
    public ICollection<PlanReinsuranceAgreement> ReinsuranceAgreements { get; set; } = new List<PlanReinsuranceAgreement>();
    public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
}
