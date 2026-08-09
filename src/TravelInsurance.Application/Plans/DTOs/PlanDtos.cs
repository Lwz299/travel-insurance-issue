using System.ComponentModel.DataAnnotations;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Plans.DTOs;

public class PlanCountryInput
{
    [Required]
    public Guid CountryId { get; set; }

    public bool IsExcluded { get; set; }
}

public class PricingRuleInput
{
    [Range(0, 120)]
    public int MinAge { get; set; }

    [Range(0, 120)]
    public int MaxAge { get; set; }

    [Range(1, 3650)]
    public int DurationFrom { get; set; }

    [Range(1, 3650)]
    public int DurationTo { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(3), MinLength(3)]
    public string Currency { get; set; } = "USD";
}

public class CreatePlanRequest
{
    [Required]
    public Guid CompanyId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public CoverageType CoverageType { get; set; }

    [Range(1, 3650)]
    public int? DefaultDurationDays { get; set; }

    [Range(0, 120)]
    public int? MinAge { get; set; }

    [Range(0, 120)]
    public int? MaxAge { get; set; }

    [Range(1, 3650)]
    public int? MaxTripDurationDays { get; set; }

    public bool AllowsQuotation { get; set; } = true;
    public bool AllowsDirectIssuance { get; set; } = true;

    public List<PlanCountryInput> Countries { get; set; } = new();
    public List<Guid> CoverageZoneIds { get; set; } = new();
    public List<PricingRuleInput> PricingRules { get; set; } = new();
}

public class UpdatePlanRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public CoverageType CoverageType { get; set; }

    [Range(1, 3650)]
    public int? DefaultDurationDays { get; set; }

    [Range(0, 120)]
    public int? MinAge { get; set; }

    [Range(0, 120)]
    public int? MaxAge { get; set; }

    [Range(1, 3650)]
    public int? MaxTripDurationDays { get; set; }

    public bool AllowsQuotation { get; set; } = true;
    public bool AllowsDirectIssuance { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public List<PlanCountryInput> Countries { get; set; } = new();
    public List<Guid> CoverageZoneIds { get; set; } = new();
    public List<PricingRuleInput> PricingRules { get; set; } = new();
}

public class UpdatePlanStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}

public record PlanCountryResponse(Guid Id, Guid CountryId, string CountryCode, string CountryName, bool IsExcluded);
public record PlanCoverageZoneResponse(Guid Id, Guid CoverageZoneId, string CoverageZoneCode, string CoverageZoneName, bool IsWorldwide, bool IsActive);

public record PricingRuleResponse(
    Guid Id, int MinAge, int MaxAge, int DurationFrom, int DurationTo, decimal Price, string Currency);

public record PlanSummaryResponse(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Name,
    CoverageType CoverageType,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record PlanResponse(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Name,
    string? Description,
    CoverageType CoverageType,
    int? DefaultDurationDays,
    int? MinAge,
    int? MaxAge,
    int? MaxTripDurationDays,
    bool AllowsQuotation,
    bool AllowsDirectIssuance,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<PlanCountryResponse> Countries,
    IReadOnlyCollection<PlanCoverageZoneResponse> CoverageZones,
    IReadOnlyCollection<PricingRuleResponse> PricingRules,
    IReadOnlyCollection<PlanCoverageAssignmentResponse> CoverageAssignments);
