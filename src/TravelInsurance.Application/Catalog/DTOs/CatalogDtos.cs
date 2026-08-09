using System.ComponentModel.DataAnnotations;

namespace TravelInsurance.Application.Catalog.DTOs;

// ---------- Lookups ----------

public record LookupItemResponse(int Id, string Code, string Name);

// ---------- Coverage Groups ----------

public class CreateCoverageGroupRequest
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

public class UpdateCoverageGroupRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public record CoverageGroupResponse(Guid Id, string Code, string Name, int SortOrder, bool IsActive);

// ---------- Coverages ----------

public class CreateCoverageRequest
{
    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public int RiskTypeId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsMandatory { get; set; }
}

public class UpdateCoverageRequest
{
    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public int RiskTypeId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCoverageStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}

public record CoverageResponse(
    Guid Id, Guid GroupId, string GroupName, int RiskTypeId, string RiskTypeName,
    string Code, string Name, string? Description, bool IsMandatory, bool IsActive);

public record CoverageDetailResponse(
    Guid Id, Guid GroupId, string GroupName, int RiskTypeId, string RiskTypeName,
    string Code, string Name, string? Description, bool IsMandatory, bool IsActive,
    IReadOnlyCollection<CoverageBenefitResponse> Benefits);

// ---------- Benefits ----------

public class CreateBenefitRequest
{
    [Required]
    public int BenefitTypeId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public class UpdateBenefitRequest
{
    [Required]
    public int BenefitTypeId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateBenefitStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}

public record BenefitResponse(
    Guid Id, int BenefitTypeId, string BenefitTypeName, string Code, string Name, string? Description, bool IsActive);

// ---------- Coverage-Benefit Mapping ----------

public class AddCoverageBenefitRequest
{
    [Required]
    public Guid BenefitId { get; set; }

    [Required]
    public int LimitTypeId { get; set; }

    public decimal? LimitValue { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public decimal? Deductible { get; set; }
    public int? WaitingPeriodDays { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsOptional { get; set; }
}

public class UpdateCoverageBenefitRequest
{
    [Required]
    public int LimitTypeId { get; set; }

    public decimal? LimitValue { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public decimal? Deductible { get; set; }
    public int? WaitingPeriodDays { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsOptional { get; set; }
}

public record CoverageBenefitResponse(
    Guid Id, Guid BenefitId, string BenefitCode, string BenefitName,
    int LimitTypeId, string LimitTypeName,
    decimal? LimitValue, string? Currency, decimal? Deductible,
    int? WaitingPeriodDays, int DisplayOrder, bool IsOptional);
