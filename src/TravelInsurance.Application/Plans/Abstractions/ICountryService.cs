using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Application.Plans.Abstractions;

public interface ICountryService
{
    Task<IReadOnlyCollection<CountryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
