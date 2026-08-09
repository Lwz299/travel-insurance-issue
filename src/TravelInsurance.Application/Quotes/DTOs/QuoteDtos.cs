using System.ComponentModel.DataAnnotations;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Quotes.DTOs;

public class CreateQuoteRequest : CalculatePremiumRequest
{
    [Range(1, 168)]
    public int ValidForHours { get; set; } = 72;
}

public record QuoteResponse(
    Guid Id, string QuoteNumber, Guid PlanId, Guid CompanyId, QuoteStatus Status,
    DateOnly TravelStart, DateOnly TravelEnd, decimal GrossPremium, string Currency,
    DateTimeOffset ExpiresAt, DateTimeOffset? AcceptedAt,
    IReadOnlyCollection<PassengerInput> Passengers,
    IReadOnlyCollection<CoverageSnapshotDto> CoverageSnapshots);

public class CancelQuoteRequest
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
