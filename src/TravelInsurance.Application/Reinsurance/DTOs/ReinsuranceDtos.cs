using System.ComponentModel.DataAnnotations;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Reinsurance.DTOs;

public class CreateReinsuranceAgreementRequest
{
    [Required] public Guid CompanyId { get; set; }
    [Required] public Guid ReinsurerCompanyId { get; set; }
    [Required, MaxLength(100)] public string AgreementNumber { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string AgreementName { get; set; } = string.Empty;
    public ReinsuranceAgreementType Type { get; set; } = ReinsuranceAgreementType.Treaty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public ReinsuranceReportingMode ReportingMode { get; set; } = ReinsuranceReportingMode.PeriodicBordereau;
    public ReportingFrequency? ReportingFrequency { get; set; } = TravelInsurance.Domain.Enums.ReportingFrequency.Monthly;
    public bool RequiresPreApproval { get; set; }
    [Range(1, 100)] public decimal CededSharePercentage { get; set; }
    [Range(1, 3650)] public int? MaxTripDurationDays { get; set; }
    [Range(0, double.MaxValue)] public decimal? MaxCoverageLimit { get; set; }
    [Range(0, 120)] public int? MinAge { get; set; }
    [Range(0, 120)] public int? MaxAge { get; set; }
    [Range(1, int.MaxValue)] public int Priority { get; set; } = 100;
    [MinLength(1)] public List<Guid> PlanIds { get; set; } = new();
    public List<Guid> CoverageZoneIds { get; set; } = new();
}

public class UpdateReinsuranceAgreementRequest : CreateReinsuranceAgreementRequest
{
    public bool IsActive { get; set; } = true;
}

public record ReinsuranceAgreementResponse(
    Guid Id, Guid CompanyId, Guid ReinsurerCompanyId, string AgreementNumber, string AgreementName,
    ReinsuranceAgreementType Type, DateOnly EffectiveFrom, DateOnly EffectiveTo,
    ReinsuranceReportingMode ReportingMode, ReportingFrequency? ReportingFrequency, bool RequiresPreApproval,
    decimal CededSharePercentage, int? MaxTripDurationDays, decimal? MaxCoverageLimit, int? MinAge, int? MaxAge,
    int Priority, bool IsActive, IReadOnlyCollection<Guid> PlanIds, IReadOnlyCollection<Guid> CoverageZoneIds);

public record ReinsuranceCessionResponse(
    Guid Id, Guid PolicyId, Guid ReinsuranceAgreementId, Guid ReinsurerCompanyId, string AgreementNumber,
    ReinsuranceCessionStatus Status, decimal GrossPremium, decimal CededPremium, decimal GrossSumInsured,
    decimal CededSumInsured, decimal CededSharePercentage, decimal CompanyRetentionPercentage, string Currency,
    Guid? BordereauBatchId, DateTimeOffset CreatedAt);

public class GenerateBordereauRequest
{
    [Required] public Guid ReinsuranceAgreementId { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
}

public record ReinsuranceBordereauResponse(
    Guid Id, Guid ReinsuranceAgreementId, string BatchNumber, DateOnly PeriodFrom, DateOnly PeriodTo,
    ReinsuranceBordereauStatus Status, DateTimeOffset GeneratedAt, DateTimeOffset? FinalizedAt,
    IReadOnlyCollection<ReinsuranceCessionResponse> Cessions);
