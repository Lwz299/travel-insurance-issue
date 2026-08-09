using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Application.Plans.Services;

public class CoverageZoneService(ICoverageZoneRepository coverageZoneRepository, ICountryRepository countryRepository) : ICoverageZoneService
{
    public async Task<IReadOnlyCollection<CoverageZoneSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var zones = await coverageZoneRepository.GetAllAsync(cancellationToken);
        return zones.Select(z => new CoverageZoneSummaryResponse(z.Id, z.Code, z.Name, z.IsWorldwide, z.IsActive)).ToArray();
    }

    public async Task<CoverageZoneResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var zone = await coverageZoneRepository.GetByIdAsync(id, cancellationToken);
        return zone is null ? null : Map(zone);
    }

    public async Task<(CoverageZoneResponse? Zone, OperationResponse Result)> CreateAsync(CreateCoverageZoneRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(request.Code, request.Name, request.IsWorldwide, request.Countries, cancellationToken);
        if (!validation.Success) return (null, validation);

        var zone = new CoverageZone { Code = request.Code.Trim().ToUpperInvariant(), Name = request.Name.Trim(), IsWorldwide = request.IsWorldwide };
        ApplyCountries(zone, request.Countries);
        await coverageZoneRepository.AddAsync(zone, cancellationToken);
        return (Map(zone), new OperationResponse { Success = true, Message = "Coverage zone created." });
    }

    public async Task<(CoverageZoneResponse? Zone, OperationResponse Result)> UpdateAsync(Guid id, UpdateCoverageZoneRequest request, CancellationToken cancellationToken = default)
    {
        var zone = await coverageZoneRepository.GetByIdAsync(id, cancellationToken);
        if (zone is null) return (null, new OperationResponse { Success = false, Message = "Coverage zone not found." });

        var validation = await ValidateAsync(request.Code, request.Name, request.IsWorldwide, request.Countries, cancellationToken, id);
        if (!validation.Success) return (null, validation);

        zone.Code = request.Code.Trim().ToUpperInvariant();
        zone.Name = request.Name.Trim();
        zone.IsWorldwide = request.IsWorldwide;
        zone.IsActive = request.IsActive;
        zone.Countries.Clear();
        ApplyCountries(zone, request.Countries);
        await coverageZoneRepository.UpdateAsync(zone, cancellationToken);
        return (Map(zone), new OperationResponse { Success = true, Message = "Coverage zone updated." });
    }

    private async Task<OperationResponse> ValidateAsync(string code, string name, bool isWorldwide, List<CoverageZoneCountryInput> countries, CancellationToken cancellationToken, Guid? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) return new OperationResponse { Success = false, Message = "Coverage zone code and name are required." };
        if (!isWorldwide && countries.Count == 0) return new OperationResponse { Success = false, Message = "A regional coverage zone must include at least one country." };
        if (isWorldwide && countries.Any(c => !c.IsExcluded)) return new OperationResponse { Success = false, Message = "Worldwide zones can only list excluded countries." };

        var countryIds = countries.Select(c => c.CountryId).ToArray();
        if (countryIds.Length != countryIds.Distinct().Count()) return new OperationResponse { Success = false, Message = "Duplicate countries are not allowed in a coverage zone." };
        var existingCountries = await countryRepository.GetExistingIdsAsync(countryIds, cancellationToken);
        if (existingCountries.Count != countryIds.Distinct().Count()) return new OperationResponse { Success = false, Message = "One or more countries do not exist." };

        var matchingCode = (await coverageZoneRepository.GetAllAsync(cancellationToken))
            .Any(z => z.Id != currentId && string.Equals(z.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
        return matchingCode
            ? new OperationResponse { Success = false, Message = "Coverage zone code already exists." }
            : new OperationResponse { Success = true, Message = "Valid." };
    }

    private static void ApplyCountries(CoverageZone zone, IEnumerable<CoverageZoneCountryInput> countries)
    {
        foreach (var country in countries)
            zone.Countries.Add(new CoverageZoneCountry { CountryId = country.CountryId, IsExcluded = country.IsExcluded });
    }

    private static CoverageZoneResponse Map(CoverageZone zone) => new(zone.Id, zone.Code, zone.Name, zone.IsWorldwide, zone.IsActive,
        zone.Countries.OrderBy(c => c.Country?.Name).Select(c => new CoverageZoneCountryResponse(c.CountryId, c.Country?.Code ?? string.Empty, c.Country?.Name ?? string.Empty, c.IsExcluded)).ToArray());
}
