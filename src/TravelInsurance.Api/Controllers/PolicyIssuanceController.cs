using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.PolicyIssuance.Abstractions;
using TravelInsurance.Application.PolicyIssuance.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("policy-issuance")]
[Authorize]
public class PolicyIssuanceController(IPolicyIssuanceService policyIssuanceService) : ControllerBase
{
    [HttpPost("from-quote")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "POLICY.ISSUE_FROM_QUOTE")]
    public async Task<ActionResult<IssuanceResponse>> FromQuote([FromBody] IssueFromQuoteRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (issuance, result) = await policyIssuanceService.IssueFromQuoteAsync(request, userId.Value, idempotencyKey ?? string.Empty, cancellationToken);
        return result.Success && issuance is not null ? Ok(issuance) : result.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase) ? Conflict(result) : BadRequest(result);
    }

    [HttpPost("direct")]
    [Authorize(Policy = PermissionPolicyProvider.Prefix + "POLICY.ISSUE_DIRECT")]
    public async Task<ActionResult<IssuanceResponse>> Direct([FromBody] DirectIssueRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (issuance, result) = await policyIssuanceService.IssueDirectAsync(request, userId.Value, idempotencyKey ?? string.Empty, cancellationToken);
        return result.Success && issuance is not null ? Ok(issuance) : result.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase) ? Conflict(result) : BadRequest(result);
    }

    [HttpGet("{requestId:guid}/status")]
    public async Task<ActionResult<IssuanceResponse>> GetStatus(Guid requestId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); if (userId is null) return Unauthorized();
        var (issuance, result) = await policyIssuanceService.GetStatusAsync(requestId, userId.Value, cancellationToken);
        return result.Success && issuance is not null ? Ok(issuance) : NotFound(result);
    }
}
