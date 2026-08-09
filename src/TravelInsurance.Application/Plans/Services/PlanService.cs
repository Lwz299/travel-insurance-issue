using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Plans.Services;

public class PlanService(
    IPlanRepository planRepository,
    ICompanyRepository companyRepository,
    ICountryRepository countryRepository,
    ICoverageZoneRepository coverageZoneRepository,
    IAuditLogRepository auditLogRepository) : IPlanService
{
    public async Task<IReadOnlyCollection<PlanSummaryResponse>> GetAllAsync(Guid? companyId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var plans = await planRepository.GetAllAsync(companyId, isActive, cancellationToken);
        return plans.Select(p => new PlanSummaryResponse(
            p.Id, p.CompanyId, p.Company?.Name ?? string.Empty, p.Name,
            p.CoverageType, p.IsActive, p.CreatedAt, p.UpdatedAt)).ToArray();
    }

    public async Task<PlanResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await planRepository.GetByIdWithGraphAsync(id, cancellationToken);
        return plan is null ? null : Map(plan);
    }

    public async Task<(PlanResponse? Plan, OperationResponse Result)> CreateAsync(CreatePlanRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(request.CompanyId, request.MinAge, request.MaxAge, request.AllowsQuotation, request.AllowsDirectIssuance,
            request.Countries, request.CoverageZoneIds, request.PricingRules, cancellationToken);
        if (!validation.Success)
        {
            return (null, validation);
        }

        var plan = new InsurancePlan
        {
            CompanyId = request.CompanyId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CoverageType = request.CoverageType,
            DefaultDurationDays = request.DefaultDurationDays,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge,
            MaxTripDurationDays = request.MaxTripDurationDays,
            AllowsQuotation = request.AllowsQuotation,
            AllowsDirectIssuance = request.AllowsDirectIssuance,
            IsActive = true
        };

        ApplyChildren(plan, request.Countries, request.CoverageZoneIds, request.PricingRules);

        await planRepository.AddAsync(plan, cancellationToken);
        await WriteAuditAsync(actorUserId, "PLAN.CREATED", plan.Id, ipAddress, userAgent, cancellationToken);

        var created = await planRepository.GetByIdWithGraphAsync(plan.Id, cancellationToken);
        return (created is null ? null : Map(created), new OperationResponse { Success = true, Message = "Plan created." });
    }

    public async Task<(PlanResponse? Plan, OperationResponse Result)> UpdateAsync(Guid id, UpdatePlanRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var plan = await planRepository.GetByIdWithGraphAsync(id, cancellationToken);
        if (plan is null)
        {
            return (null, new OperationResponse { Success = false, Message = "Plan not found." });
        }

        var validation = await ValidateAsync(plan.CompanyId, request.MinAge, request.MaxAge, request.AllowsQuotation, request.AllowsDirectIssuance,
            request.Countries, request.CoverageZoneIds, request.PricingRules, cancellationToken);
        if (!validation.Success)
        {
            return (null, validation);
        }

        plan.Name = request.Name.Trim();
        plan.Description = request.Description?.Trim();
        plan.CoverageType = request.CoverageType;
        plan.DefaultDurationDays = request.DefaultDurationDays;
        plan.MinAge = request.MinAge;
        plan.MaxAge = request.MaxAge;
        plan.MaxTripDurationDays = request.MaxTripDurationDays;
        plan.AllowsQuotation = request.AllowsQuotation;
        plan.AllowsDirectIssuance = request.AllowsDirectIssuance;
        plan.IsActive = request.IsActive;

        plan.Countries.Clear();
        plan.CoverageZones.Clear();
        plan.PricingRules.Clear();
        ApplyChildren(plan, request.Countries, request.CoverageZoneIds, request.PricingRules);

        await planRepository.UpdateAsync(plan, cancellationToken);
        await WriteAuditAsync(actorUserId, "PLAN.UPDATED", plan.Id, ipAddress, userAgent, cancellationToken);

        var updated = await planRepository.GetByIdWithGraphAsync(plan.Id, cancellationToken);
        return (updated is null ? null : Map(updated), new OperationResponse { Success = true, Message = "Plan updated." });
    }

    public async Task<OperationResponse> SetStatusAsync(Guid id, bool isActive, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var plan = await planRepository.GetByIdAsync(id, cancellationToken);
        if (plan is null)
        {
            return new OperationResponse { Success = false, Message = "Plan not found." };
        }

        plan.IsActive = isActive;
        await planRepository.UpdateAsync(plan, cancellationToken);
        await WriteAuditAsync(actorUserId, isActive ? "PLAN.ACTIVATED" : "PLAN.DEACTIVATED", plan.Id, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = isActive ? "Plan activated." : "Plan deactivated." };
    }

    public async Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var plan = await planRepository.GetByIdAsync(id, cancellationToken);
        if (plan is null)
        {
            return new OperationResponse { Success = false, Message = "Plan not found." };
        }

        plan.IsDeleted = true;
        plan.IsActive = false;
        await planRepository.UpdateAsync(plan, cancellationToken);
        await WriteAuditAsync(actorUserId, "PLAN.DELETED", plan.Id, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Plan deleted." };
    }

    private async Task<OperationResponse> ValidateAsync(
        Guid companyId, int? minAge, int? maxAge, bool allowsQuotation, bool allowsDirectIssuance,
        List<PlanCountryInput> countries, List<Guid> coverageZoneIds, List<PricingRuleInput> pricingRules,
        CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            return new OperationResponse { Success = false, Message = "Company not found." };
        }

        if (company.Type != CompanyType.Insurance)
        {
            return new OperationResponse { Success = false, Message = "Plans can only belong to an insurance company." };
        }

        if (minAge.HasValue && maxAge.HasValue && minAge > maxAge)
        {
            return new OperationResponse { Success = false, Message = "MinAge cannot be greater than MaxAge." };
        }

        if (!allowsQuotation && !allowsDirectIssuance)
        {
            return new OperationResponse { Success = false, Message = "A plan must allow quotation, direct issuance, or both." };
        }

        foreach (var rule in pricingRules)
        {
            if (rule.MinAge > rule.MaxAge)
            {
                return new OperationResponse { Success = false, Message = "Pricing rule MinAge cannot be greater than MaxAge." };
            }

            if (rule.DurationFrom > rule.DurationTo)
            {
                return new OperationResponse { Success = false, Message = "Pricing rule DurationFrom cannot be greater than DurationTo." };
            }
        }

        for (var i = 0; i < pricingRules.Count; i++)
        for (var j = i + 1; j < pricingRules.Count; j++)
        {
            var first = pricingRules[i];
            var second = pricingRules[j];
            var agesOverlap = first.MinAge <= second.MaxAge && second.MinAge <= first.MaxAge;
            var durationsOverlap = first.DurationFrom <= second.DurationTo && second.DurationFrom <= first.DurationTo;
            if (agesOverlap && durationsOverlap)
            {
                return new OperationResponse { Success = false, Message = "Pricing rules must not overlap by age and duration." };
            }
        }

        var countryIds = countries.Select(c => c.CountryId).ToList();
        if (countryIds.Count != countryIds.Distinct().Count())
        {
            return new OperationResponse { Success = false, Message = "Duplicate countries are not allowed." };
        }

        if (countryIds.Count > 0)
        {
            var existing = await countryRepository.GetExistingIdsAsync(countryIds, cancellationToken);
            var missing = countryIds.Where(cid => !existing.Contains(cid)).ToList();
            if (missing.Count > 0)
            {
                return new OperationResponse { Success = false, Message = "One or more countries do not exist." };
            }
        }

        if (coverageZoneIds.Count != coverageZoneIds.Distinct().Count())
        {
            return new OperationResponse { Success = false, Message = "Duplicate coverage zones are not allowed." };
        }

        if (coverageZoneIds.Count > 0)
        {
            var existing = await coverageZoneRepository.GetExistingIdsAsync(coverageZoneIds, cancellationToken);
            if (existing.Count != coverageZoneIds.Count)
            {
                return new OperationResponse { Success = false, Message = "One or more coverage zones do not exist or are inactive." };
            }
        }

        return new OperationResponse { Success = true, Message = "Valid." };
    }

    private static void ApplyChildren(InsurancePlan plan, List<PlanCountryInput> countries, List<Guid> coverageZoneIds, List<PricingRuleInput> pricingRules)
    {
        foreach (var pc in countries)
        {
            plan.Countries.Add(new PlanCountry { CountryId = pc.CountryId, IsExcluded = pc.IsExcluded });
        }

        foreach (var coverageZoneId in coverageZoneIds)
        {
            plan.CoverageZones.Add(new PlanCoverageZone { CoverageZoneId = coverageZoneId });
        }

        foreach (var r in pricingRules)
        {
            plan.PricingRules.Add(new PricingRule
            {
                MinAge = r.MinAge,
                MaxAge = r.MaxAge,
                DurationFrom = r.DurationFrom,
                DurationTo = r.DurationTo,
                Price = r.Price,
                Currency = r.Currency.Trim().ToUpperInvariant()
            });
        }
    }

    private async Task WriteAuditAsync(Guid? actorUserId, string action, Guid entityId, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "plans",
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);
    }

    private static PlanResponse Map(InsurancePlan p) => new(
        p.Id, p.CompanyId, p.Company?.Name ?? string.Empty, p.Name, p.Description, p.CoverageType,
        p.DefaultDurationDays, p.MinAge, p.MaxAge, p.MaxTripDurationDays, p.AllowsQuotation, p.AllowsDirectIssuance, p.IsActive, p.CreatedAt, p.UpdatedAt,
        p.Countries.Select(pc => new PlanCountryResponse(
            pc.Id, pc.CountryId, pc.Country?.Code ?? string.Empty, pc.Country?.Name ?? string.Empty, pc.IsExcluded)).ToArray(),
        p.CoverageZones.Select(pz => new PlanCoverageZoneResponse(
            pz.Id, pz.CoverageZoneId, pz.CoverageZone?.Code ?? string.Empty, pz.CoverageZone?.Name ?? string.Empty,
            pz.CoverageZone?.IsWorldwide ?? false, pz.CoverageZone?.IsActive ?? false)).ToArray(),
        p.PricingRules.Select(r => new PricingRuleResponse(
            r.Id, r.MinAge, r.MaxAge, r.DurationFrom, r.DurationTo, r.Price, r.Currency)).ToArray(),
        p.CoverageAssignments.OrderBy(a => a.DisplayOrder).Select(PlanCoverageMappers.MapAssignment).ToArray());
}
