using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Application.Plans.Services;

public class CountryService(ICountryRepository countryRepository) : ICountryService
{
    public async Task<IReadOnlyCollection<CountryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.GetAllAsync(cancellationToken);
        return countries.Select(c => new CountryResponse(c.Id, c.Code, c.Name)).ToArray();
    }
}
