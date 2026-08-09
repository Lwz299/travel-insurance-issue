using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("coverage-zones")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "PLAN.MANAGE")]
public class CoverageZonesController(ICoverageZoneService coverageZoneService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CoverageZoneSummaryResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await coverageZoneService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CoverageZoneResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var zone = await coverageZoneService.GetByIdAsync(id, cancellationToken);
        return zone is null ? NotFound() : Ok(zone);
    }

    [HttpPost]
    public async Task<ActionResult<CoverageZoneResponse>> Create(CreateCoverageZoneRequest request, CancellationToken cancellationToken)
    {
        var (zone, result) = await coverageZoneService.CreateAsync(request, cancellationToken);
        return !result.Success || zone is null ? BadRequest(result) : CreatedAtAction(nameof(GetById), new { id = zone.Id }, zone);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CoverageZoneResponse>> Update(Guid id, UpdateCoverageZoneRequest request, CancellationToken cancellationToken)
    {
        var (zone, result) = await coverageZoneService.UpdateAsync(id, request, cancellationToken);
        if (!result.Success || zone is null) return result.Message == "Coverage zone not found." ? NotFound(result) : BadRequest(result);
        return Ok(zone);
    }
}
