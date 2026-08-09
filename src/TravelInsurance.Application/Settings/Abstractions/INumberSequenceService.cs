using TravelInsurance.Domain.Enums;

namespace TravelInsurance.Application.Settings.Abstractions;

public interface INumberSequenceService
{
    Task<string> NextAsync(Guid companyId, NumberSequenceType type, CancellationToken cancellationToken = default);
}
