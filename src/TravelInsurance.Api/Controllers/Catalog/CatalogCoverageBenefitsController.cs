using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Api.Controllers.Catalog;

[ApiController]
[Route("catalog/coverages/{coverageId:guid}/benefits")]
[Authorize]
public class CatalogCoverageBenefitsController(ICoverageBenefitMappingService mappingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CoverageBenefitResponse>>> GetAll(Guid coverageId, CancellationToken cancellationToken)
    {
        var mappings = await mappingService.GetByCoverageAsync(coverageId, cancellationToken);
        return Ok(mappings);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> Add(Guid coverageId, [FromBody] AddCoverageBenefitRequest request, CancellationToken cancellationToken)
    {
        var response = await mappingService.AddAsync(coverageId, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{benefitId:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> Update(Guid coverageId, Guid benefitId, [FromBody] UpdateCoverageBenefitRequest request, CancellationToken cancellationToken)
    {
        var response = await mappingService.UpdateAsync(coverageId, benefitId, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!response.Success)
        {
            return response.Message is "Coverage not found." or "Benefit mapping not found." ? NotFound(response) : BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{benefitId:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> Remove(Guid coverageId, Guid benefitId, CancellationToken cancellationToken)
    {
        var response = await mappingService.RemoveAsync(coverageId, benefitId, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
