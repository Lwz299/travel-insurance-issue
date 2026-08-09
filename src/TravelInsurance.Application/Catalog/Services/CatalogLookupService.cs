using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Application.Catalog.Services;

public class CatalogLookupService(ICatalogRepository repository) : ICatalogLookupService
{
    public async Task<IReadOnlyCollection<LookupItemResponse>> GetBenefitTypesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetBenefitTypesAsync(cancellationToken);
        return items.Select(x => new LookupItemResponse(x.Id, x.Code, x.Name)).ToArray();
    }

    public async Task<IReadOnlyCollection<LookupItemResponse>> GetLimitTypesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetLimitTypesAsync(cancellationToken);
        return items.Select(x => new LookupItemResponse(x.Id, x.Code, x.Name)).ToArray();
    }

    public async Task<IReadOnlyCollection<LookupItemResponse>> GetRiskTypesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetRiskTypesAsync(cancellationToken);
        return items.Select(x => new LookupItemResponse(x.Id, x.Code, x.Name)).ToArray();
    }
}
