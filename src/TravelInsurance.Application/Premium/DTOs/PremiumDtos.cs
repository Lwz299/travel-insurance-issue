using System.ComponentModel.DataAnnotations;

namespace TravelInsurance.Application.Premium.DTOs;

public class PassengerInput
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }
}

public class CalculatePremiumRequest
{
    [Required]
    public Guid PlanId { get; set; }
    [Required]
    public Guid DestinationCountryId { get; set; }
    public DateOnly TravelStart { get; set; }
    public DateOnly TravelEnd { get; set; }
    [MinLength(1)]
    public List<PassengerInput> Passengers { get; set; } = new();
}

public record BenefitSnapshotDto(Guid BenefitId, string BenefitCode, string BenefitName, int LimitTypeId, decimal? Limit, string? Currency, decimal? Deductible, int? WaitingPeriodDays);

public record CoverageSnapshotDto(Guid CoverageId, string CoverageCode, string CoverageName, decimal? Limit, string? Currency, decimal? Deductible, IReadOnlyCollection<BenefitSnapshotDto> Benefits);

public record PremiumCalculationResponse(
    bool Success,
    string Message,
    Guid PlanId,
    int DurationDays,
    decimal GrossPremium,
    string Currency,
    IReadOnlyCollection<CoverageSnapshotDto> CoverageSnapshots);
