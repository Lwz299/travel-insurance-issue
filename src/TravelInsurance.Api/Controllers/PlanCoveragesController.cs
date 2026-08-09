using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Authentication.DTOs;
using TravelInsurance.Application.Plans.Abstractions;
using TravelInsurance.Application.Plans.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("plans/{planId:guid}/coverages")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "PLAN.MANAGE")]
public class PlanCoveragesController(IPlanCoverageService planCoverageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PlanCoverageAssignmentResponse>>> GetAll(Guid planId, CancellationToken cancellationToken)
    {
        var assignments = await planCoverageService.GetByPlanAsync(planId, cancellationToken);
        return Ok(assignments);
    }

    [HttpPost]
    public async Task<ActionResult<OperationResponse>> Assign(Guid planId, [FromBody] AssignPlanCoverageRequest request, CancellationToken cancellationToken)
    {
        var response = await planCoverageService.AssignAsync(planId, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{coverageId:guid}")]
    public async Task<ActionResult<OperationResponse>> Update(Guid planId, Guid coverageId, [FromBody] UpdatePlanCoverageRequest request, CancellationToken cancellationToken)
    {
        var response = await planCoverageService.UpdateAsync(planId, coverageId, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!response.Success)
        {
            return response.Message is "Plan not found." or "Coverage assignment not found." ? NotFound(response) : BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{coverageId:guid}")]
    public async Task<ActionResult<OperationResponse>> Remove(Guid planId, Guid coverageId, CancellationToken cancellationToken)
    {
        var response = await planCoverageService.RemoveAsync(planId, coverageId, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut("{coverageId:guid}/benefits/{benefitId:guid}")]
    public async Task<ActionResult<OperationResponse>> UpsertBenefitOverride(
        Guid planId, Guid coverageId, Guid benefitId,
        [FromBody] UpsertPlanBenefitOverrideRequest request, CancellationToken cancellationToken)
    {
        request.BenefitId = benefitId;
        var response = await planCoverageService.UpsertBenefitOverrideAsync(planId, coverageId, request, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        if (!response.Success)
        {
            return response.Message is "Plan not found." or "Coverage assignment not found." ? NotFound(response) : BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{coverageId:guid}/benefits/{benefitId:guid}")]
    public async Task<ActionResult<OperationResponse>> RemoveBenefitOverride(Guid planId, Guid coverageId, Guid benefitId, CancellationToken cancellationToken)
    {
        var response = await planCoverageService.RemoveBenefitOverrideAsync(planId, coverageId, benefitId, User.GetUserId(), ClientIp(), UserAgent(), cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? UserAgent() => Request.Headers.UserAgent;
}
