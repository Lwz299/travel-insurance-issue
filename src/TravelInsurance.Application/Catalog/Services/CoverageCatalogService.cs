using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Catalog.Services;

public class CoverageCatalogService(
    ICatalogRepository repository,
    IAuditLogRepository auditLogRepository) : ICoverageCatalogService
{
    public async Task<IReadOnlyCollection<CoverageResponse>> GetAllAsync(Guid? groupId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCoveragesAsync(groupId, cancellationToken);
        return items.Select(CatalogMappers.MapCoverage).ToArray();
    }

    public async Task<CoverageDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coverage = await repository.GetCoverageWithBenefitsAsync(id, cancellationToken);
        return coverage is null ? null : CatalogMappers.MapCoverageDetail(coverage);
    }

    public async Task<(CoverageResponse? Coverage, OperationResponse Result)> CreateAsync(CreateCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateCoverageRefsAsync(request.GroupId, request.RiskTypeId, request.Code, null, cancellationToken);
        if (!validation.Success) return (null, validation);

        var coverage = new Coverage
        {
            GroupId = request.GroupId,
            RiskTypeId = request.RiskTypeId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsMandatory = request.IsMandatory
        };

        await repository.AddCoverageAsync(coverage, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.COVERAGE_CREATED", coverage.Id, ip, ua, cancellationToken);

        var created = await repository.GetCoverageByIdAsync(coverage.Id, cancellationToken);
        return (created is null ? null : CatalogMappers.MapCoverage(created), new OperationResponse { Success = true, Message = "Coverage created." });
    }

    public async Task<(CoverageResponse? Coverage, OperationResponse Result)> UpdateAsync(Guid id, UpdateCoverageRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var coverage = await repository.GetCoverageByIdAsync(id, cancellationToken);
        if (coverage is null)
        {
            return (null, new OperationResponse { Success = false, Message = "Coverage not found." });
        }

        var group = await repository.GetGroupByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
        {
            return (null, new OperationResponse { Success = false, Message = "Coverage group not found." });
        }

        if (!await repository.RiskTypeExistsAsync(request.RiskTypeId, cancellationToken))
        {
            return (null, new OperationResponse { Success = false, Message = "Risk type not found." });
        }

        coverage.GroupId = request.GroupId;
        coverage.RiskTypeId = request.RiskTypeId;
        coverage.Name = request.Name.Trim();
        coverage.Description = request.Description?.Trim();
        coverage.IsMandatory = request.IsMandatory;
        coverage.IsActive = request.IsActive;
        coverage.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.UpdateCoverageAsync(coverage, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.COVERAGE_UPDATED", coverage.Id, ip, ua, cancellationToken);

        var updated = await repository.GetCoverageByIdAsync(coverage.Id, cancellationToken);
        return (updated is null ? null : CatalogMappers.MapCoverage(updated), new OperationResponse { Success = true, Message = "Coverage updated." });
    }

    public async Task<OperationResponse> SetStatusAsync(Guid id, bool isActive, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var coverage = await repository.GetCoverageByIdAsync(id, cancellationToken);
        if (coverage is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage not found." };
        }

        coverage.IsActive = isActive;
        coverage.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.UpdateCoverageAsync(coverage, cancellationToken);
        await AuditAsync(actorUserId, isActive ? "CATALOG.COVERAGE_ACTIVATED" : "CATALOG.COVERAGE_DEACTIVATED", coverage.Id, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = isActive ? "Coverage activated." : "Coverage deactivated." };
    }

    public async Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var coverage = await repository.GetCoverageByIdAsync(id, cancellationToken);
        if (coverage is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage not found." };
        }

        if (await repository.CoverageIsAssignedToPlansAsync(id, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Cannot delete coverage assigned to plans." };
        }

        coverage.IsDeleted = true;
        coverage.IsActive = false;
        coverage.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.UpdateCoverageAsync(coverage, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.COVERAGE_DELETED", coverage.Id, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Coverage deleted." };
    }

    private async Task<OperationResponse> ValidateCoverageRefsAsync(Guid groupId, int riskTypeId, string code, Guid? excludeId, CancellationToken cancellationToken)
    {
        if (await repository.GetGroupByIdAsync(groupId, cancellationToken) is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage group not found." };
        }

        if (!await repository.RiskTypeExistsAsync(riskTypeId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Risk type not found." };
        }

        if (await repository.CoverageCodeExistsAsync(code.Trim(), excludeId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Coverage code already exists." };
        }

        return new OperationResponse { Success = true, Message = "Valid." };
    }

    private async Task AuditAsync(Guid? actorUserId, string action, Guid entityId, string? ip, string? ua, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "coverages",
            EntityId = entityId,
            IpAddress = ip,
            UserAgent = ua
        }, cancellationToken);
    }
}
