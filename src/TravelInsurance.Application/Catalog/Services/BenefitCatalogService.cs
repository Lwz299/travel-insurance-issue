using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Catalog.Services;

public class BenefitCatalogService(
    ICatalogRepository repository,
    IAuditLogRepository auditLogRepository) : IBenefitCatalogService
{
    public async Task<IReadOnlyCollection<BenefitResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetBenefitsAsync(cancellationToken);
        return items.Select(CatalogMappers.MapBenefit).ToArray();
    }

    public async Task<BenefitResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var benefit = await repository.GetBenefitByIdAsync(id, cancellationToken);
        return benefit is null ? null : CatalogMappers.MapBenefit(benefit);
    }

    public async Task<(BenefitResponse? Benefit, OperationResponse Result)> CreateAsync(CreateBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        if (!await repository.BenefitTypeExistsAsync(request.BenefitTypeId, cancellationToken))
        {
            return (null, new OperationResponse { Success = false, Message = "Benefit type not found." });
        }

        if (await repository.BenefitCodeExistsAsync(request.Code.Trim(), null, cancellationToken))
        {
            return (null, new OperationResponse { Success = false, Message = "Benefit code already exists." });
        }

        var benefit = new Benefit
        {
            BenefitTypeId = request.BenefitTypeId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        await repository.AddBenefitAsync(benefit, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.BENEFIT_CREATED", benefit.Id, ip, ua, cancellationToken);

        var created = await repository.GetBenefitByIdAsync(benefit.Id, cancellationToken);
        return (created is null ? null : CatalogMappers.MapBenefit(created), new OperationResponse { Success = true, Message = "Benefit created." });
    }

    public async Task<(BenefitResponse? Benefit, OperationResponse Result)> UpdateAsync(Guid id, UpdateBenefitRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var benefit = await repository.GetBenefitByIdAsync(id, cancellationToken);
        if (benefit is null)
        {
            return (null, new OperationResponse { Success = false, Message = "Benefit not found." });
        }

        if (!await repository.BenefitTypeExistsAsync(request.BenefitTypeId, cancellationToken))
        {
            return (null, new OperationResponse { Success = false, Message = "Benefit type not found." });
        }

        benefit.BenefitTypeId = request.BenefitTypeId;
        benefit.Name = request.Name.Trim();
        benefit.Description = request.Description?.Trim();
        benefit.IsActive = request.IsActive;
        benefit.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.UpdateBenefitAsync(benefit, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.BENEFIT_UPDATED", benefit.Id, ip, ua, cancellationToken);

        var updated = await repository.GetBenefitByIdAsync(benefit.Id, cancellationToken);
        return (updated is null ? null : CatalogMappers.MapBenefit(updated), new OperationResponse { Success = true, Message = "Benefit updated." });
    }

    public async Task<OperationResponse> SetStatusAsync(Guid id, bool isActive, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var benefit = await repository.GetBenefitByIdAsync(id, cancellationToken);
        if (benefit is null)
        {
            return new OperationResponse { Success = false, Message = "Benefit not found." };
        }

        benefit.IsActive = isActive;
        benefit.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.UpdateBenefitAsync(benefit, cancellationToken);
        await AuditAsync(actorUserId, isActive ? "CATALOG.BENEFIT_ACTIVATED" : "CATALOG.BENEFIT_DEACTIVATED", benefit.Id, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = isActive ? "Benefit activated." : "Benefit deactivated." };
    }

    public async Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var benefit = await repository.GetBenefitByIdAsync(id, cancellationToken);
        if (benefit is null)
        {
            return new OperationResponse { Success = false, Message = "Benefit not found." };
        }

        if (await repository.BenefitIsInUseAsync(id, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Cannot delete benefit while in use." };
        }

        benefit.IsDeleted = true;
        benefit.IsActive = false;
        benefit.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.UpdateBenefitAsync(benefit, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.BENEFIT_DELETED", benefit.Id, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Benefit deleted." };
    }

    private async Task AuditAsync(Guid? actorUserId, string action, Guid entityId, string? ip, string? ua, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "benefits",
            EntityId = entityId,
            IpAddress = ip,
            UserAgent = ua
        }, cancellationToken);
    }
}
