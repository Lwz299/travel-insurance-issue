using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.DTOs;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Reinsurance.Services;

public class ReinsuranceAgreementService(
    IReinsuranceRepository reinsuranceRepository,
    ICompanyRepository companyRepository,
    IPlanRepository planRepository,
    ICoverageZoneRepository coverageZoneRepository) : IReinsuranceAgreementService
{
    public async Task<IReadOnlyCollection<ReinsuranceAgreementResponse>> GetAllAsync(Guid? companyId, CancellationToken cancellationToken = default) =>
        (await reinsuranceRepository.GetAgreementsAsync(companyId, cancellationToken)).Select(Map).ToArray();

    public async Task<ReinsuranceAgreementResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agreement = await reinsuranceRepository.GetAgreementAsync(id, cancellationToken);
        return agreement is null ? null : Map(agreement);
    }

    public async Task<(ReinsuranceAgreementResponse? Agreement, OperationResponse Result)> CreateAsync(CreateReinsuranceAgreementRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(request, null, cancellationToken);
        if (!validation.Success) return (null, validation);
        var agreement = new ReinsuranceAgreement();
        Apply(agreement, request, true);
        reinsuranceRepository.AddAgreement(agreement);
        await reinsuranceRepository.SaveChangesAsync(cancellationToken);
        return (Map(agreement), Ok("Reinsurance agreement created."));
    }

    public async Task<(ReinsuranceAgreementResponse? Agreement, OperationResponse Result)> UpdateAsync(Guid id, UpdateReinsuranceAgreementRequest request, CancellationToken cancellationToken = default)
    {
        var agreement = await reinsuranceRepository.GetAgreementAsync(id, cancellationToken);
        if (agreement is null) return (null, Fail("Reinsurance agreement not found."));
        var validation = await ValidateAsync(request, id, cancellationToken);
        if (!validation.Success) return (null, validation);
        Apply(agreement, request, request.IsActive);
        agreement.UpdatedAt = DateTimeOffset.UtcNow;
        await reinsuranceRepository.SaveChangesAsync(cancellationToken);
        return (Map(agreement), Ok("Reinsurance agreement updated."));
    }

    private async Task<OperationResponse> ValidateAsync(CreateReinsuranceAgreementRequest request, Guid? currentId, CancellationToken ct)
    {
        if (request.EffectiveTo < request.EffectiveFrom) return Fail("Agreement end date cannot be before its start date.");
        if (request.MinAge.HasValue && request.MaxAge.HasValue && request.MinAge > request.MaxAge) return Fail("Agreement MinAge cannot be greater than MaxAge.");
        if (request.ReportingMode == ReinsuranceReportingMode.PeriodicBordereau && !request.ReportingFrequency.HasValue) return Fail("A periodic bordereau agreement requires a reporting frequency.");
        if (request.RequiresPreApproval || request.Type == ReinsuranceAgreementType.Facultative)
            return Fail("Pre-approval and facultative agreements require the Special Acceptance workflow, which is not enabled in this automatic cession phase.");
        if (request.PlanIds.Count != request.PlanIds.Distinct().Count() || request.CoverageZoneIds.Count != request.CoverageZoneIds.Distinct().Count()) return Fail("Duplicate plan or coverage-zone assignments are not allowed.");

        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company is null || !company.IsActive || company.Type != CompanyType.Insurance) return Fail("Agreement company must be an active insurance company.");
        var reinsurer = await companyRepository.GetByIdAsync(request.ReinsurerCompanyId, ct);
        if (reinsurer is null || !reinsurer.IsActive || reinsurer.Type != CompanyType.Reinsurer) return Fail("Reinsurer must be an active reinsurer company.");
        if (request.CompanyId == request.ReinsurerCompanyId) return Fail("The insurer and reinsurer must be different companies.");

        foreach (var planId in request.PlanIds)
        {
            var plan = await planRepository.GetByIdAsync(planId, ct);
            if (plan is null || plan.CompanyId != request.CompanyId) return Fail("Every assigned plan must belong to the agreement insurance company.");
        }

        var zones = await coverageZoneRepository.GetExistingIdsAsync(request.CoverageZoneIds, ct);
        if (zones.Count != request.CoverageZoneIds.Count) return Fail("One or more coverage zones do not exist or are inactive.");

        var duplicateNumber = (await reinsuranceRepository.GetAgreementsAsync(request.CompanyId, ct))
            .Any(a => a.Id != currentId && string.Equals(a.AgreementNumber, request.AgreementNumber.Trim(), StringComparison.OrdinalIgnoreCase));
        return duplicateNumber ? Fail("Agreement number already exists for this company.") : Ok("Valid.");
    }

    private static void Apply(ReinsuranceAgreement agreement, CreateReinsuranceAgreementRequest request, bool isActive)
    {
        agreement.CompanyId = request.CompanyId;
        agreement.ReinsurerCompanyId = request.ReinsurerCompanyId;
        agreement.AgreementNumber = request.AgreementNumber.Trim().ToUpperInvariant();
        agreement.AgreementName = request.AgreementName.Trim();
        agreement.Type = request.Type;
        agreement.EffectiveFrom = request.EffectiveFrom;
        agreement.EffectiveTo = request.EffectiveTo;
        agreement.ReportingMode = request.ReportingMode;
        agreement.ReportingFrequency = request.ReportingFrequency;
        agreement.RequiresPreApproval = request.RequiresPreApproval;
        agreement.CededSharePercentage = request.CededSharePercentage;
        agreement.MaxTripDurationDays = request.MaxTripDurationDays;
        agreement.MaxCoverageLimit = request.MaxCoverageLimit;
        agreement.MinAge = request.MinAge;
        agreement.MaxAge = request.MaxAge;
        agreement.Priority = request.Priority;
        agreement.IsActive = isActive;
        agreement.PlanAgreements.Clear();
        agreement.CoverageZones.Clear();
        foreach (var planId in request.PlanIds) agreement.PlanAgreements.Add(new PlanReinsuranceAgreement { PlanId = planId });
        foreach (var zoneId in request.CoverageZoneIds) agreement.CoverageZones.Add(new ReinsuranceAgreementCoverageZone { CoverageZoneId = zoneId });
    }

    private static ReinsuranceAgreementResponse Map(ReinsuranceAgreement a) => new(
        a.Id, a.CompanyId, a.ReinsurerCompanyId, a.AgreementNumber, a.AgreementName, a.Type, a.EffectiveFrom, a.EffectiveTo,
        a.ReportingMode, a.ReportingFrequency, a.RequiresPreApproval, a.CededSharePercentage, a.MaxTripDurationDays,
        a.MaxCoverageLimit, a.MinAge, a.MaxAge, a.Priority, a.IsActive,
        a.PlanAgreements.Select(p => p.PlanId).ToArray(), a.CoverageZones.Select(z => z.CoverageZoneId).ToArray());
    private static OperationResponse Ok(string message) => new() { Success = true, Message = message };
    private static OperationResponse Fail(string message) => new() { Success = false, Message = message };
}
