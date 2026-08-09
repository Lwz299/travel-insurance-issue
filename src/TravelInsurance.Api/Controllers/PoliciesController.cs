using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.PolicyManagement.Abstractions;
using TravelInsurance.Application.PolicyManagement.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("policies")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "POLICY.READ")]
public class PoliciesController(IPolicyManagementService policyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PolicySummaryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        return Ok(await policyService.GetAllAsync(userId.Value, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PolicyResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (policy, result) = await policyService.GetByIdAsync(id, userId.Value, cancellationToken);
        return result.Success && policy is not null ? Ok(policy) : NotFound(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "POLICY.CANCEL")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelPolicyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var result = await policyService.CancelAsync(id, request, userId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/expire")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "POLICY.EXPIRE")]
    public async Task<IActionResult> Expire(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var result = await policyService.ExpireAsync(id, userId.Value, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
