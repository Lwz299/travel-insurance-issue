using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelInsurance.Api.Authorization;
using TravelInsurance.Application.Reinsurance.Abstractions;
using TravelInsurance.Application.Reinsurance.DTOs;

namespace TravelInsurance.Api.Controllers;

[ApiController]
[Route("reinsurance/agreements")]
[Authorize(Policy = PermissionPolicyProvider.Prefix + "PLAN.MANAGE")]
public class ReinsuranceAgreementsController(IReinsuranceAgreementService agreementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReinsuranceAgreementResponse>>> GetAll([FromQuery] Guid? companyId, CancellationToken cancellationToken) =>
        Ok(await agreementService.GetAllAsync(companyId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReinsuranceAgreementResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var agreement = await agreementService.GetByIdAsync(id, cancellationToken);
        return agreement is null ? NotFound() : Ok(agreement);
    }

    [HttpPost]
    public async Task<ActionResult<ReinsuranceAgreementResponse>> Create(CreateReinsuranceAgreementRequest request, CancellationToken cancellationToken)
    {
        var (agreement, result) = await agreementService.CreateAsync(request, cancellationToken);
        return !result.Success || agreement is null ? BadRequest(result) : CreatedAtAction(nameof(GetById), new { id = agreement.Id }, agreement);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReinsuranceAgreementResponse>> Update(Guid id, UpdateReinsuranceAgreementRequest request, CancellationToken cancellationToken)
    {
        var (agreement, result) = await agreementService.UpdateAsync(id, request, cancellationToken);
        if (!result.Success || agreement is null) return result.Message == "Reinsurance agreement not found." ? NotFound(result) : BadRequest(result);
        return Ok(agreement);
    }
}
