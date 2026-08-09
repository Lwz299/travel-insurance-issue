using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Domain.Entities;

public class ReinsuranceAgreement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ReinsurerCompanyId { get; set; }
    public string AgreementNumber { get; set; } = string.Empty;
    public string AgreementName { get; set; } = string.Empty;
    public ReinsuranceAgreementType Type { get; set; } = ReinsuranceAgreementType.Treaty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public ReinsuranceReportingMode ReportingMode { get; set; } = ReinsuranceReportingMode.PeriodicBordereau;
    public ReportingFrequency? ReportingFrequency { get; set; } = Enums.ReportingFrequency.Monthly;
    public bool RequiresPreApproval { get; set; }
    public int Priority { get; set; } = 100;
    public decimal CededSharePercentage { get; set; }
    public int? MaxTripDurationDays { get; set; }
    public decimal? MaxCoverageLimit { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Company? Company { get; set; }
    public Company? ReinsurerCompany { get; set; }
    public ICollection<PlanReinsuranceAgreement> PlanAgreements { get; set; } = new List<PlanReinsuranceAgreement>();
    public ICollection<ReinsuranceAgreementCoverageZone> CoverageZones { get; set; } = new List<ReinsuranceAgreementCoverageZone>();
    public ICollection<ReinsuranceCession> Cessions { get; set; } = new List<ReinsuranceCession>();
    public ICollection<ReinsuranceBordereauBatch> Bordereaux { get; set; } = new List<ReinsuranceBordereauBatch>();
}
