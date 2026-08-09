using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("plans")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "PLAN.MANAGE")]
public class PlansController(IPlanService planService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PlanSummaryResponse>>> GetAll(
        [FromQuery] Guid? companyId, [FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var plans = await planService.GetAllAsync(companyId, isActive, cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlanResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var plan = await planService.GetByIdAsync(id, cancellationToken);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<PlanResponse>> Create([FromBody] CreatePlanRequest request, CancellationToken cancellationToken)
    {
        var (plan, result) = await planService.CreateAsync(request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || plan is null)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlanResponse>> Update(Guid id, [FromBody] UpdatePlanRequest request, CancellationToken cancellationToken)
    {
        var (plan, result) = await planService.UpdateAsync(id, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!result.Success || plan is null)
        {
            return result.Message == "Plan not found." ? NotFound(result) : BadRequest(result);
        }

        return Ok(plan);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OperationResponse>> SetStatus(Guid id, [FromBody] UpdatePlanStatusRequest request, CancellationToken cancellationToken)
    {
        var response = await planService.SetStatusAsync(id, request.IsActive, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<OperationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await planService.DeleteAsync(id, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
