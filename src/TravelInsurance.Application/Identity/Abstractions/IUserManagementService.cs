using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Identity.DTOs;

namespace TravelInsurance.Application.Identity.Abstractions;

public interface IUserManagementService
{
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<OperationResponse> AssignRoleAsync(Guid userId, AssignRoleRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> RemoveRoleAsync(Guid userId, int roleId, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResponse> AssignCompanyAsync(Guid userId, AssignCompanyRequest request, Guid? actorUserId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
}
