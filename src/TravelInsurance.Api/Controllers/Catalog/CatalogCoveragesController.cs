using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Api.Controllers.Catalog;

[ApiController]
[Route("catalog/coverages")]
[Authorize]
public class CatalogCoveragesController(ICoverageCatalogService coverageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CoverageResponse>>> GetAll([FromQuery] Guid? groupId, CancellationToken cancellationToken)
    {
        var coverages = await coverageService.GetAllAsync(groupId, cancellationToken);
        return Ok(coverages);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CoverageDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var coverage = await coverageService.GetByIdAsync(id, cancellationToken);
        return coverage is null ? NotFound() : Ok(coverage);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<CoverageResponse>> Create([FromBody] CreateCoverageRequest request, CancellationToken cancellationToken)
    {
        var (coverage, result) = await coverageService.CreateAsync(request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || coverage is null)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = coverage.Id }, coverage);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<CoverageResponse>> Update(Guid id, [FromBody] UpdateCoverageRequest request, CancellationToken cancellationToken)
    {
        var (coverage, result) = await coverageService.UpdateAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || coverage is null)
        {
            return result.Message == "Coverage not found." ? NotFound(result) : BadRequest(result);
        }

        return Ok(coverage);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> SetStatus(Guid id, [FromBody] UpdateCoverageStatusRequest request, CancellationToken cancellationToken)
    {
        var response = await coverageService.SetStatusAsync(id, request.IsActive, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await coverageService.DeleteAsync(id, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
