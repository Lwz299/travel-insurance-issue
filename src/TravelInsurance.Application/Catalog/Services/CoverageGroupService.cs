using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Catalog.Services;

public class CoverageGroupService(
    ICatalogRepository repository,
    IAuditLogRepository auditLogRepository) : ICoverageGroupService
{
    public async Task<IReadOnlyCollection<CoverageGroupResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var groups = await repository.GetGroupsAsync(cancellationToken);
        return groups.Select(Map).ToArray();
    }

    public async Task<CoverageGroupResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupByIdAsync(id, cancellationToken);
        return group is null ? null : Map(group);
    }

    public async Task<(CoverageGroupResponse? Group, OperationResponse Result)> CreateAsync(CreateCoverageGroupRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        if (await repository.GroupCodeExistsAsync(request.Code.Trim(), null, cancellationToken))
        {
            return (null, new OperationResponse { Success = false, Message = "Group code already exists." });
        }

        var group = new CoverageGroup
        {
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            SortOrder = request.SortOrder
        };

        await repository.AddGroupAsync(group, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.GROUP_CREATED", group.Id, ip, ua, cancellationToken);
        return (Map(group), new OperationResponse { Success = true, Message = "Coverage group created." });
    }

    public async Task<(CoverageGroupResponse? Group, OperationResponse Result)> UpdateAsync(Guid id, UpdateCoverageGroupRequest request, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupByIdAsync(id, cancellationToken);
        if (group is null)
        {
            return (null, new OperationResponse { Success = false, Message = "Coverage group not found." });
        }

        group.Name = request.Name.Trim();
        group.SortOrder = request.SortOrder;
        group.IsActive = request.IsActive;
        group.UpdatedAt = DateTimeOffset.UtcNow;

        await repository.UpdateGroupAsync(group, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.GROUP_UPDATED", group.Id, ip, ua, cancellationToken);
        return (Map(group), new OperationResponse { Success = true, Message = "Coverage group updated." });
    }

    public async Task<OperationResponse> DeleteAsync(Guid id, Guid? actorUserId, string? ip, string? ua, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupByIdAsync(id, cancellationToken);
        if (group is null)
        {
            return new OperationResponse { Success = false, Message = "Coverage group not found." };
        }

        if (await repository.GroupHasCoveragesAsync(id, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Cannot delete a group that has coverages." };
        }

        await repository.DeleteGroupAsync(group, cancellationToken);
        await AuditAsync(actorUserId, "CATALOG.GROUP_DELETED", group.Id, ip, ua, cancellationToken);
        return new OperationResponse { Success = true, Message = "Coverage group deleted." };
    }

    private static CoverageGroupResponse Map(CoverageGroup g) =>
        new(g.Id, g.Code, g.Name, g.SortOrder, g.IsActive);

    private async Task AuditAsync(Guid? actorUserId, string action, Guid entityId, string? ip, string? ua, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "coverage_groups",
            EntityId = entityId,
            IpAddress = ip,
            UserAgent = ua
        }, cancellationToken);
    }
}
