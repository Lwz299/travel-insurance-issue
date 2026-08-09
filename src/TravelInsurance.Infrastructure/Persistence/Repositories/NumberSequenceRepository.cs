using Microsoft.EntityFrameworkCore;
using TravelInsurance.Application.Settings.Abstractions;
using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Infrastructure.Persistence.Repositories;

public class NumberSequenceRepository(AppDbContext dbContext) : INumberSequenceRepository
{
    public Task<NumberSequence?> GetAsync(Guid companyId, NumberSequenceType type, CancellationToken cancellationToken = default) => dbContext.Set<NumberSequence>().FirstOrDefaultAsync(x => x.CompanyId == companyId && x.SequenceType == type, cancellationToken);
    public void Add(NumberSequence sequence) => dbContext.Set<NumberSequence>().Add(sequence);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
