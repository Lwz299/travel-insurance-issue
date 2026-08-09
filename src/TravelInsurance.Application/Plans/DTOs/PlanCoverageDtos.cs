using System.ComponentModel.DataAnnotations;

namespace TravelInsurance.Application.Plans.DTOs;

// ---------- Plan Coverage Assignment ----------

public class AssignPlanCoverageRequest
{
    [Required]
    public Guid CoverageId { get; set; }

    public decimal? CustomLimit { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public decimal? CustomDeductible { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
}

public class UpdatePlanCoverageRequest
{
    public decimal? CustomLimit { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public decimal? CustomDeductible { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
}

public class UpsertPlanBenefitOverrideRequest
{
    [Required]
    public Guid BenefitId { get; set; }

    [Required]
    public int LimitTypeId { get; set; }

    public decimal? LimitOverride { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public decimal? DeductibleOverride { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public record PlanBenefitOverrideResponse(
    Guid Id, Guid BenefitId, string BenefitCode, string BenefitName,
    int LimitTypeId, string LimitTypeName,
    decimal? LimitOverride, string? Currency, decimal? DeductibleOverride, bool IsEnabled);

public record PlanCoverageAssignmentResponse(
    Guid Id, Guid CoverageId, string CoverageCode, string CoverageName, string GroupName,
    decimal? CustomLimit, string? Currency, decimal? CustomDeductible,
    bool IsEnabled, int DisplayOrder,
    IReadOnlyCollection<PlanBenefitOverrideResponse> BenefitOverrides);
