using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Catalog.Abstractions;
using TravelInsurance.Application.Catalog.DTOs;

namespace TravelInsurance.Api.Controllers.Catalog;

[ApiController]
[Route("catalog/benefits")]
[Authorize]
public class CatalogBenefitsController(IBenefitCatalogService benefitService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<BenefitResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var benefits = await benefitService.GetAllAsync(cancellationToken);
        return Ok(benefits);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BenefitResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var benefit = await benefitService.GetByIdAsync(id, cancellationToken);
        return benefit is null ? NotFound() : Ok(benefit);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<BenefitResponse>> Create([FromBody] CreateBenefitRequest request, CancellationToken cancellationToken)
    {
        var (benefit, result) = await benefitService.CreateAsync(request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || benefit is null)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = benefit.Id }, benefit);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<BenefitResponse>> Update(Guid id, [FromBody] UpdateBenefitRequest request, CancellationToken cancellationToken)
    {
        var (benefit, result) = await benefitService.UpdateAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || benefit is null)
        {
            return result.Message == "Benefit not found." ? NotFound(result) : BadRequest(result);
        }

        return Ok(benefit);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> SetStatus(Guid id, [FromBody] UpdateBenefitStatusRequest request, CancellationToken cancellationToken)
    {
        var response = await benefitService.SetStatusAsync(id, request.IsActive, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "CATALOG.MANAGE")]
    public async Task<ActionResult<OperationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await benefitService.DeleteAsync(id, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
