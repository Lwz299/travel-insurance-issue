using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("reinsurance/bordereaux")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "PLAN.MANAGE")]
public class ReinsuranceBordereauxController(IReinsuranceBordereauService bordereauService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReinsuranceBordereauResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var bordereau = await bordereauService.GetByIdAsync(id, cancellationToken);
        return bordereau is null ? NotFound() : Ok(bordereau);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ReinsuranceBordereauResponse>> Generate(GenerateBordereauRequest request, CancellationToken cancellationToken)
    {
        var (bordereau, result) = await bordereauService.GenerateAsync(request, cancellationToken);
        return !result.Success || bordereau is null ? BadRequest(result) : Ok(bordereau);
    }
}
