using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Identity.Abstractions;
using TravelInsurance.Application.Identity.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Identity.Services;

public class UserManagementService(
    IUserAdminRepository userAdminRepository,
    IAuditLogRepository auditLogRepository) : IUserManagementService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userAdminRepository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToArray();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userAdminRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await userAdminRepository.GetRolesAsync(cancellationToken);
        return roles.Select(r => new RoleDto(r.Id, r.Name)).ToArray();
    }

    public async Task<OperationResponse> AssignRoleAsync(Guid userId, AssignRoleRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userAdminRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return new OperationResponse { Success = false, Message = "User not found." };
        }

        if (!await userAdminRepository.RoleExistsAsync(request.RoleId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Role not found." };
        }

        if (await userAdminRepository.HasRoleAsync(userId, request.RoleId, cancellationToken))
        {
            return new OperationResponse { Success = true, Message = "User already has this role." };
        }

        await userAdminRepository.AddRoleAsync(userId, request.RoleId, cancellationToken);
        await WriteAuditAsync(actorUserId, "USER.ROLE_ASSIGNED", userId, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Role assigned." };
    }

    public async Task<OperationResponse> RemoveRoleAsync(Guid userId, int roleId, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userAdminRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return new OperationResponse { Success = false, Message = "User not found." };
        }

        if (!await userAdminRepository.HasRoleAsync(userId, roleId, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "User does not have this role." };
        }

        await userAdminRepository.RemoveRoleAsync(userId, roleId, cancellationToken);
        await WriteAuditAsync(actorUserId, "USER.ROLE_REMOVED", userId, ipAddress, userAgent, cancellationToken);
        return new OperationResponse { Success = true, Message = "Role removed." };
    }

    public async Task<OperationResponse> AssignCompanyAsync(Guid userId, AssignCompanyRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userAdminRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return new OperationResponse { Success = false, Message = "User not found." };
        }

        if (request.CompanyId.HasValue && !await userAdminRepository.CompanyExistsAsync(request.CompanyId.Value, cancellationToken))
        {
            return new OperationResponse { Success = false, Message = "Company not found." };
        }

        await userAdminRepository.SetCompanyAsync(user, request.CompanyId, cancellationToken);
        await WriteAuditAsync(actorUserId, "USER.COMPANY_ASSIGNED", userId, ipAddress, userAgent, cancellationToken);
        return new OperationResponse
        {
            Success = true,
            Message = request.CompanyId.HasValue ? "Company assigned." : "Company unassigned."
        };
    }

    private async Task WriteAuditAsync(Guid? actorUserId, string action, Guid entityId, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityType = "users",
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);
    }

    private static UserResponse Map(User u) => new(
        u.Id,
        u.FullName,
        u.Email,
        u.Phone,
        u.Status.ToString(),
        u.EmailVerified,
        u.CompanyId,
        u.Company?.Name,
        u.LastLoginAt,
        u.CreatedAt,
        u.UserRoles.Select(ur => new RoleDto(ur.Role.Id, ur.Role.Name)).ToArray());
}
