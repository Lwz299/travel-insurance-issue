using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("reinsurance/cessions")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "PLAN.MANAGE")]
public class ReinsuranceCessionsController(IReinsuranceCessionService cessionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReinsuranceCessionResponse>>> GetAll([FromQuery] Guid? policyId, [FromQuery] Guid? agreementId, CancellationToken cancellationToken) =>
        Ok(await cessionService.GetAllAsync(policyId, agreementId, cancellationToken));
}
