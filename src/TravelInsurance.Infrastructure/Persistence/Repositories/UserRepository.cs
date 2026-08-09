using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdWithAuthGraphAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddUserRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
        if (!exists)
        {
            dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
