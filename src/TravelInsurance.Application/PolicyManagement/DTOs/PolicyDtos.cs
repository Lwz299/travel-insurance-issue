using System.ComponentModel.DataAnnotations;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.PolicyManagement.DTOs;

public class CancelPolicyRequest
{
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public record PolicyResponse(
    Guid Id, string PolicyNumber, Guid PlanId, Guid? QuoteId, IssuanceSource IssuanceSource,
    PolicyStatus Status, decimal GrossPremium, string Currency, DateOnly EffectiveFrom, DateOnly EffectiveTo,
    IReadOnlyCollection<PassengerInput> Passengers,
    IReadOnlyCollection<CoverageSnapshotDto> CoverageSnapshots);

public record PolicySummaryResponse(Guid Id, string PolicyNumber, Guid PlanId, Guid? QuoteId, IssuanceSource IssuanceSource, PolicyStatus Status, decimal GrossPremium, string Currency, DateOnly EffectiveFrom, DateOnly EffectiveTo, DateTimeOffset IssuedAt);
