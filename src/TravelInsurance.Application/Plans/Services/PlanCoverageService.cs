using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Plans.Services;

public class PlanCoverageService(
    IPlanCoverageRepository repository,
    ICatalogRepository catalogRepository,
    IAuditLogRepository auditLogRepository) : IPlanCoverageService
{
    public async Task<IReadOnlyCollection<PlanCoverageAssignmentResponse>> GetByPlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await repository.GetPlanWithCoveragesAsync(planId, cancellationToken);
        if (plan is null) return Array.Empty<PlanCoverageAssignmentResponse>();
        return plan.CoverageAssignments.OrderBy(x => x.DisplayOrder).Select(PlanCoverageMappers.MapAssignment).ToArray();
    }

    public async Task<OperationResponse> AssignAsync(Guid planId, AssignPlanCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        if (!await repository.PlanExistsAsync(planId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Plan not found." };
        }

        if (!await repository.CoverageExistsAndActiveAsync(request.CoverageId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Coverage not found or inactive." };
        }

        if (await repository.AssignmentExistsAsync(planId, request.CoverageId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Coverage already assigned to this plan." };
        }

        var assignment = new PlanCoverageAssignment
        {
            PlanId = planId,
            CoverageId = request.CoverageId,
            CustomLimit = request.CustomLimit,
            Currency = request.Currency?.Trim().ToUpperInvariant(),
            CustomDeductible = request.CustomDeductible,
            IsEnabled = request.IsEnabled,
            DisplayOrder = request.DisplayOrder
        };

        await repository.AddAssignmentAsync(assignment, cancellationToken);
        await AuditAsync(actorUserId, "PLAN.COVERAGE_ASSIGNED", planId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Coverage assigned to plan." };
    }

    public async Task<OperationResponse> UpdateAsync(Guid planId, Guid coverageId, UpdatePlanCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var assignment = await repository.GetAssignmentAsync(planId, coverageId, cancellationToken);
        if (assignment is null)
        {
            return new OperationResponse { Success = false, Message = "Plan coverage assignment not found." };
        }

        assignment.CustomLimit = request.CustomLimit;
        assignment.Currency = request.Currency?.Trim().ToUpperInvariant();
        assignment.CustomDeductible = request.CustomDeductible;
        assignment.IsEnabled = request.IsEnabled;
        assignment.DisplayOrder = request.DisplayOrder;

        await repository.SaveChangesAsync(cancellationToken);
        await AuditAsync(actorUserId, "PLAN.COVERAGE_UPDATED", planId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Plan coverage updated." };
    }

    public async Task<OperationResponse> RemoveAsync(Guid planId, Guid coverageId, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var assignment = await repository.GetAssignmentAsync(planId, coverageId, cancellationToken);
        if (assignment is null)
        {
            return new OperationResponse { Success = false, Message = "Plan coverage assignment not found." };
        }

        await repository.RemoveAssignmentAsync(assignment, cancellationToken);
        await AuditAsync(actorUserId, "PLAN.COVERAGE_REMOVED", planId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Coverage removed from plan." };
    }

    public async Task<OperationResponse> UpsertBenefitOverrideAsync(Guid planId, Guid coverageId, UpsertPlanBenefitOverrideRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var assignment = await repository.GetAssignmentAsync(planId, coverageId, cancellationToken);
        if (assignment is null)
        {
            return new OperationResponse { Success = false, Message = "Plan coverage assignment not found." };
        }

        if (await catalogRepository.GetBenefitByIdAsync(request.BenefitId, cancellationToken) is null)
        {
            return new OperationResponse { Success = false, Message = "Benefit not found." };
        }

        if (!await catalogRepository.LimitTypeExistsAsync(request.LimitTypeId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Limit type not found." };
        }

        var existing = await repository.GetOverrideAsync(assignment.Id, request.BenefitId, cancellationToken);
        if (existing is null)
        {
            assignment.BenefitOverrides.Add(new PlanCoverageBenefitOverride
            {
                PlanCoverageId = assignment.Id,
                BenefitId = request.BenefitId,
                LimitTypeId = request.LimitTypeId,
                LimitOverride = request.LimitOverride,
                Currency = request.Currency?.Trim().ToUpperInvariant(),
                DeductibleOverride = request.DeductibleOverride,
                IsEnabled = request.IsEnabled
            });
        }
        else
        {
            existing.LimitTypeId = request.LimitTypeId;
            existing.LimitOverride = request.LimitOverride;
            existing.Currency = request.Currency?.Trim().ToUpperInvariant();
            existing.DeductibleOverride = request.DeductibleOverride;
            existing.IsEnabled = request.IsEnabled;
        }

        await repository.SaveChangesAsync(cancellationToken);
        await AuditAsync(actorUserId, "PLAN.BENEFIT_OVERRIDE_UPSERTED", planId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Benefit override saved." };
    }

    public async Task<OperationResponse> RemoveBenefitOverrideAsync(Guid planId, Guid coverageId, Guid benefitId, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var assignment = await repository.GetAssignmentAsync(planId, coverageId, cancellationToken);
        if (assignment is null)
        {
            return new OperationResponse { Success = false, Message = "Plan coverage assignment not found." };
        }

        var existing = assignment.BenefitOverrides.FirstOrDefault(x => x.BenefitId == benefitId);
        if (existing is null)
        {
            return new OperationResponse { Success = false, Message = "Benefit override not found." };
        }

        assignment.BenefitOverrides.Remove(existing);
        await repository.SaveChangesAsync(cancellationToken);
        await AuditAsync(actorUserId, "PLAN.BENEFIT_OVERRIDE_REMOVED", planId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Benefit override removed." };
    }

    private async Task AuditAsync(Guid? actorUserId, string action, Guid entityId, string? ip, string? ua, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "plans",
            EntityId = entityId,
            IpAddress = ip,
            UserAgent = ua
        }, cancellationToken);
    }
}

internal static class PlanCoverageMappers
{
    public static PlanCoverageAssignmentResponse MapAssignment(PlanCoverageAssignment a) => new(
        a.Id, a.CoverageId, a.Coverage?.Code ?? string.Empty, a.Coverage?.Name ?? string.Empty,
        a.Coverage?.Group?.Name ?? string.Empty,
        a.CustomLimit, a.Currency, a.CustomDeductible, a.IsEnabled, a.DisplayOrder,
        a.BenefitOverrides.Select(o => new PlanBenefitOverrideResponse(
            o.Id, o.BenefitId, o.Benefit?.Code ?? string.Empty, o.Benefit?.Name ?? string.Empty,
            o.LimitTypeId, o.LimitType?.Name ?? string.Empty,
            o.LimitOverride, o.Currency, o.DeductibleOverride, o.IsEnabled)).ToArray());
}
