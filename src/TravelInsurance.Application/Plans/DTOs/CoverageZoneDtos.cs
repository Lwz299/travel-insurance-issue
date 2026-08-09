using System.ComponentModel.DataAnnotations;

namespace TravelInsurance.Application.Plans.DTOs;

public class CoverageZoneCountryInput
{
    [Required]
    public Guid CountryId { get; set; }

    public bool IsExcluded { get; set; }
}

public class CreateCoverageZoneRequest
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsWorldwide { get; set; }
    public List<CoverageZoneCountryInput> Countries { get; set; } = new();
}

public class UpdateCoverageZoneRequest : CreateCoverageZoneRequest
{
    public bool IsActive { get; set; } = true;
}

public record CoverageZoneCountryResponse(Guid CountryId, string CountryCode, string CountryName, bool IsExcluded);
public record CoverageZoneSummaryResponse(Guid Id, string Code, string Name, bool IsWorldwide, bool IsActive);
public record CoverageZoneResponse(Guid Id, string Code, string Name, bool IsWorldwide, bool IsActive, IReadOnlyCollection<CoverageZoneCountryResponse> Countries);
