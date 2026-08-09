using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Catalog.Services;

public class CoverageBenefitMappingService(
    ICatalogRepository repository,
    IAuditLogRepository auditLogRepository) : ICoverageBenefitMappingService
{
    public async Task<IReadOnlyCollection<CoverageBenefitResponse>> GetByCoverageAsync(Guid coverageId, CancellationToken cancellationToken = default)
    {
        var coverage = await repository.GetCoverageWithBenefitsAsync(coverageId, cancellationToken);
        if (coverage is null) return Array.Empty<CoverageBenefitResponse>();
        return coverage.CoverageBenefits.OrderBy(x => x.DisplayOrder).Select(CatalogMappers.MapCoverageBenefit).ToArray();
    }

    public async Task<OperationResponse> AddAsync(Guid coverageId, AddCoverageBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var coverage = await repository.GetCoverageByIdAsync(coverageId, cancellationToken);
        if (coverage is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage not found." };
        }

        if (await repository.GetBenefitByIdAsync(request.BenefitId, cancellationToken) is null)
        {
            return new OperationResponse { Success = false, Message = "Benefit not found." };
        }

        if (!await repository.LimitTypeExistsAsync(request.LimitTypeId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Limit type not found." };
        }

        if (await repository.GetCoverageBenefitAsync(coverageId, request.BenefitId, cancellationToken) is not null)
        {
            return new OperationResponse { Success = false, Message = "Benefit already mapped to this coverage." };
        }

        var mapping = new CoverageBenefit
        {
            CoverageId = coverageId,
            BenefitId = request.BenefitId,
            LimitTypeId = request.LimitTypeId,
            LimitValue = request.LimitValue,
            Currency = request.Currency?.Trim().ToUpperInvariant(),
            Deductible = request.Deductible,
            WaitingPeriodDays = request.WaitingPeriodDays,
            DisplayOrder = request.DisplayOrder,
            IsOptional = request.IsOptional
        };

        await repository.AddCoverageBenefitAsync(mapping, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.COVERAGE_BENEFIT_ADDED", coverageId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Benefit mapped to coverage." };
    }

    public async Task<OperationResponse> UpdateAsync(Guid coverageId, Guid benefitId, UpdateCoverageBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var mapping = await repository.GetCoverageBenefitAsync(coverageId, benefitId, cancellationToken);
        if (mapping is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage-benefit mapping not found." };
        }

        if (!await repository.LimitTypeExistsAsync(request.LimitTypeId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Limit type not found." };
        }

        mapping.LimitTypeId = request.LimitTypeId;
        mapping.LimitValue = request.LimitValue;
        mapping.Currency = request.Currency?.Trim().ToUpperInvariant();
        mapping.Deductible = request.Deductible;
        mapping.WaitingPeriodDays = request.WaitingPeriodDays;
        mapping.DisplayOrder = request.DisplayOrder;
        mapping.IsOptional = request.IsOptional;

        await repository.UpdateCoverageBenefitAsync(mapping, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.COVERAGE_BENEFIT_UPDATED", coverageId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Coverage-benefit mapping updated." };
    }

    public async Task<OperationResponse> RemoveAsync(Guid coverageId, Guid benefitId, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var mapping = await repository.GetCoverageBenefitAsync(coverageId, benefitId, cancellationToken);
        if (mapping is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage-benefit mapping not found." };
        }

        await repository.RemoveCoverageBenefitAsync(mapping, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.COVERAGE_BENEFIT_REMOVED", coverageId, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Benefit removed from coverage." };
    }

    private async Task AuditAsync(Guid? actorUserId, string action, Guid entityId, string? ip, string? ua, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "coverage_benefits",
            EntityId = entityId,
            IpAddress = ip,
            UserAgent = ua
        }, cancellationToken);
    }
}
