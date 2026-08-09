using TravelInsurance.Application.Authentication.Abstractions;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
