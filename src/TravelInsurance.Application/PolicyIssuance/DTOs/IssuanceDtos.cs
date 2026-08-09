using System.ComponentModel.DataAnnotations;
using TravelInsurance.Application.Payments.DTOs;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.PolicyIssuance.DTOs;

public class DirectIssueRequest : CalculatePremiumRequest
{
    public Guid? AgencyCompanyId { get; set; }
    [Required] public PaymentSettlementInput Settlement { get; set; } = new();
}

public class IssueFromQuoteRequest
{
    [Required] public Guid QuoteId { get; set; }
}

public record IssuanceResponse(
    Guid Id,
    Guid PlanId,
    Guid? QuoteId,
    Guid? PolicyId,
    Guid? PaymentId,
    IssuanceSource IssuanceSource,
    IssuanceStatus Status,
    decimal GrossPremium,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly EffectiveTo,
    string? FailureReason);

public record DirectIssuancePolicyDraft(
    Guid DestinationCountryId,
    IReadOnlyCollection<PassengerInput> Passengers,
    IReadOnlyCollection<CoverageSnapshotDto> CoverageSnapshots);
