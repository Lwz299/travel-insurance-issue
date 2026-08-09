using TravelInsurance.Application.Issuance.Abstractions;
using TravelInsurance.Application.Premium.Abstractions;
using TravelInsurance.Application.Premium.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Premium.Services;

public class PremiumCalculationService(IInsuranceWorkflowRepository workflowRepository) : IPremiumCalculationService
{
    public async Task<PremiumCalculationResponse> CalculateAsync(CalculatePremiumRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DestinationCountryId == Guid.Empty || request.TravelEnd < request.TravelStart || request.Passengers.Count == 0)
        {
            return Failed(request.PlanId, "Travel dates or passengers are invalid.");
        }

        if (request.TravelStart < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Failed(request.PlanId, "Travel start date cannot be in the past.");
        }

        var plan = await workflowRepository.GetPlanForIssuanceAsync(request.PlanId, cancellationToken);
        if (plan is null || !plan.IsActive)
        {
            return Failed(request.PlanId, "Plan is not available.");
        }

        var duration = request.TravelEnd.DayNumber - request.TravelStart.DayNumber + 1;
        if (plan.MaxTripDurationDays.HasValue && duration > plan.MaxTripDurationDays.Value)
        {
            return Failed(request.PlanId, "Trip duration exceeds the plan limit.");
        }

        if (!IsDestinationCovered(plan, request.DestinationCountryId))
        {
            return Failed(request.PlanId, "The selected destination is not covered by this plan.");
        }

        var matchedRules = new List<PricingRule>();
        foreach (var passenger in request.Passengers)
        {
            var age = CalculateAge(passenger.DateOfBirth, request.TravelStart);
            if (age < 0 || (plan.MinAge.HasValue && age < plan.MinAge) || (plan.MaxAge.HasValue && age > plan.MaxAge))
            {
                return Failed(request.PlanId, $"Passenger '{passenger.FullName}' does not meet the plan age rules.");
            }

            var rules = plan.PricingRules
                .Where(r => age >= r.MinAge && age <= r.MaxAge && duration >= r.DurationFrom && duration <= r.DurationTo)
                .ToArray();
            if (rules.Length != 1)
            {
                return Failed(request.PlanId, rules.Length == 0
                    ? $"No price rule matches passenger '{passenger.FullName}'."
                    : $"More than one pricing rule matches passenger '{passenger.FullName}'.");
            }

            matchedRules.Add(rules[0]);
        }

        var currencies = matchedRules.Select(r => r.Currency).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (currencies.Length != 1)
        {
            return Failed(request.PlanId, "All matched pricing rules must use one currency.");
        }

        var snapshots = plan.CoverageAssignments
            .Where(a => a.IsEnabled && a.Coverage is { IsActive: true, IsDeleted: false })
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new CoverageSnapshotDto(
                a.CoverageId, a.Coverage!.Code, a.Coverage.Name, a.CustomLimit, a.Currency, a.CustomDeductible,
                ResolveBenefitSnapshots(a)))
            .ToArray();

        return new PremiumCalculationResponse(true, "Premium calculated.", plan.Id, duration,
            matchedRules.Sum(r => r.Price), currencies[0].ToUpperInvariant(), snapshots);
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly referenceDate)
    {
        var age = referenceDate.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(age) > referenceDate) age--;
        return age;
    }

    private static bool IsDestinationCovered(InsurancePlan plan, Guid destinationCountryId)
    {
        // A plan-level country rule is the most specific rule and overrides its zones.
        var directCountryRule = plan.Countries.SingleOrDefault(c => c.CountryId == destinationCountryId);
        if (directCountryRule is not null) return !directCountryRule.IsExcluded;

        var activeZones = plan.CoverageZones
            .Select(pz => pz.CoverageZone)
            .Where(z => z is { IsActive: true })
            .Cast<CoverageZone>();

        if (activeZones.Any(zone => ZoneCoversCountry(zone, destinationCountryId))) return true;

        // Preserve the previous behavior: a plan with no geographic rules is worldwide.
        return plan.Countries.Count == 0 && plan.CoverageZones.Count == 0;
    }

    private static bool ZoneCoversCountry(CoverageZone zone, Guid countryId)
    {
        var countryRule = zone.Countries.SingleOrDefault(c => c.CountryId == countryId);
        return zone.IsWorldwide
            ? countryRule is null || !countryRule.IsExcluded
            : countryRule is { IsExcluded: false };
    }

    private static IReadOnlyCollection<BenefitSnapshotDto> ResolveBenefitSnapshots(PlanCoverageAssignment assignment) =>
        assignment.Coverage!.CoverageBenefits
            .OrderBy(cb => cb.DisplayOrder)
            .Select(cb =>
            {
                var overrideValue = assignment.BenefitOverrides.SingleOrDefault(o => o.BenefitId == cb.BenefitId);
                return new { Catalog = cb, Override = overrideValue };
            })
            .Where(x => x.Override is null || x.Override.IsEnabled)
            .Select(x => new BenefitSnapshotDto(
                x.Catalog.BenefitId, x.Catalog.Benefit?.Code ?? string.Empty, x.Catalog.Benefit?.Name ?? string.Empty,
                x.Override?.LimitTypeId ?? x.Catalog.LimitTypeId,
                x.Override?.LimitOverride ?? x.Catalog.LimitValue,
                x.Override?.Currency ?? x.Catalog.Currency,
                x.Override?.DeductibleOverride ?? x.Catalog.Deductible,
                x.Catalog.WaitingPeriodDays))
            .ToArray();

    private static PremiumCalculationResponse Failed(Guid planId, string message) =>
        new(false, message, planId, 0, 0, string.Empty, Array.Empty<CoverageSnapshotDto>());
}
