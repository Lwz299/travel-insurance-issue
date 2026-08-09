using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Identity.Abstractions;

public interface IUserAdminRepository
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> CompanyExistsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
    Task AddRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
    Task RemoveRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
    Task SetCompanyAsync(User user, Guid? companyId, CancellationToken cancellationToken = default);
}
