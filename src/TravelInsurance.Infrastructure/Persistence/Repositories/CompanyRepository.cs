using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Organization.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class CompanyRepository(AppDbContext dbContext) : ICompanyRepository
{
    public Task<List<Company>> GetAllAsync(CompanyType? type, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Companies.AsNoTracking().AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        return query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Companies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        company.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Companies.Update(company);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasUsersAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(x => x.CompanyId == companyId, cancellationToken);
    }

    public async Task DeleteAsync(Company company, CancellationToken cancellationToken = default)
    {
        dbContext.Companies.Remove(company);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
