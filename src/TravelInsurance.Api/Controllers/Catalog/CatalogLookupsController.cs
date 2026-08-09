using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Api.Controllers.Catalog;

[ApiController]
[Route("catalog/lookups")]
[Authorize]
public class CatalogLookupsController(ICatalogLookupService lookupService) : ControllerBase
{
    [HttpGet("benefit-types")]
    public async Task<ActionResult<IReadOnlyCollection<LookupItemResponse>>> GetBenefitTypes(CancellationToken cancellationToken)
        => Ok(await lookupService.GetBenefitTypesAsync(cancellationToken));

    [HttpGet("limit-types")]
    public async Task<ActionResult<IReadOnlyCollection<LookupItemResponse>>> GetLimitTypes(CancellationToken cancellationToken)
        => Ok(await lookupService.GetLimitTypesAsync(cancellationToken));

    [HttpGet("risk-types")]
    public async Task<ActionResult<IReadOnlyCollection<LookupItemResponse>>> GetRiskTypes(CancellationToken cancellationToken)
        => Ok(await lookupService.GetRiskTypesAsync(cancellationToken));
}
