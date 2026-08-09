using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.DTOs;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Reinsurance.Services;

public class ReinsuranceCessionService(IReinsuranceRepository reinsuranceRepository) : IReinsuranceCessionService
{
    public async Task CreateForIssuedPolicyAsync(Policy policy, Guid companyId, CancellationToken cancellationToken = default)
    {
        if (policy.DestinationCountryId is null || await reinsuranceRepository.HasCessionForPolicyAsync(policy.Id, cancellationToken)) return;

        var agreement = (await reinsuranceRepository.GetAutomaticAgreementsForPlanAsync(companyId, policy.PlanId, cancellationToken))
            .FirstOrDefault(a => IsEligible(a, policy));
        if (agreement is null) return;

        var grossSumInsured = policy.CoverageSnapshots.Where(s => string.Equals(s.Currency, policy.Currency, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Limit ?? 0m).DefaultIfEmpty(0m).Max();
        var cededShare = agreement.CededSharePercentage;
        reinsuranceRepository.AddCession(new ReinsuranceCession
        {
            PolicyId = policy.Id, ReinsuranceAgreementId = agreement.Id, ReinsurerCompanyId = agreement.ReinsurerCompanyId,
            AgreementNumberSnapshot = agreement.AgreementNumber,
            Status = agreement.ReportingMode == ReinsuranceReportingMode.PeriodicBordereau ? ReinsuranceCessionStatus.PendingBordereau : ReinsuranceCessionStatus.PendingSubmission,
            GrossPremium = policy.GrossPremium, CededPremium = ShareOf(policy.GrossPremium, cededShare), GrossSumInsured = grossSumInsured,
            CededSumInsured = ShareOf(grossSumInsured, cededShare), CededSharePercentage = cededShare,
            CompanyRetentionPercentage = 100m - cededShare, Currency = policy.Currency.ToUpperInvariant()
        });
    }

    public async Task<IReadOnlyCollection<ReinsuranceCessionResponse>> GetAllAsync(Guid? policyId, Guid? agreementId, CancellationToken cancellationToken = default) =>
        (await reinsuranceRepository.GetCessionsAsync(policyId, agreementId, cancellationToken)).Select(Map).ToArray();

    internal static ReinsuranceCessionResponse Map(ReinsuranceCession c) => new(c.Id, c.PolicyId, c.ReinsuranceAgreementId, c.ReinsurerCompanyId, c.AgreementNumberSnapshot, c.Status, c.GrossPremium, c.CededPremium, c.GrossSumInsured, c.CededSumInsured, c.CededSharePercentage, c.CompanyRetentionPercentage, c.Currency, c.BordereauBatchId, c.CreatedAt);

    private static bool IsEligible(ReinsuranceAgreement agreement, Policy policy)
    {
        if (policy.EffectiveFrom < agreement.EffectiveFrom || policy.EffectiveFrom > agreement.EffectiveTo) return false;
        var duration = policy.EffectiveTo.DayNumber - policy.EffectiveFrom.DayNumber + 1;
        if (agreement.MaxTripDurationDays.HasValue && duration > agreement.MaxTripDurationDays) return false;
        if (agreement.MinAge.HasValue || agreement.MaxAge.HasValue)
        {
            var ages = policy.Passengers.Select(p => AgeAt(p.DateOfBirth, policy.EffectiveFrom));
            if (ages.Any(age => (agreement.MinAge.HasValue && age < agreement.MinAge) || (agreement.MaxAge.HasValue && age > agreement.MaxAge))) return false;
        }
        var grossSumInsured = policy.CoverageSnapshots.Where(s => string.Equals(s.Currency, policy.Currency, StringComparison.OrdinalIgnoreCase)).Select(s => s.Limit ?? 0m).DefaultIfEmpty(0m).Max();
        if (agreement.MaxCoverageLimit.HasValue && grossSumInsured > agreement.MaxCoverageLimit) return false;
        return agreement.CoverageZones.Count == 0 || agreement.CoverageZones.Any(z => z.CoverageZone is { IsActive: true } zone && ZoneCovers(zone, policy.DestinationCountryId!.Value));
    }

    private static bool ZoneCovers(CoverageZone zone, Guid countryId) { var rule = zone.Countries.SingleOrDefault(c => c.CountryId == countryId); return zone.IsWorldwide ? rule is null || !rule.IsExcluded : rule is { IsExcluded: false }; }
    private static int AgeAt(DateOnly dob, DateOnly date) { var age = date.Year - dob.Year; return dob.AddYears(age) > date ? age - 1 : age; }
    private static decimal ShareOf(decimal amount, decimal share) => Math.Round(amount * share / 100m, 2, MidpointRounding.AwayFromZero);
}
