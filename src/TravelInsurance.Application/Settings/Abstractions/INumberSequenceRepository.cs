using TravelInsurance.Domain.Entities;
using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Settings.Abstractions;

public interface INumberSequenceRepository
{
    Task<NumberSequence?> GetAsync(Guid companyId, NumberSequenceType type, CancellationToken cancellationToken = default);
    void Add(NumberSequence sequence);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
