using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Premium.Abstractions;
using TravelInsurance.Application.Premium.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("premium")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "QUOTE.CREATE")]
public class PremiumController(IPremiumCalculationService premiumCalculationService) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<PremiumCalculationResponse>> Calculate([FromBody] CalculatePremiumRequest request, CancellationToken cancellationToken)
    {
        var result = await premiumCalculationService.CalculateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
