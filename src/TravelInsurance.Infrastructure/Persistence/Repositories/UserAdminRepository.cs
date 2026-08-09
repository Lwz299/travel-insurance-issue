using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Identity.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class UserAdminRepository(AppDbContext dbContext) : IUserAdminRepository
{
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .Include(u => u.Company)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(u => u.Company)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Roles.AsNoTracking().OrderBy(r => r.Id).ToListAsync(cancellationToken);
    }

    public Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return dbContext.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);
    }

    public Task<bool> CompanyExistsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Companies.AnyAsync(c => c.Id == companyId, cancellationToken);
    }

    public Task<bool> HasRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        return dbContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

    public async Task AddRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
        if (userRole is not null)
        {
            dbContext.UserRoles.Remove(userRole);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SetCompanyAsync(User user, Guid? companyId, CancellationToken cancellationToken = default)
    {
        user.CompanyId = companyId;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
